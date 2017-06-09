// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.OAuth
{
    /// <summary>
    /// Context passed when a Challenge causes a redirect to authorize endpoint in the handler.
    /// </summary>
    public class OAuthRedirectToAuthorizationContext : HandlerContext<IOAuthHandler>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="handler">The authentication handler.</param>
        /// <param name="context">The HTTP request context.</param>
        /// <param name="properties">The authentication properties of the challenge.</param>
        /// <param name="redirectUri">The initial redirect URI.</param>
        public OAuthRedirectToAuthorizationContext(
            IOAuthHandler handler,
            HttpContext context,
            AuthenticationProperties properties,
            string redirectUri)
            : base(handler, context)
        {
            Properties = properties ?? new AuthenticationProperties();
            RedirectUri = redirectUri;
        }

        /// <summary>
        /// Gets the URI used for the redirect operation.
        /// </summary>
        public string RedirectUri { get; private set; }
    }
}
