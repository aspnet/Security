// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    /// <summary>
    /// A per-request authentication handler for OpenIdConnect.
    /// </summary>
    public class OpenIdConnectHandler : OpenIdConnectHandler<OpenIdConnectOptions>
    {
        public OpenIdConnectHandler(IOptionsSnapshot<OpenIdConnectOptions> options, ILoggerFactory logger, HtmlEncoder htmlEncoder, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, htmlEncoder, encoder, clock)
        { }
    }

    /// <summary>
    /// A per-request authentication handler for OpenIdConnect.
    /// </summary>
    /// <typeparam name="TOptions">The options type used to configure the handler.</typeparam>
    public class OpenIdConnectHandler<TOptions> : RemoteAuthenticationHandler<TOptions>
        where TOptions : OpenIdConnectOptions, new()
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

        protected HttpClient Backchannel => Options.Backchannel;

        protected HtmlEncoder HtmlEncoder { get; }

        public OpenIdConnectHandler(IOptionsSnapshot<TOptions> options, ILoggerFactory logger, HtmlEncoder htmlEncoder, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            HtmlEncoder = htmlEncoder;
        }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new OpenIdConnectEvents Events
        {
            get { return (OpenIdConnectEvents)base.Events; }
            set { base.Events = value; }
        }

        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new OpenIdConnectEvents());

        public override Task<bool> HandleRequestAsync()
        {
            if (Options.RemoteSignOutPath.HasValue && Options.RemoteSignOutPath == Request.Path)
            {
                return HandleRemoteSignOutAsync();
            }
            else if (Options.SignedOutCallbackPath.HasValue && Options.SignedOutCallbackPath == Request.Path)
            {
                return HandleSignOutCallbackAsync();
            }

            return base.HandleRequestAsync();
        }

        protected virtual async Task<bool> HandleRemoteSignOutAsync()
        {
            OpenIdConnectMessage message = null;

            if (string.Equals(Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                message = new OpenIdConnectMessage(Request.Query.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
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

            var remoteSignOutContext = new RemoteSignOutContext(Context, Scheme, Options, message);
            await Events.RemoteSignOut(remoteSignOutContext);

            if (remoteSignOutContext.HandledResponse)
            {
                Logger.RemoteSignOutHandledResponse();
                return true;
            }
            if (remoteSignOutContext.Skipped)
            {
                Logger.RemoteSignOutSkipped();
                return false;
            }

            if (message == null)
            {
                return false;
            }

            // Try to extract the session identifier from the authentication ticket persisted by the sign-in handler.
            // If the identifier cannot be found, bypass the session identifier checks: this may indicate that the
            // authentication cookie was already cleared, that the session identifier was lost because of a lossy
            // external/application cookie conversion or that the identity provider doesn't support sessions.
            var sid = (await Context.AuthenticateAsync(Options.SignOutScheme))
                          ?.Principal
                          ?.FindFirst(JwtRegisteredClaimNames.Sid)
                          ?.Value;
            if (!string.IsNullOrEmpty(sid))
            {
                // Ensure a 'sid' parameter was sent by the identity provider.
                if (string.IsNullOrEmpty(message.Sid))
                {
                    Logger.RemoteSignOutSessionIdMissing();
                    return true;
                }
                // Ensure the 'sid' parameter corresponds to the 'sid' stored in the authentication ticket.
                if (!string.Equals(sid, message.Sid, StringComparison.Ordinal))
                {
                    Logger.RemoteSignOutSessionIdInvalid();
                    return true;
                }
            }

            Logger.RemoteSignOut();

            // We've received a remote sign-out request
            await Context.SignOutAsync(Options.SignOutScheme);
            return true;
        }

        /// <summary>
        /// Redirect user to the identity provider for sign out
        /// </summary>
        /// <returns>A task executing the sign out procedure</returns>
        protected override async Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            Logger.EnteringOpenIdAuthenticationHandlerHandleSignOutAsync(GetType().FullName);

            if (_configuration == null && Options.ConfigurationManager != null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
            }

            var message = new OpenIdConnectMessage()
            {
                EnableTelemetryParameters = !Options.DisableTelemetry,
                IssuerAddress = _configuration?.EndSessionEndpoint ?? string.Empty,

                // Redirect back to SigneOutCallbackPath first before user agent is redirected to actual post logout redirect uri
                PostLogoutRedirectUri = BuildRedirectUriIfRelative(Options.SignedOutCallbackPath)
            };

            // Get the post redirect URI.
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = BuildRedirectUriIfRelative(Options.PostLogoutRedirectUri);
                if (string.IsNullOrWhiteSpace(properties.RedirectUri))
                {
                    properties.RedirectUri = CurrentUri;
                }
            }
            Logger.PostSignOutRedirect(properties.RedirectUri);

            // Attach the identity token to the logout request when possible.
            message.IdTokenHint = await Context.GetTokenAsync(Options.SignOutScheme, OpenIdConnectParameterNames.IdToken);

            var redirectContext = new RedirectContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = message
            };

            await Events.RedirectToIdentityProviderForSignOut(redirectContext);
            if (redirectContext.HandledResponse)
            {
                Logger.RedirectToIdentityProviderForSignOutHandledResponse();
                return;
            }
            else if (redirectContext.Skipped)
            {
                Logger.RedirectToIdentityProviderForSignOutSkipped();
                return;
            }

            message = redirectContext.ProtocolMessage;

            if (!string.IsNullOrEmpty(message.State))
            {
                properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = message.State;
            }

            message.State = Options.StateDataFormat.Protect(properties);

            if (string.IsNullOrEmpty(message.IssuerAddress))
            {
                throw new InvalidOperationException(
                    "Cannot redirect to the end session endpoint, the configuration may be missing or invalid.");
            }

            if (Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.RedirectGet)
            {
                var redirectUri = message.CreateLogoutRequestUrl();
                if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    Logger.InvalidLogoutQueryStringRedirectUrl(redirectUri);
                }

                Response.Redirect(redirectUri);
            }
            else if (Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.FormPost)
            {
                var inputs = new StringBuilder();
                foreach (var parameter in message.Parameters)
                {
                    var name = HtmlEncoder.Encode(parameter.Key);
                    var value = HtmlEncoder.Encode(parameter.Value);

                    var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                    inputs.AppendLine(input);
                }

                var issuer = HtmlEncoder.Encode(message.IssuerAddress);

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
            else
            {
                throw new NotImplementedException($"An unsupported authentication method has been configured: {Options.AuthenticationMethod}");
            }
        }

        /// <summary>
        /// Response to the callback from OpenId provider after session ended.
        /// </summary>
        /// <returns>A task executing the callback procedure</returns>
        protected virtual Task<bool> HandleSignOutCallbackAsync()
        {
            StringValues protectedState;
            if (Request.Query.TryGetValue(OpenIdConnectParameterNames.State, out protectedState))
            {
                var properties = Options.StateDataFormat.Unprotect(protectedState);
                if (!string.IsNullOrEmpty(properties?.RedirectUri))
                {
                    Response.Redirect(properties.RedirectUri);
                }
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Responds to a 401 Challenge. Sends an OpenIdConnect message to the 'identity authority' to obtain an identity.
        /// </summary>
        /// <returns></returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Logger.EnteringOpenIdAuthenticationHandlerHandleUnauthorizedAsync(GetType().FullName);

            // order for local RedirectUri
            // 1. challenge.Properties.RedirectUri
            // 2. CurrentUri if RedirectUri is not set)
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = CurrentUri;
            }
            Logger.PostAuthenticationLocalRedirect(properties.RedirectUri);

            if (_configuration == null && Options.ConfigurationManager != null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
            }

            var message = new OpenIdConnectMessage
            {
                ClientId = Options.ClientId,
                EnableTelemetryParameters = !Options.DisableTelemetry,
                IssuerAddress = _configuration?.AuthorizationEndpoint ?? string.Empty,
                RedirectUri = BuildRedirectUri(Options.CallbackPath),
                Resource = Options.Resource,
                ResponseType = Options.ResponseType,
                Scope = string.Join(" ", Options.Scope)
            };

            // Omitting the response_mode parameter when it already corresponds to the default
            // response_mode used for the specified response_type is recommended by the specifications.
            // See http://openid.net/specs/oauth-v2-multiple-response-types-1_0.html#ResponseModes
            if (!string.Equals(Options.ResponseType, OpenIdConnectResponseType.Code, StringComparison.Ordinal) ||
                !string.Equals(Options.ResponseMode, OpenIdConnectResponseMode.Query, StringComparison.Ordinal))
            {
                message.ResponseMode = Options.ResponseMode;
            }

            if (Options.ProtocolValidator.RequireNonce)
            {
                message.Nonce = Options.ProtocolValidator.GenerateNonce();
                WriteNonceCookie(message.Nonce);
            }

            GenerateCorrelationId(properties);

            var redirectContext = new RedirectContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = message
            };

            await Events.RedirectToIdentityProvider(redirectContext);
            if (redirectContext.HandledResponse)
            {
                Logger.RedirectToIdentityProviderHandledResponse();
                return;
            }
            else if (redirectContext.Skipped)
            {
                Logger.RedirectToIdentityProviderSkipped();
                return;
            }

            message = redirectContext.ProtocolMessage;

            if (!string.IsNullOrEmpty(message.State))
            {
                properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = message.State;
            }

            // When redeeming a 'code' for an AccessToken, this value is needed
            properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, message.RedirectUri);

            message.State = Options.StateDataFormat.Protect(properties);

            if (string.IsNullOrEmpty(message.IssuerAddress))
            {
                throw new InvalidOperationException(
                    "Cannot redirect to the authorization endpoint, the configuration may be missing or invalid.");
            }

            if (Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.RedirectGet)
            {
                var redirectUri = message.CreateAuthenticationRequestUrl();
                if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    Logger.InvalidAuthenticationRequestUrl(redirectUri);
                }

                Response.Redirect(redirectUri);
                return;
            }
            else if (Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.FormPost)
            {
                var inputs = new StringBuilder();
                foreach (var parameter in message.Parameters)
                {
                    var name = HtmlEncoder.Encode(parameter.Key);
                    var value = HtmlEncoder.Encode(parameter.Value);

                    var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                    inputs.AppendLine(input);
                }

                var issuer = HtmlEncoder.Encode(message.IssuerAddress);

                var content = string.Format(CultureInfo.InvariantCulture, HtmlFormFormat, issuer, inputs);
                var buffer = Encoding.UTF8.GetBytes(content);

                Response.ContentLength = buffer.Length;
                Response.ContentType = "text/html;charset=UTF-8";

                // Emit Cache-Control=no-cache to prevent client caching.
                Response.Headers[HeaderNames.CacheControl] = "no-cache";
                Response.Headers[HeaderNames.Pragma] = "no-cache";
                Response.Headers[HeaderNames.Expires] = "-1";

                await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                return;
            }

            throw new NotImplementedException($"An unsupported authentication method has been configured: {Options.AuthenticationMethod}");
        }

        /// <summary>
        /// Invoked to process incoming OpenIdConnect messages.
        /// </summary>
        /// <returns>An <see cref="AuthenticationTicket"/> if successful.</returns>
        protected override async Task<AuthenticateResult> HandleRemoteAuthenticateAsync()
        {
            Logger.EnteringOpenIdAuthenticationHandlerHandleRemoteAuthenticateAsync(GetType().FullName);

            OpenIdConnectMessage authorizationResponse = null;

            if (string.Equals(Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                authorizationResponse = new OpenIdConnectMessage(Request.Query.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));

                // response_mode=query (explicit or not) and a response_type containing id_token
                // or token are not considered as a safe combination and MUST be rejected.
                // See http://openid.net/specs/oauth-v2-multiple-response-types-1_0.html#Security
                if (!string.IsNullOrEmpty(authorizationResponse.IdToken) || !string.IsNullOrEmpty(authorizationResponse.AccessToken))
                {
                    if (Options.SkipUnrecognizedRequests)
                    {
                        // Not for us?
                        return AuthenticateResult.None();
                    }
                    return AuthenticateResult.Fail("An OpenID Connect response cannot contain an " +
                            "identity token or an access token when using response_mode=query");
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
                authorizationResponse = new OpenIdConnectMessage(form.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
            }

            if (authorizationResponse == null)
            {
                if (Options.SkipUnrecognizedRequests)
                {
                    // Not for us?
                    return AuthenticateResult.None();
                }
                return AuthenticateResult.Fail("No message.");
            }

            AuthenticateResult result;

            try
            {
                AuthenticationProperties properties = null;
                if (!string.IsNullOrEmpty(authorizationResponse.State))
                {
                    properties = Options.StateDataFormat.Unprotect(authorizationResponse.State);
                }

                var messageReceivedContext = await RunMessageReceivedEventAsync(authorizationResponse, properties);
                if (messageReceivedContext.IsProcessingComplete(out result))
                {
                    return result;
                }
                authorizationResponse = messageReceivedContext.ProtocolMessage;
                properties = messageReceivedContext.Properties;

                if (properties == null)
                {
                    // Fail if state is missing, it's required for the correlation id.
                    if (string.IsNullOrEmpty(authorizationResponse.State))
                    {
                        // This wasn't a valid OIDC message, it may not have been intended for us.
                        Logger.NullOrEmptyAuthorizationResponseState();
                        if (Options.SkipUnrecognizedRequests)
                        {
                            return AuthenticateResult.None();
                        }
                        return AuthenticateResult.Fail(Resources.MessageStateIsNullOrEmpty);
                    }

                    // if state exists and we failed to 'unprotect' this is not a message we should process.
                    properties = Options.StateDataFormat.Unprotect(authorizationResponse.State);
                }

                if (properties == null)
                {
                    Logger.UnableToReadAuthorizationResponseState();
                    if (Options.SkipUnrecognizedRequests)
                    {
                        // Not for us?
                        return AuthenticateResult.None();
                    }
                    return AuthenticateResult.Fail(Resources.MessageStateIsInvalid);
                }

                string userstate = null;
                properties.Items.TryGetValue(OpenIdConnectDefaults.UserstatePropertiesKey, out userstate);
                authorizationResponse.State = userstate;

                if (!ValidateCorrelationId(properties))
                {
                    return AuthenticateResult.Fail("Correlation failed.");
                }

                // if any of the error fields are set, throw error null
                if (!string.IsNullOrEmpty(authorizationResponse.Error))
                {
                    return AuthenticateResult.Fail(CreateOpenIdConnectProtocolException(authorizationResponse, response: null));
                }

                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    Logger.UpdatingConfiguration();
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                PopulateSessionProperties(authorizationResponse, properties);

                AuthenticationTicket ticket = null;
                JwtSecurityToken jwt = null;
                string nonce = null;
                var validationParameters = Options.TokenValidationParameters.Clone();

                // Hybrid or Implicit flow
                if (!string.IsNullOrEmpty(authorizationResponse.IdToken))
                {
                    Logger.ReceivedIdToken();
                    ticket = ValidateToken(authorizationResponse.IdToken, properties, validationParameters, out jwt);

                    nonce = jwt.Payload.Nonce;
                    if (!string.IsNullOrEmpty(nonce))
                    {
                        nonce = ReadNonceCookie(nonce);
                    }

                    var tokenValidatedContext = await RunTokenValidatedEventAsync(authorizationResponse, null, properties, ticket, jwt, nonce);
                    if (tokenValidatedContext.IsProcessingComplete(out result))
                    {
                        return result;
                    }
                    authorizationResponse = tokenValidatedContext.ProtocolMessage;
                    properties = tokenValidatedContext.Properties;
                    ticket = tokenValidatedContext.Ticket;
                    jwt = tokenValidatedContext.SecurityToken;
                    nonce = tokenValidatedContext.Nonce;
                }

                Options.ProtocolValidator.ValidateAuthenticationResponse(new OpenIdConnectProtocolValidationContext()
                {
                    ClientId = Options.ClientId,
                    ProtocolMessage = authorizationResponse,
                    ValidatedIdToken = jwt,
                    Nonce = nonce
                });

                OpenIdConnectMessage tokenEndpointResponse = null;

                // Authorization Code or Hybrid flow
                if (!string.IsNullOrEmpty(authorizationResponse.Code))
                {
                    var authorizationCodeReceivedContext = await RunAuthorizationCodeReceivedEventAsync(authorizationResponse, properties, ticket, jwt);
                    if (authorizationCodeReceivedContext.IsProcessingComplete(out result))
                    {
                        return result;
                    }
                    authorizationResponse = authorizationCodeReceivedContext.ProtocolMessage;
                    properties = authorizationCodeReceivedContext.Properties;
                    var tokenEndpointRequest = authorizationCodeReceivedContext.TokenEndpointRequest;
                    // If the developer redeemed the code themselves...
                    tokenEndpointResponse = authorizationCodeReceivedContext.TokenEndpointResponse;
                    ticket = authorizationCodeReceivedContext.Ticket;
                    jwt = authorizationCodeReceivedContext.JwtSecurityToken;

                    if (!authorizationCodeReceivedContext.HandledCodeRedemption)
                    {
                        tokenEndpointResponse = await RedeemAuthorizationCodeAsync(tokenEndpointRequest);
                    }

                    var tokenResponseReceivedContext = await RunTokenResponseReceivedEventAsync(authorizationResponse, tokenEndpointResponse, properties, ticket);
                    if (tokenResponseReceivedContext.IsProcessingComplete(out result))
                    {
                        return result;
                    }

                    authorizationResponse = tokenResponseReceivedContext.ProtocolMessage;
                    tokenEndpointResponse = tokenResponseReceivedContext.TokenEndpointResponse;

                    // no need to validate signature when token is received using "code flow" as per spec
                    // [http://openid.net/specs/openid-connect-core-1_0.html#IDTokenValidation].
                    validationParameters.RequireSignedTokens = false;

                    // At least a cursory validation is required on the new IdToken, even if we've already validated the one from the authorization response.
                    // And we'll want to validate the new JWT in ValidateTokenResponse.
                    JwtSecurityToken tokenEndpointJwt;
                    var tokenEndpointTicket = ValidateToken(tokenEndpointResponse.IdToken, properties, validationParameters, out tokenEndpointJwt);

                    // Avoid reading & deleting the nonce cookie, running the event, etc, if it was already done as part of the authorization response validation.
                    if (ticket == null)
                    {
                        nonce = tokenEndpointJwt.Payload.Nonce;
                        if (!string.IsNullOrEmpty(nonce))
                        {
                            nonce = ReadNonceCookie(nonce);
                        }

                        var tokenValidatedContext = await RunTokenValidatedEventAsync(authorizationResponse, tokenEndpointResponse, properties, tokenEndpointTicket, tokenEndpointJwt, nonce);
                        if (tokenValidatedContext.IsProcessingComplete(out result))
                        {
                            return result;
                        }
                        authorizationResponse = tokenValidatedContext.ProtocolMessage;
                        tokenEndpointResponse = tokenValidatedContext.TokenEndpointResponse;
                        properties = tokenValidatedContext.Properties;
                        ticket = tokenValidatedContext.Ticket;
                        jwt = tokenValidatedContext.SecurityToken;
                        nonce = tokenValidatedContext.Nonce;
                    }
                    else
                    {
                        if (!string.Equals(jwt.Subject, tokenEndpointJwt.Subject, StringComparison.Ordinal))
                        {
                            throw new SecurityTokenException("The sub claim does not match in the id_token's from the authorization and token endpoints.");
                        }

                        jwt = tokenEndpointJwt;
                    }

                    // Validate the token response if it wasn't provided manually
                    if (!authorizationCodeReceivedContext.HandledCodeRedemption)
                    {
                        Options.ProtocolValidator.ValidateTokenResponse(new OpenIdConnectProtocolValidationContext()
                        {
                            ClientId = Options.ClientId,
                            ProtocolMessage = tokenEndpointResponse,
                            ValidatedIdToken = jwt,
                            Nonce = nonce
                        });
                    }
                }

                if (Options.SaveTokens)
                {
                    SaveTokens(ticket.Properties, tokenEndpointResponse ?? authorizationResponse);
                }

                if (Options.GetClaimsFromUserInfoEndpoint)
                {
                    return await GetUserInformationAsync(tokenEndpointResponse ?? authorizationResponse, jwt, ticket);
                }
                else
                {
                    var identity = (ClaimsIdentity)ticket.Principal.Identity;
                    foreach (var action in Options.ClaimActions)
                    {
                        action.Run(null, identity, ClaimsIssuer);
                    }
                }

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception exception)
            {
                Logger.ExceptionProcessingMessage(exception);

                // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
                if (Options.RefreshOnIssuerKeyNotFound && exception is SecurityTokenSignatureKeyNotFoundException)
                {
                    if (Options.ConfigurationManager != null)
                    {
                        Logger.ConfigurationManagerRequestRefreshCalled();
                        Options.ConfigurationManager.RequestRefresh();
                    }
                }

                var authenticationFailedContext = await RunAuthenticationFailedEventAsync(authorizationResponse, exception);
                if (authenticationFailedContext.IsProcessingComplete(out result))
                {
                    return result;
                }

                return AuthenticateResult.Fail(exception);
            }
        }

        private void PopulateSessionProperties(OpenIdConnectMessage message, AuthenticationProperties properties)
        {
            if (!string.IsNullOrEmpty(message.SessionState))
            {
                properties.Items[OpenIdConnectSessionProperties.SessionState] = message.SessionState;
            }

            if (!string.IsNullOrEmpty(_configuration.CheckSessionIframe))
            {
                properties.Items[OpenIdConnectSessionProperties.CheckSessionIFrame] = _configuration.CheckSessionIframe;
            }
        }

        /// <summary>
        /// Redeems the authorization code for tokens at the token endpoint.
        /// </summary>
        /// <param name="tokenEndpointRequest">The request that will be sent to the token endpoint and is available for customization.</param>
        /// <returns>OpenIdConnect message that has tokens inside it.</returns>
        protected virtual async Task<OpenIdConnectMessage> RedeemAuthorizationCodeAsync(OpenIdConnectMessage tokenEndpointRequest)
        {
            Logger.RedeemingCodeForTokens();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.TokenEndpoint);
            requestMessage.Content = new FormUrlEncodedContent(tokenEndpointRequest.Parameters);

            var responseMessage = await Backchannel.SendAsync(requestMessage);

            var contentMediaType = responseMessage.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrEmpty(contentMediaType))
            {
                Logger.LogDebug($"Unexpected token response format. Status Code: {(int)responseMessage.StatusCode}. Content-Type header is missing.");
            }
            else if (!string.Equals(contentMediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogDebug($"Unexpected token response format. Status Code: {(int)responseMessage.StatusCode}. Content-Type {responseMessage.Content.Headers.ContentType}.");
            }

            // Error handling:
            // 1. If the response body can't be parsed as json, throws.
            // 2. If the response's status code is not in 2XX range, throw OpenIdConnectProtocolException. If the body is correct parsed,
            //    pass the error information from body to the exception.
            OpenIdConnectMessage message;
            try
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                message = new OpenIdConnectMessage(responseContent);
            }
            catch (Exception ex)
            {
                throw new OpenIdConnectProtocolException($"Failed to parse token response body as JSON. Status Code: {(int)responseMessage.StatusCode}. Content-Type: {responseMessage.Content.Headers.ContentType}", ex);
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw CreateOpenIdConnectProtocolException(message, responseMessage);
            }

            return message;
        }

        /// <summary>
        /// Goes to UserInfo endpoint to retrieve additional claims and add any unique claims to the given identity.
        /// </summary>
        /// <param name="message">message that is being processed</param>
        /// <param name="jwt">The <see cref="JwtSecurityToken"/>.</param>
        /// <param name="ticket">authentication ticket with claims principal and identities</param>
        /// <returns>Authentication ticket with identity with additional claims, if any.</returns>
        protected virtual async Task<AuthenticateResult> GetUserInformationAsync(OpenIdConnectMessage message, JwtSecurityToken jwt, AuthenticationTicket ticket)
        {
            var userInfoEndpoint = _configuration?.UserInfoEndpoint;

            if (string.IsNullOrEmpty(userInfoEndpoint))
            {
                Logger.UserInfoEndpointNotSet();
                return AuthenticateResult.Success(ticket);
            }
            if (string.IsNullOrEmpty(message.AccessToken))
            {
                Logger.AccessTokenNotAvailable();
                return AuthenticateResult.Success(ticket);
            }
            Logger.RetrievingClaims();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", message.AccessToken);
            var responseMessage = await Backchannel.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
            var userInfoResponse = await responseMessage.Content.ReadAsStringAsync();

            JObject user;
            var contentType = responseMessage.Content.Headers.ContentType;
            if (contentType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                user = JObject.Parse(userInfoResponse);
            }
            else if (contentType.MediaType.Equals("application/jwt", StringComparison.OrdinalIgnoreCase))
            {
                var userInfoEndpointJwt = new JwtSecurityToken(userInfoResponse);
                user = JObject.FromObject(userInfoEndpointJwt.Payload);
            }
            else
            {
                return AuthenticateResult.Fail("Unknown response type: " + contentType.MediaType);
            }

            var userInformationReceivedContext = await RunUserInformationReceivedEventAsync(ticket, message, user);
            AuthenticateResult result;
            if (userInformationReceivedContext.IsProcessingComplete(out result))
            {
                return result;
            }
            ticket = userInformationReceivedContext.Ticket;
            user = userInformationReceivedContext.User;

            Options.ProtocolValidator.ValidateUserInfoResponse(new OpenIdConnectProtocolValidationContext()
            {
                UserInfoEndpointResponse = userInfoResponse,
                ValidatedIdToken = jwt,
            });

            var identity = (ClaimsIdentity)ticket.Principal.Identity;

            foreach (var action in Options.ClaimActions)
            {
                action.Run(user, identity, ClaimsIssuer);
            }

            return AuthenticateResult.Success(ticket);
        }

        /// <summary>
        /// Save the tokens contained in the <see cref="OpenIdConnectMessage"/> in the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="properties">The <see cref="AuthenticationProperties"/> in which tokens are saved.</param>
        /// <param name="message">The OpenID Connect response.</param>
        private void SaveTokens(AuthenticationProperties properties, OpenIdConnectMessage message)
        {
            var tokens = new List<AuthenticationToken>();

            if (!string.IsNullOrEmpty(message.AccessToken))
            {
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.AccessToken, Value = message.AccessToken });
            }

            if (!string.IsNullOrEmpty(message.IdToken))
            {
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.IdToken, Value = message.IdToken });
            }

            if (!string.IsNullOrEmpty(message.RefreshToken))
            {
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.RefreshToken, Value = message.RefreshToken });
            }

            if (!string.IsNullOrEmpty(message.TokenType))
            {
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.TokenType, Value = message.TokenType });
            }

            if (!string.IsNullOrEmpty(message.ExpiresIn))
            {
                int value;
                if (int.TryParse(message.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    var expiresAt = Clock.UtcNow + TimeSpan.FromSeconds(value);
                    // https://www.w3.org/TR/xmlschema-2/#dateTime
                    // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
                    tokens.Add(new AuthenticationToken { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });
                }
            }

            properties.StoreTokens(tokens);
        }

        /// <summary>
        /// Adds the nonce to <see cref="HttpResponse.Cookies"/>.
        /// </summary>
        /// <param name="nonce">the nonce to remember.</param>
        /// <remarks><see cref="M:IResponseCookies.Append"/> of <see cref="HttpResponse.Cookies"/> is called to add a cookie with the name: 'OpenIdConnectAuthenticationDefaults.Nonce + <see cref="M:ISecureDataFormat{TData}.Protect"/>(nonce)' of <see cref="OpenIdConnectOptions.StringDataFormat"/>.
        /// The value of the cookie is: "N".</remarks>
        private void WriteNonceCookie(string nonce)
        {
            if (string.IsNullOrEmpty(nonce))
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            Response.Cookies.Append(
                OpenIdConnectDefaults.CookieNoncePrefix + Options.StringDataFormat.Protect(nonce),
                NonceProperty,
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = Http.SameSiteMode.None,
                    Secure = Request.IsHttps,
                    Expires = Clock.UtcNow.Add(Options.ProtocolValidator.NonceLifetime)
                });
        }

        /// <summary>
        /// Searches <see cref="HttpRequest.Cookies"/> for a matching nonce.
        /// </summary>
        /// <param name="nonce">the nonce that we are looking for.</param>
        /// <returns>echos 'nonce' if a cookie is found that matches, null otherwise.</returns>
        /// <remarks>Examine <see cref="IRequestCookieCollection.Keys"/> of <see cref="HttpRequest.Cookies"/> that start with the prefix: 'OpenIdConnectAuthenticationDefaults.Nonce'.
        /// <see cref="M:ISecureDataFormat{TData}.Unprotect"/> of <see cref="OpenIdConnectOptions.StringDataFormat"/> is used to obtain the actual 'nonce'. If the nonce is found, then <see cref="M:IResponseCookies.Delete"/> of <see cref="HttpResponse.Cookies"/> is called.</remarks>
        private string ReadNonceCookie(string nonce)
        {
            if (nonce == null)
            {
                return null;
            }

            foreach (var nonceKey in Request.Cookies.Keys)
            {
                if (nonceKey.StartsWith(OpenIdConnectDefaults.CookieNoncePrefix))
                {
                    try
                    {
                        var nonceDecodedValue = Options.StringDataFormat.Unprotect(nonceKey.Substring(OpenIdConnectDefaults.CookieNoncePrefix.Length, nonceKey.Length - OpenIdConnectDefaults.CookieNoncePrefix.Length));
                        if (nonceDecodedValue == nonce)
                        {
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                SameSite = Http.SameSiteMode.None,
                                Secure = Request.IsHttps
                            };

                            Response.Cookies.Delete(nonceKey, cookieOptions);
                            return nonce;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.UnableToProtectNonceCookie(ex);
                    }
                }
            }

            return null;
        }

        private AuthenticationProperties GetPropertiesFromState(string state)
        {
            // assume a well formed query string: <a=b&>OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey=kasjd;fljasldkjflksdj<&c=d>
            var startIndex = 0;
            if (string.IsNullOrEmpty(state) || (startIndex = state.IndexOf(OpenIdConnectDefaults.AuthenticationPropertiesKey, StringComparison.Ordinal)) == -1)
            {
                return null;
            }

            var authenticationIndex = startIndex + OpenIdConnectDefaults.AuthenticationPropertiesKey.Length;
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

        private async Task<MessageReceivedContext> RunMessageReceivedEventAsync(OpenIdConnectMessage message, AuthenticationProperties properties)
        {
            Logger.MessageReceived(message.BuildRedirectUrl());
            var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options)
            {
                ProtocolMessage = message,
                Properties = properties,
            };

            await Events.MessageReceived(messageReceivedContext);
            if (messageReceivedContext.HandledResponse)
            {
                Logger.MessageReceivedContextHandledResponse();
            }
            else if (messageReceivedContext.Skipped)
            {
                Logger.MessageReceivedContextSkipped();
            }

            return messageReceivedContext;
        }

        private async Task<TokenValidatedContext> RunTokenValidatedEventAsync(OpenIdConnectMessage authorizationResponse, OpenIdConnectMessage tokenEndpointResponse, AuthenticationProperties properties, AuthenticationTicket ticket, JwtSecurityToken jwt, string nonce)
        {
            var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
            {
                ProtocolMessage = authorizationResponse,
                TokenEndpointResponse = tokenEndpointResponse,
                Properties = properties,
                Ticket = ticket,
                SecurityToken = jwt,
                Nonce = nonce,
            };

            await Events.TokenValidated(tokenValidatedContext);
            if (tokenValidatedContext.HandledResponse)
            {
                Logger.TokenValidatedHandledResponse();
            }
            else if (tokenValidatedContext.Skipped)
            {
                Logger.TokenValidatedSkipped();
            }

            return tokenValidatedContext;
        }

        private async Task<AuthorizationCodeReceivedContext> RunAuthorizationCodeReceivedEventAsync(OpenIdConnectMessage authorizationResponse, AuthenticationProperties properties, AuthenticationTicket ticket, JwtSecurityToken jwt)
        {
            Logger.AuthorizationCodeReceived();

            var tokenEndpointRequest = new OpenIdConnectMessage()
            {
                ClientId = Options.ClientId,
                ClientSecret = Options.ClientSecret,
                Code = authorizationResponse.Code,
                GrantType = OpenIdConnectGrantTypes.AuthorizationCode,
                EnableTelemetryParameters = !Options.DisableTelemetry,
                RedirectUri = properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]
            };

            var authorizationCodeReceivedContext = new AuthorizationCodeReceivedContext(Context, Scheme, Options)
            {
                ProtocolMessage = authorizationResponse,
                Properties = properties,
                TokenEndpointRequest = tokenEndpointRequest,
                Ticket = ticket,
                JwtSecurityToken = jwt,
                Backchannel = Backchannel,
            };

            await Events.AuthorizationCodeReceived(authorizationCodeReceivedContext);
            if (authorizationCodeReceivedContext.HandledResponse)
            {
                Logger.AuthorizationCodeReceivedContextHandledResponse();
            }
            else if (authorizationCodeReceivedContext.Skipped)
            {
                Logger.AuthorizationCodeReceivedContextSkipped();
            }

            return authorizationCodeReceivedContext;
        }

        private async Task<TokenResponseReceivedContext> RunTokenResponseReceivedEventAsync(
            OpenIdConnectMessage message,
            OpenIdConnectMessage tokenEndpointResponse,
            AuthenticationProperties properties,
            AuthenticationTicket ticket)
        {
            Logger.TokenResponseReceived();
            var eventContext = new TokenResponseReceivedContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = message,
                TokenEndpointResponse = tokenEndpointResponse,
                Ticket = ticket
            };

            await Events.TokenResponseReceived(eventContext);
            if (eventContext.HandledResponse)
            {
                Logger.TokenResponseReceivedHandledResponse();
            }
            else if (eventContext.Skipped)
            {
                Logger.TokenResponseReceivedSkipped();
            }

            return eventContext;
        }

        private async Task<UserInformationReceivedContext> RunUserInformationReceivedEventAsync(AuthenticationTicket ticket, OpenIdConnectMessage message, JObject user)
        {
            Logger.UserInformationReceived(user.ToString());

            var userInformationReceivedContext = new UserInformationReceivedContext(Context, Scheme, Options)
            {
                Ticket = ticket,
                ProtocolMessage = message,
                User = user,
            };

            await Events.UserInformationReceived(userInformationReceivedContext);
            if (userInformationReceivedContext.HandledResponse)
            {
                Logger.UserInformationReceivedHandledResponse();
            }
            else if (userInformationReceivedContext.Skipped)
            {
                Logger.UserInformationReceivedSkipped();
            }

            return userInformationReceivedContext;
        }

        private async Task<AuthenticationFailedContext> RunAuthenticationFailedEventAsync(OpenIdConnectMessage message, Exception exception)
        {
            var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
            {
                ProtocolMessage = message,
                Exception = exception
            };

            await Events.AuthenticationFailed(authenticationFailedContext);
            if (authenticationFailedContext.HandledResponse)
            {
                Logger.AuthenticationFailedContextHandledResponse();
            }
            else if (authenticationFailedContext.Skipped)
            {
                Logger.AuthenticationFailedContextSkipped();
            }

            return authenticationFailedContext;
        }

        private AuthenticationTicket ValidateToken(string idToken, AuthenticationProperties properties, TokenValidationParameters validationParameters, out JwtSecurityToken jwt)
        {
            if (!Options.SecurityTokenValidator.CanReadToken(idToken))
            {
                Logger.UnableToReadIdToken(idToken);
                throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.UnableToValidateToken, idToken));
            }

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
            var principal = Options.SecurityTokenValidator.ValidateToken(idToken, validationParameters, out validatedToken);
            jwt = validatedToken as JwtSecurityToken;
            if (jwt == null)
            {
                Logger.InvalidSecurityTokenType(validatedToken?.GetType().ToString());
                throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.ValidatedSecurityTokenNotJwt, validatedToken?.GetType()));
            }

            if (validatedToken == null)
            {
                Logger.UnableToValidateIdToken(idToken);
                throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.UnableToValidateToken, idToken));
            }

            var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);

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

        /// <summary>
        /// Build a redirect path if the given path is a relative path.
        /// </summary>
        private string BuildRedirectUriIfRelative(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return uri;
            }

            if (!uri.StartsWith("/", StringComparison.Ordinal))
            {
                return uri;
            }

            return BuildRedirectUri(uri);
        }

        private OpenIdConnectProtocolException CreateOpenIdConnectProtocolException(OpenIdConnectMessage message, HttpResponseMessage response)
        {
            var description = message.ErrorDescription ?? "error_description is null";
            var errorUri = message.ErrorUri ?? "error_uri is null";

            if (response != null)
            {
                Logger.ResponseErrorWithStatusCode(message.Error, description, errorUri, (int)response.StatusCode);
            }
            else
            {
                Logger.ResponseError(message.Error, description, errorUri);
            }

            return new OpenIdConnectProtocolException(string.Format(
                CultureInfo.InvariantCulture,
                Resources.MessageContainsError,
                message.Error,
                description,
                errorUri));
        }
    }
}
