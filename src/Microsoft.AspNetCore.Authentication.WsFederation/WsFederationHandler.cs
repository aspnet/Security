﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Authentication.WsFederation
{
    /// <summary>
    /// A per-request authentication handler for the WsFederation.
    /// </summary>
    public class WsFederationHandler : RemoteAuthenticationHandler<WsFederationOptions>, IAuthenticationSignOutHandler
    {
        private WsFederationConfiguration _configuration;

        /// <summary>
        /// Creates a new WsFederationAuthenticationHandler
        /// </summary>
        /// <param name="options"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="logger"></param>
        public WsFederationHandler(IOptionsMonitor<WsFederationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        /// <summary>
        /// Handles Challenge
        /// </summary>
        /// <returns></returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (_configuration == null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
            }

            string baseUri =
                    Request.Scheme +
                    Uri.SchemeDelimiter +
                    Request.Host +
                    Request.PathBase;

            string currentUri =
                baseUri +
                Request.Path +
                Request.QueryString;

            // Save the original challenge URI so we can redirect back to it when we're done.
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = currentUri;
            }

            WsFederationMessage wsFederationMessage = new WsFederationMessage()
            {
                IssuerAddress = _configuration.TokenEndpoint ?? string.Empty,
                Wtrealm = Options.Wtrealm,
                Wa = WsFederationConstants.WsFederationActions.SignIn,
            };

            if (!string.IsNullOrEmpty(Options.Wreply))
            {
                wsFederationMessage.Wreply = Options.Wreply;
            }
            else
            {
                wsFederationMessage.Wreply = BuildRedirectUri(Options.CallbackPath);
            }

            var redirectContext = new RedirectContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = wsFederationMessage
            };
            await Options.Events.RedirectToIdentityProvider(redirectContext);

            if (redirectContext.Handled)
            {
                return;
            }

            wsFederationMessage = redirectContext.ProtocolMessage;

            if (!string.IsNullOrEmpty(wsFederationMessage.Wctx))
            {
                properties.Items[WsFederationDefaults.UserstatePropertiesKey] = wsFederationMessage.Wctx;
            }

            wsFederationMessage.Wctx = Uri.EscapeDataString(Options.StateDataFormat.Protect(properties));

            string redirectUri = wsFederationMessage.CreateSignInUrl();
            if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
            {
                Logger.MalformedRedirectUri(redirectUri);
            }
            Response.Redirect(redirectUri);
        }

        /// <summary>
        /// Invoked to process incoming authentication messages.
        /// </summary>
        /// <returns></returns>
        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            WsFederationMessage wsFederationMessage = null;

            // assumption: if the ContentType is "application/x-www-form-urlencoded" it should be safe to read as it is small.
            if (HttpMethods.IsPost(Request.Method)
              && !string.IsNullOrEmpty(Request.ContentType)
              // May have media/type; charset=utf-8, allow partial match.
              && Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
              && Request.Body.CanRead)
            {
                var form = await Request.ReadFormAsync();
    
                wsFederationMessage = new WsFederationMessage(form.Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
            }

            if (wsFederationMessage == null || !wsFederationMessage.IsSignInMessage)
            {
                if (Options.SkipUnrecognizedRequests)
                {
                    // Not for us?
                    return HandleRequestResult.SkipHandler();
                }

                return HandleRequestResult.Fail("No message.");
            }
            
            try
            {
                // Retrieve our cached redirect uri
                var state = wsFederationMessage.Wctx;
                // WsFed allows for uninitiated logins, state may be missing.
                var properties = Options.StateDataFormat.Unprotect(state);

                if (properties == null)
                {
                    properties = new AuthenticationProperties();
                }
                else
                {
                    // Extract the user state from properties and reset.
                    properties.Items.TryGetValue(WsFederationDefaults.UserstatePropertiesKey, out var userState);
                    wsFederationMessage.Wctx = userState;
                }

                var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options, properties)
                {
                    ProtocolMessage = wsFederationMessage
                };
                await Options.Events.MessageReceived(messageReceivedContext);
                if (messageReceivedContext.Result != null)
                {
                    return messageReceivedContext.Result;
                }

                if (wsFederationMessage.Wresult == null)
                {
                    Logger.SignInWithoutWresult();
                    return HandleRequestResult.Fail(Resources.SignInMessageWresultIsMissing);
                }

                var token = wsFederationMessage.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    Logger.SignInWithoutToken();
                    return HandleRequestResult.Fail(Resources.SignInMessageTokenIsMissing);
                }

                var securityTokenReceivedContext = new SecurityTokenReceivedContext(Context, Scheme, Options, properties)
                {
                    ProtocolMessage = wsFederationMessage
                };
                await Options.Events.SecurityTokenReceived(securityTokenReceivedContext);
                if (securityTokenReceivedContext.Result != null)
                {
                    return securityTokenReceivedContext.Result;
                }

                if (_configuration == null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                // Copy and augment to avoid cross request race conditions for updated configurations.
                var tvp = Options.TokenValidationParameters.Clone();
                var issuers = new[] { _configuration.Issuer };
                tvp.ValidIssuers = (tvp.ValidIssuers == null ? issuers : tvp.ValidIssuers.Concat(issuers));
                tvp.IssuerSigningKeys = (tvp.IssuerSigningKeys == null ? _configuration.SigningKeys : tvp.IssuerSigningKeys.Concat(_configuration.SigningKeys));

                ClaimsPrincipal principal = null;
                SecurityToken parsedToken = null;
                foreach (var validator in Options.SecurityTokenHandlers)
                {
                    if (validator.CanReadToken(token))
                    {
                        principal = validator.ValidateToken(token, tvp, out parsedToken);
                        break;
                    }
                }

                if (principal == null)
                {
                    throw new SecurityTokenException(Resources.Exception_NoTokenValidatorFound);
                }

                if (Options.UseTokenLifetime && parsedToken != null)
                {
                    // Override any session persistence to match the token lifetime.
                    var issued = parsedToken.ValidFrom;
                    if (issued != DateTime.MinValue)
                    {
                        properties.IssuedUtc = issued.ToUniversalTime();
                    }
                    var expires = parsedToken.ValidTo;
                    if (expires != DateTime.MinValue)
                    {
                        properties.ExpiresUtc = expires.ToUniversalTime();
                    }
                    properties.AllowRefresh = false;
                }

                var securityTokenValidatedContext = new SecurityTokenValidatedContext(Context, Scheme, Options, principal, properties)
                {
                    ProtocolMessage = wsFederationMessage,
                    SecurityToken = parsedToken,
                };

                await Options.Events.SecurityTokenValidated(securityTokenValidatedContext);
                if (securityTokenValidatedContext.Result != null)
                {
                    return securityTokenValidatedContext.Result;
                }

                // Flow possible changes
                principal = securityTokenValidatedContext.Principal;
                properties = securityTokenValidatedContext.Properties;

                return HandleRequestResult.Success(new AuthenticationTicket(principal, properties, Scheme.Name));
            }
            catch (Exception exception)
            {
                Logger.ExceptionProcessingMessage(exception);

                // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the notification.
                if (Options.RefreshOnIssuerKeyNotFound && exception.GetType().Equals(typeof(SecurityTokenSignatureKeyNotFoundException)))
                {
                    Options.ConfigurationManager.RequestRefresh();
                }

                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                {
                    ProtocolMessage = wsFederationMessage,
                    Exception = exception
                };
                await Options.Events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }

        /// <summary>
        /// Handles Signout
        /// </summary>
        /// <returns></returns>
        public async virtual Task SignOutAsync(AuthenticationProperties properties)
        {
            if (_configuration == null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
            }

            var wsFederationMessage = new WsFederationMessage()
            {
                IssuerAddress = _configuration.TokenEndpoint ?? string.Empty,
                Wtrealm = Options.Wtrealm,
                Wa = WsFederationConstants.WsFederationActions.SignOut,
            };

            // Set Wreply in order:
            // 1. properties.Redirect
            // 2. Options.SignOutWreply
            // 3. Options.Wreply
            if (properties != null && !string.IsNullOrEmpty(properties.RedirectUri))
            {
                wsFederationMessage.Wreply = BuildRedirectUriIfRelative(properties.RedirectUri);
            }
            else if (!string.IsNullOrEmpty(Options.SignOutWreply))
            {
                wsFederationMessage.Wreply = BuildRedirectUriIfRelative(Options.SignOutWreply);
            }
            else if (!string.IsNullOrEmpty(Options.Wreply))
            {
                wsFederationMessage.Wreply = BuildRedirectUriIfRelative(Options.Wreply);
            }

            var redirectContext = new RedirectContext(Context, Scheme, Options, properties)
            {
                ProtocolMessage = wsFederationMessage
            };
            await Options.Events.RedirectToIdentityProvider(redirectContext);

            if (!redirectContext.Handled)
            {
                var redirectUri = redirectContext.ProtocolMessage.CreateSignOutUrl();
                if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    Logger.MalformedRedirectUri(redirectUri);
                }
                Response.Redirect(redirectUri);
            }
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
    }
}