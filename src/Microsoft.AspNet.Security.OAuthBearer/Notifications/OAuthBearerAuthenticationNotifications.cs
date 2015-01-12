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
		}

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedNotification<OAuthBearerTokenContext, OAuthBearerAuthenticationOptions>, Task> AuthenticationFailed { get; set; }

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public Func<MessageReceivedNotification<OAuthBearerTokenContext, OAuthBearerAuthenticationOptions>, Task> MessageReceived { get; set; }

        /// <summary>
        /// Invoked with the security token that has been extracted from the protocol message.
        /// </summary>
        public Func<SecurityTokenReceivedNotification<OAuthBearerTokenContext, OAuthBearerAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<SecurityTokenValidatedNotification<OAuthBearerTokenContext, OAuthBearerAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }
    }
}
