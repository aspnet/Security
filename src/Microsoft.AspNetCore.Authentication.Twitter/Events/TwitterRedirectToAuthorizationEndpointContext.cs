// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Twitter
{
    /// <summary>
    /// The Context passed when a Challenge causes a redirect to authorize endpoint in the Twitter handler.
    /// </summary>
    public class TwitterRedirectToAuthorizationEndpointContext : BaseContext<TwitterOptions>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <param name="scheme">The scheme data</param>
        /// <param name="options">The Twitter handler options.</param>
        /// <param name="properties">The authentication properties of the challenge.</param>
        /// <param name="redirectUri">The initial redirect URI.</param>
        public TwitterRedirectToAuthorizationEndpointContext(
            HttpContext context,
            AuthenticationScheme scheme,
            TwitterOptions options,
            AuthenticationProperties properties,
            string redirectUri)
            : base(context, scheme, options)
        {
            Properties = properties;
            RedirectUri = redirectUri;
        }

        /// <summary>
        /// Gets the URI used for the redirect operation.
        /// </summary>
        public string RedirectUri { get; private set; }
    }
}
