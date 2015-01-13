// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.AspNet.Security.Notifications;
using Microsoft.Framework.Logging;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security.OAuthBearer
{
    internal class OAuthBearerAuthenticationHandler : AuthenticationHandler<OAuthBearerAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private OpenIdConnectConfiguration _configuration;

        public OAuthBearerAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override AuthenticationTicket AuthenticateCore()
        {
            return AuthenticateCoreAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Searches the 'Authorization' header for a 'Bearer' token. If the 'Bearer' token is found, it is validated using <see cref="TokenValidationParameters"/> set in the options.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            ExceptionDispatchInfo authFailedEx = null;
            string token = null;
            try
            {
                // Give application opportunity to find from a different location, adjust, or reject token
                var messageReceivedNotification =
                    new MessageReceivedNotification<HttpContext, OAuthBearerAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = Context,
                    };

                // notification can set the token
                await Options.Notifications.MessageReceived(messageReceivedNotification);
                if (messageReceivedNotification.HandledResponse)
                {
                    return messageReceivedNotification.AuthenticationTicket;
                }

                if (messageReceivedNotification.Skipped)
                {
                    return null;
                }

                string authorization = Request.Headers.Get("Authorization");
                if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                     token = authorization.Substring("Bearer ".Length).Trim();
                }

                // If no token found, no further work possible
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                // notify user token was received
                var securityTokenReceivedNotification =
                new SecurityTokenReceivedNotification<HttpContext, OAuthBearerAuthenticationOptions>(Context, Options)
                {
                    ProtocolMessage = Context,
                    SecurityToken = token,
                };

                await Options.Notifications.SecurityTokenReceived(securityTokenReceivedNotification);
                if (securityTokenReceivedNotification.HandledResponse)
                {
                    return securityTokenReceivedNotification.AuthenticationTicket;
                }

                if (securityTokenReceivedNotification.Skipped)
                {
                    return null;
                }

                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                var validationParameters = Options.TokenValidationParameters.Clone();
                if (_configuration != null)
                {
                    if (validationParameters.ValidIssuer == null && !string.IsNullOrWhiteSpace(_configuration.Issuer))
                    {
                        validationParameters.ValidIssuer = _configuration.Issuer;
                    }
                    else
                    {
                        IEnumerable<string> issuers = new[] { _configuration.Issuer };
                        validationParameters.ValidIssuers = (validationParameters.ValidIssuers == null ? issuers : validationParameters.ValidIssuers.Concat(issuers));
                    }

                    validationParameters.IssuerSigningKeys = (validationParameters.IssuerSigningKeys == null ? _configuration.SigningKeys : validationParameters.IssuerSigningKeys.Concat(_configuration.SigningKeys));
                }

                SecurityToken validatedToken;
                foreach (var validator in Options.SecurityTokenValidators)
                {
                    if (validator.CanReadToken(token))
                    {
                        ClaimsPrincipal principal = validator.ValidateToken(token, validationParameters, out validatedToken);
                        AuthenticationTicket ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationType);
                        var securityTokenValidatedNotification = new SecurityTokenValidatedNotification<HttpContext, OAuthBearerAuthenticationOptions>(Context, Options)
                        {
                            ProtocolMessage = Context,
                            AuthenticationTicket = ticket
                        };

                        if (securityTokenReceivedNotification.HandledResponse)
                        {
                            return securityTokenValidatedNotification.AuthenticationTicket;
                        }

                        if (securityTokenReceivedNotification.Skipped)
                        {
                            return null;
                        }

                        return ticket;
                    }
                }

                throw new InvalidOperationException("No SecurityTokenValidator available for token: " + token ?? "null");
            }
            catch (Exception ex)
            {
                // We can't await inside a catch block, capture and handle outside.
                authFailedEx = ExceptionDispatchInfo.Capture(ex);
            }

            if (authFailedEx != null)
            {
                _logger.WriteError("Exception occurred while processing message: '" + authFailedEx.ToString());

                // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the notification.
                if (Options.RefreshOnIssuerKeyNotFound && authFailedEx.SourceException.GetType().Equals(typeof(SecurityTokenSignatureKeyNotFoundException)))
                {
                    Options.ConfigurationManager.RequestRefresh();
                }

                var authenticationFailedNotification =
                    new AuthenticationFailedNotification<HttpContext, OAuthBearerAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = Context,
                        Exception = authFailedEx.SourceException
                    };

                await Options.Notifications.AuthenticationFailed(authenticationFailedNotification);
                if (authenticationFailedNotification.HandledResponse)
                {
                    return authenticationFailedNotification.AuthenticationTicket;
                }

                if (authenticationFailedNotification.Skipped)
                {
                    return null;
                }

                authFailedEx.Throw();
            }

            return null;
        }

        protected override void ApplyResponseChallenge()
        {
            // N/A
        }

        protected override void ApplyResponseGrant()
        {
            // N/A
        }
    }
}
