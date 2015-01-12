// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

namespace Microsoft.AspNet.Security.OAuth
{
    internal class OAuthBearerAuthenticationHandler : AuthenticationHandler<OAuthBearerAuthenticationOptions>
    {
        private const string HandledResponse = "HandledResponse";

        private readonly ILogger _logger;
        private readonly string _challenge;
        private OpenIdConnectConfiguration _configuration;

        public OAuthBearerAuthenticationHandler(ILogger logger, string challenge)
        {
            _logger = logger;
            _challenge = challenge;
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
            OAuthRequestTokenContext requestTokenContext = null;
            try
            {
                // Find token in default location
                requestTokenContext = new OAuthRequestTokenContext(Context, null);

                // Give application opportunity to find from a different location, adjust, or reject token
                var messageReceivedNotification =
                    new MessageReceivedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = requestTokenContext,
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

                if (string.IsNullOrEmpty(requestTokenContext.Token))
                {
                    string authorization = Request.Headers.Get("Authorization");
                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        requestTokenContext.Token = authorization.Substring("Bearer ".Length).Trim();
                    }
                }

                // If no token found, no further work possible
                if (string.IsNullOrEmpty(requestTokenContext.Token))
                {
                    return null;
                }

                // notify user token was received
                var securityTokenReceivedNotification =
                new SecurityTokenReceivedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>(Context, Options)
                {
                    ProtocolMessage = requestTokenContext,
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

                if (_configuration == null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                var validationParameters = Options.TokenValidationParameters.Clone();
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
                SecurityToken validatedToken;
                foreach (var validator in Options.SecurityTokenValidators)
                {
                    if (validator.CanReadToken(requestTokenContext.Token))
                    {
                        ClaimsPrincipal principal = Options.SecurityTokenValidators.First().ValidateToken(requestTokenContext.Token, validationParameters, out validatedToken);
                        ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;
                        AuthenticationTicket ticket = new AuthenticationTicket(claimsIdentity, new AuthenticationProperties());
                        var securityTokenValidatedNotification = new SecurityTokenValidatedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>(Context, Options)
                        {
                            ProtocolMessage = requestTokenContext,
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

                throw new InvalidOperationException("No SecurityTokenValidator available for token: " + requestTokenContext.Token);
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
                    new AuthenticationFailedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = requestTokenContext,
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
            ApplyResponseChallengeAsync().GetAwaiter().GetResult();
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            if (ChallengeContext != null)
            {
                OAuthChallengeContext challengeContext = new OAuthChallengeContext(Context, _challenge);
                await Options.Notifications.ApplyChallenge(challengeContext);
            }

            return;
        }

        protected override void ApplyResponseGrant()
        {
            // N/A
        }
    }
}
