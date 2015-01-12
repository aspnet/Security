// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Security.Notifications;
using System;
using System.Threading.Tasks;

/// <summary>
/// Specifies events which the <see cref="OAuthBearerAuthenticationMiddleware"></see> invokes to enable developer control over the authentication process. />
/// </summary>
namespace Microsoft.AspNet.Security.OAuth
{
    /// <summary>
    /// OAuth bearer token middleware provider
    /// </summary>
    public class OAuthBearerAuthenticationNotifications
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthBearerAuthenticationProvider"/> class
        /// </summary>
        public OAuthBearerAuthenticationNotifications()
        {
            AuthenticationFailed = notification => Task.FromResult(0);
            MessageReceived = notification => Task.FromResult(0);
            SecurityTokenReceived = notification => Task.FromResult(0);
            SecurityTokenValidated = notification => Task.FromResult(0);

            OnApplyChallenge = context =>
            {
                context.HttpContext.Response.Headers.AppendValues("WWW-Authenticate", context.Challenge);
                return Task.FromResult(0);
            };
		}

        /// <summary>
        /// Handles applying the authentication challenge to the response message.
        /// </summary>
        public Func<OAuthChallengeContext, Task> OnApplyChallenge { get; set; }

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>, Task> AuthenticationFailed { get; set; }

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public Func<MessageReceivedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>, Task> MessageReceived { get; set; }

        /// <summary>
        /// Invoked with the security token that has been extracted from the protocol message.
        /// </summary>
        public Func<SecurityTokenReceivedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<SecurityTokenValidatedNotification<OAuthRequestTokenContext, OAuthBearerAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }

        /// <summary>
        /// Handles applying the authentication challenge to the response message.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task ApplyChallenge(OAuthChallengeContext context)
        {
            return OnApplyChallenge(context);
        }
    }
}
