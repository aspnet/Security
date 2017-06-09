// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    /// <summary>
    /// Context passed when a Challenge, SignIn, or SignOut causes a redirect in the cookie handler 
    /// </summary>
    public class CookieRedirectContext : HandlerContext<CookieAuthenticationHandler>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="handler">The cookie handler</param>
        /// <param name="context">The HTTP request context</param>
        /// <param name="redirectUri">The initial redirect URI</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        public CookieRedirectContext(CookieAuthenticationHandler handler, HttpContext context, string redirectUri, AuthenticationProperties properties)
            : base(handler, context)
        {
            RedirectUri = redirectUri;
            Properties = properties ?? new AuthenticationProperties();
        }

        public AuthenticationProperties Properties { get; set; }

        /// <summary>
        /// Gets or Sets the URI used for the redirect operation.
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
