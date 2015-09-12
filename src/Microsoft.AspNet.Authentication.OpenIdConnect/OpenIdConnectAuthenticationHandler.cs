// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.Framework.Caching.Distributed;
using Microsoft.Framework.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.Authentication.OpenIdConnect
{
    /// <summary>
    /// A per-request authentication handler for the OpenIdConnectAuthenticationMiddleware.
    /// </summary>
    public class OpenIdConnectAuthenticationHandler : AuthenticationHandler<OpenIdConnectAuthenticationOptions>
    {
        private const string NonceProperty = "N";
        private const string UriSchemeDelimiter = "://";

        private const string InputTagFormat = @"<input type=""hidden"" name=""{0}"" value=""{1}"" />";
        private const string HtmlFormFormat = @"<!doctype html>
<html>
<head>
    <title>Please wait while you're being redirected to the identity provider</title>
</head>
<body>
    <form name=""form"" method=""post"" action=""{0}"">
        {1}
        <noscript>Click here to finish the process: <input type=""submit"" /></noscript>
    </form>
    <script>document.form.submit();</script>
</body>
</html>";

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        private OpenIdConnectConfiguration _configuration;

        protected HttpClient Backchannel { get; private set; }

        public OpenIdConnectAuthenticationHandler(HttpClient backchannel)
        {
            Backchannel = backchannel;
        }

        /// <summary>
        /// Handles Signout
        /// </summary>
        /// <returns></returns>
        protected override async Task HandleSignOutAsync(SignOutContext signout)
        {
            if (signout != null)
            {
                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                var message = new OpenIdConnectMessage()
                {
                    IssuerAddress = _configuration == null ? string.Empty : (_configuration.EndSessionEndpoint ?? string.Empty),
                    RequestType = OpenIdConnectRequestType.LogoutRequest,
                };

                // Set End_Session_Endpoint in order:
                // 1. properties.Redirect
                // 2. Options.PostLogoutRedirectUri
                var properties = new AuthenticationProperties(signout.Properties);
                if (!string.IsNullOrEmpty(properties.RedirectUri))
                {
                    message.PostLogoutRedirectUri = properties.RedirectUri;
                }
                else if (!string.IsNullOrEmpty(Options.PostLogoutRedirectUri))
                {
                    message.PostLogoutRedirectUri = Options.PostLogoutRedirectUri;
                }

                var redirectToIdentityProviderContext = new RedirectToIdentityProviderContext(Context, Options)
                {
                    ProtocolMessage = message
                };

                await Options.Events.RedirectToIdentityProvider(redirectToIdentityProviderContext);
                if (redirectToIdentityProviderContext.HandledResponse)
                {
                    Logger.LogVerbose(Resources.OIDCH_0034_RedirectToIdentityProviderContextHandledResponse);
                    return;
                }
                else if (redirectToIdentityProviderContext.Skipped)
                {
                    Logger.LogVerbose(Resources.OIDCH_0035_RedirectToIdentityProviderContextSkipped);
                    return;
                }

                message = redirectToIdentityProviderContext.ProtocolMessage;

                if (Options.AuthenticationMethod == OpenIdConnectAuthenticationMethod.RedirectGet)
                {
                    var redirectUri = message.CreateLogoutRequestUrl();
                    if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                    {
                        Logger.LogWarning(Resources.OIDCH_0051_RedirectUriLogoutIsNotWellFormed, redirectUri);
                    }

                    Response.Redirect(redirectUri);
                }
                else if (Options.AuthenticationMethod == OpenIdConnectAuthenticationMethod.FormPost)
                {
                    var inputs = new StringBuilder();
                    foreach (var parameter in message.Parameters)
                    {
                        var name = Options.HtmlEncoder.HtmlEncode(parameter.Key);
                        var value = Options.HtmlEncoder.HtmlEncode(parameter.Value);

                        var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                        inputs.AppendLine(input);
                    }

                    var issuer = Options.HtmlEncoder.HtmlEncode(message.IssuerAddress);

                    var content = string.Format(CultureInfo.InvariantCulture, HtmlFormFormat, issuer, inputs);
                    var buffer = Encoding.UTF8.GetBytes(content);

                    Response.ContentLength = buffer.Length;
                    Response.ContentType = "text/html;charset=UTF-8";

                    // Emit Cache-Control=no-cache to prevent client caching.
                    Response.Headers[HeaderNames.CacheControl] = "no-cache";
                    Response.Headers[HeaderNames.Pragma] = "no-cache";
                    Response.Headers[HeaderNames.Expires] = "-1";

                    await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Responds to a 401 Challenge. Sends an OpenIdConnect message to the 'identity authority' to obtain an identity.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Uses log id's OIDCH-0026 - OIDCH-0050, next num: 37</remarks>
        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Logger.LogDebug(Resources.OIDCH_0026_ApplyResponseChallengeAsync, this.GetType());

            // order for local RedirectUri
            // 1. challenge.Properties.RedirectUri
            // 2. CurrentUri if Options.DefaultToCurrentUriOnRedirect is true)
            AuthenticationProperties properties = new AuthenticationProperties(context.Properties);

            if (!string.IsNullOrEmpty(properties.RedirectUri))
            {
                Logger.LogDebug(Resources.OIDCH_0030_Using_Properties_RedirectUri, properties.RedirectUri);
            }
            else if (Options.DefaultToCurrentUriOnRedirect)
            {
                Logger.LogDebug(Resources.OIDCH_0032_UsingCurrentUriRedirectUri, CurrentUri);
                properties.RedirectUri = CurrentUri;
            }

            if (_configuration == null && Options.ConfigurationManager != null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
            }

            var message = new OpenIdConnectMessage
            {
                ClientId = Options.ClientId,
                IssuerAddress = _configuration?.AuthorizationEndpoint ?? string.Empty,
                RedirectUri = Options.RedirectUri,
                // [brentschmaltz] - #215 this should be a property on RedirectToIdentityProviderContext not on the OIDCMessage.
                RequestType = OpenIdConnectRequestType.AuthenticationRequest,
                Resource = Options.Resource,
                ResponseType = Options.ResponseType,
                Scope = string.Join(" ", Options.Scope)
            };

            // Omitting the response_mode parameter when it already corresponds to the default
            // response_mode used for the specified response_type is recommended by the specifications.
            // See http://openid.net/specs/oauth-v2-multiple-response-types-1_0.html#ResponseModes
            if (!string.Equals(Options.ResponseType, OpenIdConnectResponseTypes.Code, StringComparison.Ordinal) ||
                !string.Equals(Options.ResponseMode, OpenIdConnectResponseModes.Query, StringComparison.Ordinal))
            {
                message.ResponseMode = Options.ResponseMode;
            }

            if (Options.ProtocolValidator.RequireNonce)
            {
                message.Nonce = Options.ProtocolValidator.GenerateNonce();
                if (Options.CacheNonces)
                {
                    if (await Options.NonceCache.GetAsync(message.Nonce) != null)
                    {
                        Logger.LogError(Resources.OIDCH_0033_NonceAlreadyExists, message.Nonce);
                        throw new OpenIdConnectProtocolException(string.Format(CultureInfo.InvariantCulture, Resources.OIDCH_0033_NonceAlreadyExists, message.Nonce));
                    }

                    await Options.NonceCache.SetAsync(message.Nonce, new byte[0], new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = Options.ProtocolValidator.NonceLifetime
                    });
                }
                else
                {
                    WriteNonceCookie(message.Nonce);
                }
            }

            GenerateCorrelationId(properties);

            var redirectToIdentityProviderContext = new RedirectToIdentityProviderContext(Context, Options)
            {
                ProtocolMessage = message
            };

            await Options.Events.RedirectToIdentityProvider(redirectToIdentityProviderContext);
            if (redirectToIdentityProviderContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0034_RedirectToIdentityProviderContextHandledResponse);
                return true;
            }
            else if (redirectToIdentityProviderContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0035_RedirectToIdentityProviderContextSkipped);
                return false;
            }

            if (!string.IsNullOrEmpty(redirectToIdentityProviderContext.ProtocolMessage.State))
            {
                properties.Items[OpenIdConnectAuthenticationDefaults.UserstatePropertiesKey] = redirectToIdentityProviderContext.ProtocolMessage.State;
            }

            message = redirectToIdentityProviderContext.ProtocolMessage;

            var redirectUriForCode = message.RedirectUri;
            if (string.IsNullOrEmpty(redirectUriForCode))
            {
                Logger.LogDebug(Resources.OIDCH_0031_Using_Options_RedirectUri, Options.RedirectUri);
                redirectUriForCode = Options.RedirectUri;
            }

            if (!string.IsNullOrEmpty(redirectUriForCode))
            {
                // When redeeming a 'code' for an AccessToken, this value is needed
                properties.Items.Add(OpenIdConnectAuthenticationDefaults.RedirectUriForCodePropertiesKey, redirectUriForCode);
            }

            message.State = Options.StateDataFormat.Protect(properties);

            if (Options.AuthenticationMethod == OpenIdConnectAuthenticationMethod.RedirectGet)
            {
                var redirectUri = message.CreateAuthenticationRequestUrl();
                if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    Logger.LogWarning(Resources.OIDCH_0036_UriIsNotWellFormed, redirectUri);
                }

                Response.Redirect(redirectUri);

                return true;
            }
            else if (Options.AuthenticationMethod == OpenIdConnectAuthenticationMethod.FormPost)
            {
                var inputs = new StringBuilder();
                foreach (var parameter in message.Parameters)
                {
                    var name = Options.HtmlEncoder.HtmlEncode(parameter.Key);
                    var value = Options.HtmlEncoder.HtmlEncode(parameter.Value);

                    var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                    inputs.AppendLine(input);
                }

                var issuer = Options.HtmlEncoder.HtmlEncode(message.IssuerAddress);

                var content = string.Format(CultureInfo.InvariantCulture, HtmlFormFormat, issuer, inputs);
                var buffer = Encoding.UTF8.GetBytes(content);

                Response.ContentLength = buffer.Length;
                Response.ContentType = "text/html;charset=UTF-8";

                // Emit Cache-Control=no-cache to prevent client caching.
                Response.Headers[HeaderNames.CacheControl] = "no-cache";
                Response.Headers[HeaderNames.Pragma] = "no-cache";
                Response.Headers[HeaderNames.Expires] = "-1";

                await Response.Body.WriteAsync(buffer, 0, buffer.Length);

                return true;
            }

            Logger.LogError("An unsupported authentication method has been configured: {0}", Options.AuthenticationMethod);
            return false;
        }

        /// <summary>
        /// Invoked to process incoming OpenIdConnect messages.
        /// </summary>
        /// <returns>An <see cref="AuthenticationTicket"/> if successful.</returns>
        /// <remarks>Uses log id's OIDCH-0000 - OIDCH-0025</remarks>
        protected override async Task<AuthenticationTicket> HandleAuthenticateAsync()
        {
            Logger.LogDebug(Resources.OIDCH_0000_AuthenticateCoreAsync, this.GetType());

            // Allow login to be constrained to a specific path. Need to make this runtime configurable.
            if (Options.CallbackPath.HasValue && Options.CallbackPath != (Request.PathBase + Request.Path))
            {
                return null;
            }

            OpenIdConnectMessage message = null;

            if (string.Equals(Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                message = new OpenIdConnectMessage(Request.Query.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));

                // response_mode=query (explicit or not) and a response_type containing id_token
                // or token are not considered as a safe combination and MUST be rejected.
                // See http://openid.net/specs/oauth-v2-multiple-response-types-1_0.html#Security
                if (!string.IsNullOrEmpty(message.IdToken) || !string.IsNullOrEmpty(message.Token))
                {
                    Logger.LogError("An OpenID Connect response cannot contain an identity token " +
                                    "or an access token when using response_mode=query");
                    return null;
                }
            }
            // assumption: if the ContentType is "application/x-www-form-urlencoded" it should be safe to read as it is small.
            else if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
              && !string.IsNullOrEmpty(Request.ContentType)
              // May have media/type; charset=utf-8, allow partial match.
              && Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
              && Request.Body.CanRead)
            {
                var form = await Request.ReadFormAsync();
                message = new OpenIdConnectMessage(form.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
            }

            if (message == null)
            {
                return null;
            }

            try
            {
                var messageReceivedContext = await RunMessageReceivedEventAsync(message);
                if (messageReceivedContext.HandledResponse)
                {
                    return messageReceivedContext.AuthenticationTicket;
                }
                else if (messageReceivedContext.Skipped)
                {
                    return null;
                }

                var properties = new AuthenticationProperties();

                // if state is missing, just log it
                if (string.IsNullOrEmpty(message.State))
                {
                    Logger.LogWarning(Resources.OIDCH_0004_MessageStateIsNullOrEmpty);
                }
                else
                {
                    // if state exists and we failed to 'unprotect' this is not a message we should process.
                    properties = Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(message.State));
                    if (properties == null)
                    {
                        Logger.LogError(Resources.OIDCH_0005_MessageStateIsInvalid);
                        return null;
                    }

                    string userstate = null;
                    properties.Items.TryGetValue(OpenIdConnectAuthenticationDefaults.UserstatePropertiesKey, out userstate);
                    message.State = userstate;
                }

                // if any of the error fields are set, throw error null
                if (!string.IsNullOrEmpty(message.Error))
                {
                    Logger.LogError(Resources.OIDCH_0006_MessageContainsError, message.Error, message.ErrorDescription ?? "ErrorDecription null", message.ErrorUri ?? "ErrorUri null");
                    throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.OIDCH_0006_MessageContainsError, message.Error, message.ErrorDescription ?? "ErrorDecription null", message.ErrorUri ?? "ErrorUri null"));
                }

                if (!ValidateCorrelationId(properties))
                {
                    return null;
                }

                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    Logger.LogVerbose(Resources.OIDCH_0007_UpdatingConfiguration);
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                if (string.IsNullOrEmpty(message.IdToken) && !string.IsNullOrEmpty(message.Code))
                {
                    return await HandleCodeOnlyFlow(message, properties);
                }
                else if (!string.IsNullOrEmpty(message.IdToken))
                {
                    return await HandleIdTokenFlows(message, properties);
                }
                else
                {
                    Logger.LogDebug(Resources.OIDCH_0045_Id_Token_Code_Missing);
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(Resources.OIDCH_0017_ExceptionOccurredWhileProcessingMessage, exception);

                // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
                if (Options.RefreshOnIssuerKeyNotFound && exception.GetType().Equals(typeof(SecurityTokenSignatureKeyNotFoundException)))
                {
                    if (Options.ConfigurationManager != null)
                    {
                        Logger.LogVerbose(Resources.OIDCH_0021_AutomaticConfigurationRefresh);
                        Options.ConfigurationManager.RequestRefresh();
                    }
                }

                var authenticationFailedContext = await RunAuthenticationFailedEventAsync(message, exception);
                if (authenticationFailedContext.HandledResponse)
                {
                    return authenticationFailedContext.AuthenticationTicket;
                }
                else if (authenticationFailedContext.Skipped)
                {
                    return null;
                }

                throw;
            }
        }

        private async Task<AuthenticationTicket> HandleCodeOnlyFlow(OpenIdConnectMessage message, AuthenticationProperties properties)
        {
            AuthenticationTicket ticket = null;
            JwtSecurityToken jwt = null;

            OpenIdConnectTokenEndpointResponse tokenEndpointResponse = null;
            string idToken = null;
            var authorizationCodeReceivedContext = await RunAuthorizationCodeReceivedEventAsync(message, properties, ticket, jwt);
            if (authorizationCodeReceivedContext.HandledResponse)
            {
                return authorizationCodeReceivedContext.AuthenticationTicket;
            }
            else if (authorizationCodeReceivedContext.Skipped)
            {
                return null;
            }

            // Redeeming authorization code for tokens
            Logger.LogDebug(Resources.OIDCH_0038_Redeeming_Auth_Code, message.Code);

            tokenEndpointResponse = await RedeemAuthorizationCodeAsync(message.Code, authorizationCodeReceivedContext.RedirectUri);
            idToken = tokenEndpointResponse.Message.IdToken;

            var authorizationCodeRedeemedContext = await RunAuthorizationCodeRedeemedEventAsync(message, tokenEndpointResponse);
            if (authorizationCodeRedeemedContext.HandledResponse)
            {
                return authorizationCodeRedeemedContext.AuthenticationTicket;
            }
            else if (authorizationCodeRedeemedContext.Skipped)
            {
                return null;
            }

            // no need to validate signature when token is received using "code flow" as per spec [http://openid.net/specs/openid-connect-core-1_0.html#IDTokenValidation].
            var validationParameters = Options.TokenValidationParameters.Clone();
            validationParameters.ValidateSignature = false;

            ticket = ValidateToken(idToken, message, properties, validationParameters, out jwt);

            await ValidateOpenIdConnectProtocolAsync(null, message);

            if (Options.GetClaimsFromUserInfoEndpoint)
            {
                Logger.LogDebug(Resources.OIDCH_0040_Sending_Request_UIEndpoint);
                ticket = await GetUserInformationAsync(properties, tokenEndpointResponse.Message, ticket);
            }

            var securityTokenValidatedContext = await RunSecurityTokenValidatedEventAsync(message, ticket);
            if (securityTokenValidatedContext.HandledResponse)
            {
                return securityTokenValidatedContext.AuthenticationTicket;
            }
            else if (securityTokenValidatedContext.Skipped)
            {
                return null;
            }

            return ticket;
        }

        private async Task<AuthenticationTicket> HandleIdTokenFlows(OpenIdConnectMessage message, AuthenticationProperties properties)
        {
            AuthenticationTicket ticket = null;
            JwtSecurityToken jwt = null;

            var securityTokenReceivedContext = await RunSecurityTokenReceivedEventAsync(message);
            if (securityTokenReceivedContext.HandledResponse)
            {
                return securityTokenReceivedContext.AuthenticationTicket;
            }
            else if (securityTokenReceivedContext.Skipped)
            {
                return null;
            }

            var validationParameters = Options.TokenValidationParameters.Clone();
            ticket = ValidateToken(message.IdToken, message, properties, validationParameters, out jwt);

            await ValidateOpenIdConnectProtocolAsync(jwt, message);

            var securityTokenValidatedContext = await RunSecurityTokenValidatedEventAsync(message, ticket);
            if (securityTokenValidatedContext.HandledResponse)
            {
                return securityTokenValidatedContext.AuthenticationTicket;
            }
            else if (securityTokenValidatedContext.Skipped)
            {
                return null;
            }

            if (message.Code != null)
            {
                var authorizationCodeReceivedContext = await RunAuthorizationCodeReceivedEventAsync(message, properties, ticket, jwt);
                if (authorizationCodeReceivedContext.HandledResponse)
                {
                    return authorizationCodeReceivedContext.AuthenticationTicket;
                }
                else if (authorizationCodeReceivedContext.Skipped)
                {
                    return null;
                }
            }

            return ticket;
        }

        /// <summary>
        /// Redeems the authorization code for tokens at the token endpoint
        /// </summary>
        /// <param name="authorizationCode">The authorization code to redeem.</param>
        /// <param name="redirectUri">Uri that was passed in the request sent for the authorization code.</param>
        /// <returns>OpenIdConnect message that has tokens inside it.</returns>
        protected virtual async Task<OpenIdConnectTokenEndpointResponse> RedeemAuthorizationCodeAsync(string authorizationCode, string redirectUri)
        {
            var openIdMessage = new OpenIdConnectMessage()
            {
                ClientId = Options.ClientId,
                ClientSecret = Options.ClientSecret,
                Code = authorizationCode,
                GrantType = "authorization_code",
                RedirectUri = redirectUri
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.TokenEndpoint);
            requestMessage.Content = new FormUrlEncodedContent(openIdMessage.Parameters);
            var responseMessage = await Backchannel.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
            var tokenResonse = await responseMessage.Content.ReadAsStringAsync();
            var jsonTokenResponse = JObject.Parse(tokenResonse);
            return new OpenIdConnectTokenEndpointResponse(jsonTokenResponse);
        }

        /// <summary>
        /// Goes to UserInfo endpoint to retrieve additional claims and add any unique claims to the given identity.
        /// </summary>
        /// <param name="properties">Authentication Properties</param>
        /// <param name="message">message that is being processed</param>
        /// <param name="ticket">authentication ticket with claims principal and identities</param>
        /// <returns>Authentication ticket with identity with additional claims, if any.</returns>
        protected virtual async Task<AuthenticationTicket> GetUserInformationAsync(AuthenticationProperties properties, OpenIdConnectMessage message, AuthenticationTicket ticket)
        {
            string userInfoEndpoint = null;
            if (_configuration != null)
            {
                userInfoEndpoint = _configuration.UserInfoEndpoint;
            }

            if (string.IsNullOrEmpty(userInfoEndpoint))
            {
                Logger.LogWarning(Resources.OIDCH_0046_UserInfo_Endpoint_Not_Set);
                return ticket;
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", message.AccessToken);
            var responseMessage = await Backchannel.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
            var userInfoResponse = await responseMessage.Content.ReadAsStringAsync();
            var user = JObject.Parse(userInfoResponse);

            var identity = (ClaimsIdentity)ticket.Principal.Identity;
            var subjectClaimType = identity.FindFirst(ClaimTypes.NameIdentifier);
            if (subjectClaimType == null)
            {
                throw new OpenIdConnectProtocolException(string.Format(CultureInfo.InvariantCulture, Resources.OIDCH_0041_Subject_Claim_Not_Found, identity.ToString()));
            }

            var userInfoSubjectClaimValue = user.Value<string>(JwtRegisteredClaimNames.Sub);

            // check if the sub claim matches
            if (userInfoSubjectClaimValue == null || !string.Equals(userInfoSubjectClaimValue, subjectClaimType.Value, StringComparison.Ordinal))
            {
                throw new OpenIdConnectProtocolException(Resources.OIDCH_0039_Subject_Claim_Mismatch);
            }

            foreach (var claim in identity.Claims)
            {
                // If this claimType is mapped by the JwtSeurityTokenHandler, then this property will be set
                var shortClaimTypeName = claim.Properties.ContainsKey(JwtSecurityTokenHandler.ShortClaimTypeProperty) ?
                    claim.Properties[JwtSecurityTokenHandler.ShortClaimTypeProperty] : string.Empty;

                // checking if claim in the identity (generated from id_token) has the same type as a claim retrieved from userinfo endpoint
                JToken value;
                var isClaimIncluded = user.TryGetValue(claim.Type, out value) || user.TryGetValue(shortClaimTypeName, out value);

                // if a same claim exists (matching both type and value) both in id_token identity and userinfo response, remove the json entry from the userinfo response
                if (isClaimIncluded && claim.Value.Equals(value.ToString(), StringComparison.Ordinal))
                {
                    if (!user.Remove(claim.Type))
                    {
                        user.Remove(shortClaimTypeName);
                    }
                }
            }

            // adding remaining unique claims from userinfo endpoint to the identity
            foreach (var pair in user)
            {
                JToken value;
                var claimValue = user.TryGetValue(pair.Key, out value) ? value.ToString() : null;
                identity.AddClaim(new Claim(pair.Key, claimValue, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            return new AuthenticationTicket(new ClaimsPrincipal(identity), ticket.Properties, ticket.AuthenticationScheme);
        }

        /// <summary>
        /// Adds the nonce to <see cref="HttpResponse.Cookies"/>.
        /// </summary>
        /// <param name="nonce">the nonce to remember.</param>
        /// <remarks><see cref="HttpResponse.Cookies.Append"/>is called to add a cookie with the name: 'OpenIdConnectAuthenticationDefaults.Nonce + <see cref="OpenIdConnectAuthenticationOptions.StringDataFormat.Protect"/>(nonce)'.
        /// The value of the cookie is: "N".</remarks>
        private void WriteNonceCookie(string nonce)
        {
            if (string.IsNullOrEmpty(nonce))
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            Response.Cookies.Append(
                OpenIdConnectAuthenticationDefaults.CookieNoncePrefix + Options.StringDataFormat.Protect(nonce),
                NonceProperty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    Expires = DateTime.UtcNow + Options.ProtocolValidator.NonceLifetime
                });
        }

        /// <summary>
        /// Searches <see cref="HttpRequest.Cookies"/> for a matching nonce.
        /// </summary>
        /// <param name="nonce">the nonce that we are looking for.</param>
        /// <returns>echos 'nonce' if a cookie is found that matches, null otherwise.</returns>
        /// <remarks>Examine <see cref="HttpRequest.Cookies.Keys"/> that start with the prefix: 'OpenIdConnectAuthenticationDefaults.Nonce'. 
        /// <see cref="OpenIdConnectAuthenticationOptions.StringDataFormat.Unprotect"/> is used to obtain the actual 'nonce'. If the nonce is found, then <see cref="HttpResponse.Cookies.Delete"/> is called.</remarks>
        private string ReadNonceCookie(string nonce)
        {
            if (nonce == null)
            {
                return null;
            }

            foreach (var nonceKey in Request.Cookies.Keys)
            {
                if (nonceKey.StartsWith(OpenIdConnectAuthenticationDefaults.CookieNoncePrefix))
                {
                    try
                    {
                        var nonceDecodedValue = Options.StringDataFormat.Unprotect(nonceKey.Substring(OpenIdConnectAuthenticationDefaults.CookieNoncePrefix.Length, nonceKey.Length - OpenIdConnectAuthenticationDefaults.CookieNoncePrefix.Length));
                        if (nonceDecodedValue == nonce)
                        {
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = Request.IsHttps
                            };

                            Response.Cookies.Delete(nonceKey, cookieOptions);
                            return nonce;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning("Failed to un-protect the nonce cookie.", ex);
                    }
                }
            }

            return null;
        }

        private void GenerateCorrelationId(AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var correlationKey = OpenIdConnectAuthenticationDefaults.CookieStatePrefix;

            var nonceBytes = new byte[32];
            CryptoRandom.GetBytes(nonceBytes);
            var correlationId = TextEncodings.Base64Url.Encode(nonceBytes);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                Expires = DateTime.UtcNow + Options.ProtocolValidator.NonceLifetime
            };

            properties.Items[correlationKey] = correlationId;

            Response.Cookies.Append(correlationKey + correlationId, NonceProperty, cookieOptions);
        }

        private bool ValidateCorrelationId(AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var correlationKey = OpenIdConnectAuthenticationDefaults.CookieStatePrefix;

            string correlationId;
            if (!properties.Items.TryGetValue(
                correlationKey,
                out correlationId))
            {
                Logger.LogWarning("{0} state property not found.", correlationKey);
                return false;
            }

            properties.Items.Remove(correlationKey);

            var cookieName = correlationKey + correlationId;

            var correlationCookie = Request.Cookies[cookieName];
            if (string.IsNullOrEmpty(correlationCookie))
            {
                Logger.LogWarning("{0} cookie not found.", cookieName);
                return false;
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };
            Response.Cookies.Delete(cookieName, cookieOptions);

            if (!string.Equals(correlationCookie, NonceProperty, StringComparison.Ordinal))
            {
                Logger.LogWarning("{0} correlation cookie and state property mismatch.", correlationKey);
                return false;
            }

            return true;
        }

        private AuthenticationProperties GetPropertiesFromState(string state)
        {
            // assume a well formed query string: <a=b&>OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey=kasjd;fljasldkjflksdj<&c=d>
            var startIndex = 0;
            if (string.IsNullOrEmpty(state) || (startIndex = state.IndexOf(OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey, StringComparison.Ordinal)) == -1)
            {
                return null;
            }

            var authenticationIndex = startIndex + OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey.Length;
            if (authenticationIndex == -1 || authenticationIndex == state.Length || state[authenticationIndex] != '=')
            {
                return null;
            }

            // scan rest of string looking for '&'
            authenticationIndex++;
            var endIndex = state.Substring(authenticationIndex, state.Length - authenticationIndex).IndexOf("&", StringComparison.Ordinal);

            // -1 => no other parameters are after the AuthenticationPropertiesKey
            if (endIndex == -1)
            {
                return Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(state.Substring(authenticationIndex).Replace('+', ' ')));
            }
            else
            {
                return Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(state.Substring(authenticationIndex, endIndex).Replace('+', ' ')));
            }
        }

        private async Task<MessageReceivedContext> RunMessageReceivedEventAsync(OpenIdConnectMessage message)
        {
            Logger.LogDebug(Resources.OIDCH_0001_MessageReceived, message.BuildRedirectUrl());
            var messageReceivedContext = new MessageReceivedContext(Context, Options)
            {
                ProtocolMessage = message
            };

            await Options.Events.MessageReceived(messageReceivedContext);
            if (messageReceivedContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0002_MessageReceivedContextHandledResponse);
            }
            else if (messageReceivedContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0003_MessageReceivedContextSkipped);
            }

            return messageReceivedContext;
        }

        private async Task<AuthorizationCodeReceivedContext> RunAuthorizationCodeReceivedEventAsync(OpenIdConnectMessage message, AuthenticationProperties properties, AuthenticationTicket ticket, JwtSecurityToken jwt)
        {
            var redirectUri = properties.Items.ContainsKey(OpenIdConnectAuthenticationDefaults.RedirectUriForCodePropertiesKey) ?
                properties.Items[OpenIdConnectAuthenticationDefaults.RedirectUriForCodePropertiesKey] : Options.RedirectUri;

            Logger.LogDebug(Resources.OIDCH_0014_AuthorizationCodeReceived, message.Code);

            var authorizationCodeReceivedContext = new AuthorizationCodeReceivedContext(Context, Options)
            {
                Code = message.Code,
                ProtocolMessage = message,
                RedirectUri = redirectUri,
                AuthenticationTicket = ticket,
                JwtSecurityToken = jwt
            };

            await Options.Events.AuthorizationCodeReceived(authorizationCodeReceivedContext);
            if (authorizationCodeReceivedContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0015_AuthorizationCodeReceivedContextHandledResponse);
            }
            else if (authorizationCodeReceivedContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0016_AuthorizationCodeReceivedContextSkipped);
            }

            return authorizationCodeReceivedContext;
        }

        private async Task<AuthorizationCodeRedeemedContext> RunAuthorizationCodeRedeemedEventAsync(OpenIdConnectMessage message, OpenIdConnectTokenEndpointResponse tokenEndpointResponse)
        {
            Logger.LogDebug(Resources.OIDCH_0042_AuthorizationCodeRedeemed, message.Code);
            var authorizationCodeRedeemedContext = new AuthorizationCodeRedeemedContext(Context, Options)
            {
                Code = message.Code,
                ProtocolMessage = message,
                TokenEndpointResponse = tokenEndpointResponse
            };

            await Options.Events.AuthorizationCodeRedeemed(authorizationCodeRedeemedContext);
            if (authorizationCodeRedeemedContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0043_AuthorizationCodeRedeemedContextHandledResponse);
            }
            else if (authorizationCodeRedeemedContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0044_AuthorizationCodeRedeemedContextSkipped);
            }
            return authorizationCodeRedeemedContext;
        }

        private async Task<SecurityTokenReceivedContext> RunSecurityTokenReceivedEventAsync(OpenIdConnectMessage message)
        {
            Logger.LogDebug(Resources.OIDCH_0020_IdTokenReceived, message.IdToken);
            var securityTokenReceivedContext = new SecurityTokenReceivedContext(Context, Options)
            {
                ProtocolMessage = message,
            };

            await Options.Events.SecurityTokenReceived(securityTokenReceivedContext);
            if (securityTokenReceivedContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0008_SecurityTokenReceivedContextHandledResponse);
            }
            else if (securityTokenReceivedContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0009_SecurityTokenReceivedContextSkipped);
            }

            return securityTokenReceivedContext;
        }

        private async Task<SecurityTokenValidatedContext> RunSecurityTokenValidatedEventAsync(OpenIdConnectMessage message, AuthenticationTicket ticket)
        {
            var securityTokenValidatedContext = new SecurityTokenValidatedContext(Context, Options)
            {
                AuthenticationTicket = ticket,
                ProtocolMessage = message
            };

            await Options.Events.SecurityTokenValidated(securityTokenValidatedContext);
            if (securityTokenValidatedContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0012_SecurityTokenValidatedContextHandledResponse);
            }
            else if (securityTokenValidatedContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0013_SecurityTokenValidatedContextSkipped);
            }

            return securityTokenValidatedContext;
        }

        private async Task<AuthenticationFailedContext> RunAuthenticationFailedEventAsync(OpenIdConnectMessage message, Exception exception)
        {
            var authenticationFailedContext = new AuthenticationFailedContext(Context, Options)
            {
                ProtocolMessage = message,
                Exception = exception
            };

            await Options.Events.AuthenticationFailed(authenticationFailedContext);
            if (authenticationFailedContext.HandledResponse)
            {
                Logger.LogVerbose(Resources.OIDCH_0018_AuthenticationFailedContextHandledResponse);
            }
            else if (authenticationFailedContext.Skipped)
            {
                Logger.LogVerbose(Resources.OIDCH_0019_AuthenticationFailedContextSkipped);
            }

            return authenticationFailedContext;
        }

        private AuthenticationTicket ValidateToken(string idToken, OpenIdConnectMessage message, AuthenticationProperties properties, TokenValidationParameters validationParameters, out JwtSecurityToken jwt)
        {
            AuthenticationTicket ticket = null;
            jwt = null;

            if (_configuration != null)
            {
                if (string.IsNullOrEmpty(validationParameters.ValidIssuer))
                {
                    validationParameters.ValidIssuer = _configuration.Issuer;
                }
                else if (!string.IsNullOrEmpty(_configuration.Issuer))
                {
                    validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(new[] { _configuration.Issuer }) ?? new[] { _configuration.Issuer };
                }

                validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(_configuration.SigningKeys) ?? _configuration.SigningKeys;
            }

            SecurityToken validatedToken = null;
            ClaimsPrincipal principal = null;
            if (Options.SecurityTokenValidator.CanReadToken(idToken))
            {
                principal = Options.SecurityTokenValidator.ValidateToken(idToken, validationParameters, out validatedToken);
                jwt = validatedToken as JwtSecurityToken;
                if (jwt == null)
                {
                    Logger.LogError(Resources.OIDCH_0010_ValidatedSecurityTokenNotJwt, validatedToken?.GetType());
                    throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.OIDCH_0010_ValidatedSecurityTokenNotJwt, validatedToken?.GetType()));
                }
            }

            if (validatedToken == null)
            {
                Logger.LogError(Resources.OIDCH_0011_UnableToValidateToken, idToken);
                throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.OIDCH_0011_UnableToValidateToken, idToken));
            }

            ticket = new AuthenticationTicket(principal, properties, Options.AuthenticationScheme);
            if (!string.IsNullOrEmpty(message.SessionState))
            {
                ticket.Properties.Items[OpenIdConnectSessionProperties.SessionState] = message.SessionState;
            }

            if (_configuration != null && !string.IsNullOrEmpty(_configuration.CheckSessionIframe))
            {
                ticket.Properties.Items[OpenIdConnectSessionProperties.CheckSessionIFrame] = _configuration.CheckSessionIframe;
            }

            // Rename?
            if (Options.UseTokenLifetime)
            {
                var issued = validatedToken.ValidFrom;
                if (issued != DateTime.MinValue)
                {
                    ticket.Properties.IssuedUtc = issued;
                }

                var expires = validatedToken.ValidTo;
                if (expires != DateTime.MinValue)
                {
                    ticket.Properties.ExpiresUtc = expires;
                }
            }

            return ticket;
        }

        private async Task ValidateOpenIdConnectProtocolAsync(JwtSecurityToken jwt, OpenIdConnectMessage message)
        {
            string nonce = jwt?.Payload.Nonce;
            if (!string.IsNullOrEmpty(nonce))
            {
                if (Options.CacheNonces)
                {
                    if (await Options.NonceCache.GetAsync(nonce) != null)
                    {
                        await Options.NonceCache.RemoveAsync(nonce);
                    }
                    else
                    {
                        // If the nonce cannot be removed, it was
                        // already used and MUST be rejected.
                        nonce = null;
                    }
                }
                else
                {
                    nonce = ReadNonceCookie(nonce);
                }
            }

            var protocolValidationContext = new OpenIdConnectProtocolValidationContext
            {
                ProtocolMessage = message,
                IdToken = jwt,
                ClientId = Options.ClientId,
                Nonce = nonce
            };

            Options.ProtocolValidator.Validate(protocolValidationContext);
        }

        /// <summary>
        /// Calls InvokeReplyPathAsync
        /// </summary>
        /// <returns>True if the request was handled, false if the next middleware should be invoked.</returns>
        public override Task<bool> InvokeAsync()
        {
            return InvokeReplyPathAsync();
        }

        private async Task<bool> InvokeReplyPathAsync()
        {
            var ticket = await HandleAuthenticateOnceAsync();
            if (ticket != null)
            {
                if (ticket.Principal != null)
                {
                    await Request.HttpContext.Authentication.SignInAsync(Options.SignInScheme, ticket.Principal, ticket.Properties);
                }

                // Redirect back to the original secured resource, if any.
                if (!string.IsNullOrEmpty(ticket.Properties.RedirectUri))
                {
                    Response.Redirect(ticket.Properties.RedirectUri);
                    return true;
                }
            }

            return false;
        }
    }
}
