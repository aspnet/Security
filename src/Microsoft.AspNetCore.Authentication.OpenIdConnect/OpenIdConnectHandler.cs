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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    /// <summary>
    /// A per-request authentication handler for the OpenIdConnectAuthenticationMiddleware.
    /// </summary>
    public class OpenIdConnectHandler : RemoteAuthenticationHandler<OpenIdConnectOptions>, IAuthenticationSignOutHandler
    {
        private const string NonceProperty = "N";

        private const string HeaderValueEpocDate = "Thu, 01 Jan 1970 00:00:00 GMT";

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        private OpenIdConnectConfiguration _configuration;

        protected HttpClient Backchannel => Options.Backchannel;

        protected HtmlEncoder HtmlEncoder { get; }

        public OpenIdConnectHandler(IOptionsMonitor<OpenIdConnectOptions> options, ILoggerFactory logger, HtmlEncoder htmlEncoder, UrlEncoder encoder, ISystemClock clock)
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

            if (remoteSignOutContext.Result != null)
            {
                if (remoteSignOutContext.Result.Handled)
                {
                    Logger.RemoteSignOutHandledResponse();
                    return true;
                }
                if (remoteSignOutContext.Result.Skipped)
                {
                    Logger.RemoteSignOutSkipped();
                    return false;
                }
                if (remoteSignOutContext.Result.Failure != null)
                {
                    throw new InvalidOperationException("An error was returned from the RemoteSignOut event.", remoteSignOutContext.Result.Failure);
                }
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
        public async virtual Task SignOutAsync(AuthenticationProperties properties)
        {
            var target = ResolveTarget(Options.ForwardSignOut);
            if (target != null)
            {
                await Context.SignOutAsync(target, properties);
                return;
            }

            properties = properties ?? new AuthenticationProperties();

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
                properties.RedirectUri = BuildRedirectUriIfRelative(Options.SignedOutRedirectUri);
                if (string.IsNullOrWhiteSpace(properties.RedirectUri))
                {
                    properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
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
            if (redirectContext.Handled)
            {
                Logger.RedirectToIdentityProviderForSignOutHandledResponse();
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
                throw new InvalidOperationException("Cannot redirect to the end session endpoint, the configuration may be missing or invalid.");
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
                var content = message.BuildFormPost();
                var buffer = Encoding.UTF8.GetBytes(content);

                Response.ContentLength = buffer.Length;
                Response.ContentType = "text/html;charset=UTF-8";

                // Emit Cache-Control=no-cache to prevent client caching.
                Response.Headers[HeaderNames.CacheControl] = "no-cache";
                Response.Headers[HeaderNames.Pragma] = "no-cache";
                Response.Headers[HeaderNames.Expires] = HeaderValueEpocDate;

                await Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                throw new NotImplementedException($"An unsupported authentication method has been configured: {Options.AuthenticationMethod}");
            }

            Logger.SignedOut(Scheme.Name);
        }

        /// <summary>
        /// Response to the callback from OpenId provider after session ended.
        /// </summary>
        /// <returns>A task executing the callback procedure</returns>
        protected async virtual Task<bool> HandleSignOutCallbackAsync()
        {
            var message = new OpenIdConnectMessage(Request.Query.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
            AuthenticationProperties properties = null;
            if (!string.IsNullOrEmpty(message.State))
            {
                properties = Options.StateDataFormat.Unprotect(message.State);
            }

            var signOut = new RemoteSignOutContext(Context, Scheme, Options, message)
            {
                Properties = properties,
            };

            await Events.SignedOutCallbackRedirect(signOut);
            if (signOut.Result != null)
            {
                if (signOut.Result.Handled)
                {
                    Logger.SignoutCallbackRedirectHandledResponse();
                    return true;
                }
                if (signOut.Result.Skipped)
                {
                    Logger.SignoutCallbackRedirectSkipped();
                    return false;
                }
                if (signOut.Result.Failure != null)
                {
                    throw new InvalidOperationException("An error was returned from the SignedOutCallbackRedirect event.", signOut.Result.Failure);
                }
            }

            properties = signOut.Properties;
            if (!string.IsNullOrEmpty(properties?.RedirectUri))
            {
                Response.Redirect(properties.RedirectUri);
            }

            return true;
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
                properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
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
                Prompt = properties.GetParameter<string>(OpenIdConnectParameterNames.Prompt) ?? Options.Prompt,
                Scope = string.Join(" ", properties.GetParameter<ICollection<string>>(OpenIdConnectParameterNames.Scope) ?? Options.Scope),
            };

            // Add the 'max_age' parameter to the authentication request if MaxAge is not null.
            // See http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
            var maxAge = properties.GetParameter<TimeSpan?>(OpenIdConnectParameterNames.MaxAge) ?? Options.MaxAge;
            if (maxAge.HasValue)
            {
                message.MaxAge = Convert.ToInt64(Math.Floor((maxAge.Value).TotalSeconds))
                    .ToString(CultureInfo.InvariantCulture);
            }

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
            if (redirectContext.Handled)
            {
                Logger.RedirectToIdentityProviderHandledResponse();
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
                var content = message.BuildFormPost();
                var buffer = Encoding.UTF8.GetBytes(content);

                Response.ContentLength = buffer.Length;
                Response.ContentType = "text/html;charset=UTF-8";

                // Emit Cache-Control=no-cache to prevent client caching.
                Response.Headers[HeaderNames.CacheControl] = "no-cache";
                Response.Headers[HeaderNames.Pragma] = "no-cache";
                Response.Headers[HeaderNames.Expires] = HeaderValueEpocDate;

                await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                return;
            }

            throw new NotImplementedException($"An unsupported authentication method has been configured: {Options.AuthenticationMethod}");
        }

        /// <summary>
        /// Invoked to process incoming OpenIdConnect messages.
        /// </summary>
        /// <returns>An <see cref="HandleRequestResult"/>.</returns>
        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
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
                        return HandleRequestResult.SkipHandler();
                    }
                    return HandleRequestResult.Fail("An OpenID Connect response cannot contain an " +
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
                    return HandleRequestResult.SkipHandler();
                }
                return HandleRequestResult.Fail("No message.");
            }

            AuthenticationProperties properties = null;
            try
            {
                properties = ReadPropertiesAndClearState(authorizationResponse);

                var messageReceivedContext = await RunMessageReceivedEventAsync(authorizationResponse, properties);
                if (messageReceivedContext.Result != null)
                {
                    return messageReceivedContext.Result;
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
                            return HandleRequestResult.SkipHandler();
                        }
                        return HandleRequestResult.Fail(Resources.MessageStateIsNullOrEmpty);
                    }

                    properties = ReadPropertiesAndClearState(authorizationResponse);
                }

                if (properties == null)
                {
                    Logger.UnableToReadAuthorizationResponseState();
                    if (Options.SkipUnrecognizedRequests)
                    {
                        // Not for us?
                        return HandleRequestResult.SkipHandler();
                    }

                    // if state exists and we failed to 'unprotect' this is not a message we should process.
                    return HandleRequestResult.Fail(Resources.MessageStateIsInvalid);
                }

                if (!ValidateCorrelationId(properties))
                {
                    return HandleRequestResult.Fail("Correlation failed.", properties);
                }

                // if any of the error fields are set, throw error null
                if (!string.IsNullOrEmpty(authorizationResponse.Error))
                {
                    // Note: access_denied errors are special protocol errors indicating the user didn't
                    // approve the authorization demand requested by the remote authorization server.
                    // Since it's a frequent scenario (that is not caused by incorrect configuration),
                    // denied errors are handled differently using HandleAccessDeniedErrorAsync().
                    // Visit https://tools.ietf.org/html/rfc6749#section-4.1.2.1 for more information.
                    if (string.Equals(authorizationResponse.Error, "access_denied", StringComparison.Ordinal))
                    {
                        return await HandleAccessDeniedErrorAsync(properties);
                    }

                    return HandleRequestResult.Fail(CreateOpenIdConnectProtocolException(authorizationResponse, response: null), properties);
                }

                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    Logger.UpdatingConfiguration();
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                PopulateSessionProperties(authorizationResponse, properties);

                ClaimsPrincipal user = null;
                JwtSecurityToken jwt = null;
                string nonce = null;
                var validationParameters = Options.TokenValidationParameters.Clone();

                // Hybrid or Implicit flow
                if (!string.IsNullOrEmpty(authorizationResponse.IdToken))
                {
                    Logger.ReceivedIdToken();
                    user = ValidateToken(authorizationResponse.IdToken, properties, validationParameters, out jwt);

                    nonce = jwt.Payload.Nonce;
                    if (!string.IsNullOrEmpty(nonce))
                    {
                        nonce = ReadNonceCookie(nonce);
                    }

                    var tokenValidatedContext = await RunTokenValidatedEventAsync(authorizationResponse, null, user, properties, jwt, nonce);
                    if (tokenValidatedContext.Result != null)
                    {
                        return tokenValidatedContext.Result;
                    }
                    authorizationResponse = tokenValidatedContext.ProtocolMessage;
                    user = tokenValidatedContext.Principal;
                    properties = tokenValidatedContext.Properties;
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
                    var authorizationCodeReceivedContext = await RunAuthorizationCodeReceivedEventAsync(authorizationResponse, user, properties, jwt);
                    if (authorizationCodeReceivedContext.Result != null)
                    {
                        return authorizationCodeReceivedContext.Result;
                    }
                    authorizationResponse = authorizationCodeReceivedContext.ProtocolMessage;
                    user = authorizationCodeReceivedContext.Principal;
                    properties = authorizationCodeReceivedContext.Properties;
                    var tokenEndpointRequest = authorizationCodeReceivedContext.TokenEndpointRequest;
                    // If the developer redeemed the code themselves...
                    tokenEndpointResponse = authorizationCodeReceivedContext.TokenEndpointResponse;
                    jwt = authorizationCodeReceivedContext.JwtSecurityToken;

                    if (!authorizationCodeReceivedContext.HandledCodeRedemption)
                    {
                        tokenEndpointResponse = await RedeemAuthorizationCodeAsync(tokenEndpointRequest);
                    }

                    var tokenResponseReceivedContext = await RunTokenResponseReceivedEventAsync(authorizationResponse, tokenEndpointResponse, user, properties);
                    if (tokenResponseReceivedContext.Result != null)
                    {
                        return tokenResponseReceivedContext.Result;
                    }

                    authorizationResponse = tokenResponseReceivedContext.ProtocolMessage;
                    tokenEndpointResponse = tokenResponseReceivedContext.TokenEndpointResponse;
                    user = tokenResponseReceivedContext.Principal;
                    properties = tokenResponseReceivedContext.Properties;

                    // no need to validate signature when token is received using "code flow" as per spec
                    // [http://openid.net/specs/openid-connect-core-1_0.html#IDTokenValidation].
                    validationParameters.RequireSignedTokens = false;

                    // At least a cursory validation is required on the new IdToken, even if we've already validated the one from the authorization response.
                    // And we'll want to validate the new JWT in ValidateTokenResponse.
                    var tokenEndpointUser = ValidateToken(tokenEndpointResponse.IdToken, properties, validationParameters, out var tokenEndpointJwt);

                    // Avoid reading & deleting the nonce cookie, running the event, etc, if it was already done as part of the authorization response validation.
                    if (user == null)
                    {
                        nonce = tokenEndpointJwt.Payload.Nonce;
                        if (!string.IsNullOrEmpty(nonce))
                        {
                            nonce = ReadNonceCookie(nonce);
                        }

                        var tokenValidatedContext = await RunTokenValidatedEventAsync(authorizationResponse, tokenEndpointResponse, tokenEndpointUser, properties, tokenEndpointJwt, nonce);
                        if (tokenValidatedContext.Result != null)
                        {
                            return tokenValidatedContext.Result;
                        }
                        authorizationResponse = tokenValidatedContext.ProtocolMessage;
                        tokenEndpointResponse = tokenValidatedContext.TokenEndpointResponse;
                        user = tokenValidatedContext.Principal;
                        properties = tokenValidatedContext.Properties;
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
                    SaveTokens(properties, tokenEndpointResponse ?? authorizationResponse);
                }

                if (Options.GetClaimsFromUserInfoEndpoint)
                {
                    return await GetUserInformationAsync(tokenEndpointResponse ?? authorizationResponse, jwt, user, properties);
                }
                else
                {
                    var identity = (ClaimsIdentity)user.Identity;
                    foreach (var action in Options.ClaimActions)
                    {
                        action.Run(null, identity, ClaimsIssuer);
                    }
                }

                return HandleRequestResult.Success(new AuthenticationTicket(user, properties, Scheme.Name));
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
                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                return HandleRequestResult.Fail(exception, properties);
            }
        }

        private AuthenticationProperties ReadPropertiesAndClearState(OpenIdConnectMessage message)
        {
            AuthenticationProperties properties = null;
            if (!string.IsNullOrEmpty(message.State))
            {
                properties = Options.StateDataFormat.Unprotect(message.State);

                if (properties != null)
                {
                    // If properties can be decoded from state, clear the message state.
                    properties.Items.TryGetValue(OpenIdConnectDefaults.UserstatePropertiesKey, out var userstate);
                    message.State = userstate;
                }
            }
            return properties;
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
        /// <param name="principal">The claims principal and identities.</param>
        /// <param name="properties">The authentication properties.</param>
        /// <returns><see cref="HandleRequestResult"/> which is used to determine if the remote authentication was successful.</returns>
        protected virtual async Task<HandleRequestResult> GetUserInformationAsync(
            OpenIdConnectMessage message, JwtSecurityToken jwt,
            ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            var userInfoEndpoint = _configuration?.UserInfoEndpoint;

            if (string.IsNullOrEmpty(userInfoEndpoint))
            {
                Logger.UserInfoEndpointNotSet();
                return HandleRequestResult.Success(new AuthenticationTicket(principal, properties, Scheme.Name));
            }
            if (string.IsNullOrEmpty(message.AccessToken))
            {
                Logger.AccessTokenNotAvailable();
                return HandleRequestResult.Success(new AuthenticationTicket(principal, properties, Scheme.Name));
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
                return HandleRequestResult.Fail("Unknown response type: " + contentType.MediaType, properties);
            }

            var userInformationReceivedContext = await RunUserInformationReceivedEventAsync(principal, properties, message, user);
            if (userInformationReceivedContext.Result != null)
            {
                return userInformationReceivedContext.Result;
            }
            principal = userInformationReceivedContext.Principal;
            properties = userInformationReceivedContext.Properties;
            user = userInformationReceivedContext.User;

            Options.ProtocolValidator.ValidateUserInfoResponse(new OpenIdConnectProtocolValidationContext()
            {
                UserInfoEndpointResponse = userInfoResponse,
                ValidatedIdToken = jwt,
            });

            var identity = (ClaimsIdentity)principal.Identity;

            foreach (var action in Options.ClaimActions)
            {
                action.Run(user, identity, ClaimsIssuer);
            }

            return HandleRequestResult.Success(new AuthenticationTicket(principal, properties, Scheme.Name));
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
                if (int.TryParse(message.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
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

            var cookieOptions = Options.NonceCookie.Build(Context, Clock.UtcNow);

            Response.Cookies.Append(
                Options.NonceCookie.Name + Options.StringDataFormat.Protect(nonce),
                NonceProperty,
                cookieOptions);
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
                if (nonceKey.StartsWith(Options.NonceCookie.Name))
                {
                    try
                    {
                        var nonceDecodedValue = Options.StringDataFormat.Unprotect(nonceKey.Substring(Options.NonceCookie.Name.Length, nonceKey.Length - Options.NonceCookie.Name.Length));
                        if (nonceDecodedValue == nonce)
                        {
                            var cookieOptions = Options.NonceCookie.Build(Context, Clock.UtcNow);
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
            var context = new MessageReceivedContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = message,
            };

            await Events.MessageReceived(context);
            if (context.Result != null)
            {
                if (context.Result.Handled)
                {
                    Logger.MessageReceivedContextHandledResponse();
                }
                else if (context.Result.Skipped)
                {
                    Logger.MessageReceivedContextSkipped();
                }
            }

            return context;
        }

        private async Task<TokenValidatedContext> RunTokenValidatedEventAsync(OpenIdConnectMessage authorizationResponse, OpenIdConnectMessage tokenEndpointResponse, ClaimsPrincipal user, AuthenticationProperties properties, JwtSecurityToken jwt, string nonce)
        {
            var context = new TokenValidatedContext(Context, Scheme, Options, user, properties)
            {
                ProtocolMessage = authorizationResponse,
                TokenEndpointResponse = tokenEndpointResponse,
                SecurityToken = jwt,
                Nonce = nonce,
            };

            await Events.TokenValidated(context);
            if (context.Result != null)
            {
                if (context.Result.Handled)
                {
                    Logger.TokenValidatedHandledResponse();
                }
                else if (context.Result.Skipped)
                {
                    Logger.TokenValidatedSkipped();
                }
            }

            return context;
        }

        private async Task<AuthorizationCodeReceivedContext> RunAuthorizationCodeReceivedEventAsync(OpenIdConnectMessage authorizationResponse, ClaimsPrincipal user, AuthenticationProperties properties, JwtSecurityToken jwt)
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

            var context = new AuthorizationCodeReceivedContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = authorizationResponse,
                TokenEndpointRequest = tokenEndpointRequest,
                Principal = user,
                JwtSecurityToken = jwt,
                Backchannel = Backchannel
            };

            await Events.AuthorizationCodeReceived(context);
            if (context.Result != null)
            {
                if (context.Result.Handled)
                {
                    Logger.AuthorizationCodeReceivedContextHandledResponse();
                }
                else if (context.Result.Skipped)
                {
                    Logger.AuthorizationCodeReceivedContextSkipped();
                }
            }

            return context;
        }

        private async Task<TokenResponseReceivedContext> RunTokenResponseReceivedEventAsync(
            OpenIdConnectMessage message,
            OpenIdConnectMessage tokenEndpointResponse,
            ClaimsPrincipal user,
            AuthenticationProperties properties)
        {
            Logger.TokenResponseReceived();
            var context = new TokenResponseReceivedContext(Context, Scheme, Options, user, properties)
            {
                ProtocolMessage = message,
                TokenEndpointResponse = tokenEndpointResponse,
            };

            await Events.TokenResponseReceived(context);
            if (context.Result != null)
            {
                if (context.Result.Handled)
                {
                    Logger.TokenResponseReceivedHandledResponse();
                }
                else if (context.Result.Skipped)
                {
                    Logger.TokenResponseReceivedSkipped();
                }
            }

            return context;
        }

        private async Task<UserInformationReceivedContext> RunUserInformationReceivedEventAsync(ClaimsPrincipal principal, AuthenticationProperties properties, OpenIdConnectMessage message, JObject user)
        {
            Logger.UserInformationReceived(user.ToString());

            var context = new UserInformationReceivedContext(Context, Scheme, Options, principal, properties)
            {
                ProtocolMessage = message,
                User = user,
            };

            await Events.UserInformationReceived(context);
            if (context.Result != null)
            {
                if (context.Result.Handled)
                {
                    Logger.UserInformationReceivedHandledResponse();
                }
                else if (context.Result.Skipped)
                {
                    Logger.UserInformationReceivedSkipped();
                }
            }

            return context;
        }

        private async Task<AuthenticationFailedContext> RunAuthenticationFailedEventAsync(OpenIdConnectMessage message, Exception exception)
        {
            var context = new AuthenticationFailedContext(Context, Scheme, Options)
            {
                ProtocolMessage = message,
                Exception = exception
            };

            await Events.AuthenticationFailed(context);
            if (context.Result != null)
            {
                if (context.Result.Handled)
                {
                    Logger.AuthenticationFailedContextHandledResponse();
                }
                else if (context.Result.Skipped)
                {
                    Logger.AuthenticationFailedContextSkipped();
                }
            }

            return context;
        }

        // Note this modifies properties if Options.UseTokenLifetime
        private ClaimsPrincipal ValidateToken(string idToken, AuthenticationProperties properties, TokenValidationParameters validationParameters, out JwtSecurityToken jwt)
        {
            if (!Options.SecurityTokenValidator.CanReadToken(idToken))
            {
                Logger.UnableToReadIdToken(idToken);
                throw new SecurityTokenException(string.Format(CultureInfo.InvariantCulture, Resources.UnableToValidateToken, idToken));
            }

            if (_configuration != null)
            {
                var issuer = new[] { _configuration.Issuer };
                validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(issuer) ?? issuer;

                validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(_configuration.SigningKeys)
                    ?? _configuration.SigningKeys;
            }

            var principal = Options.SecurityTokenValidator.ValidateToken(idToken, validationParameters, out SecurityToken validatedToken);
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

            if (Options.UseTokenLifetime)
            {
                var issued = validatedToken.ValidFrom;
                if (issued != DateTime.MinValue)
                {
                    properties.IssuedUtc = issued;
                }

                var expires = validatedToken.ValidTo;
                if (expires != DateTime.MinValue)
                {
                    properties.ExpiresUtc = expires;
                }
            }

            return principal;
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
