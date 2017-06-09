// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    /// <summary>
    /// Context object passed to the ICookieAuthenticationEvents method SigningIn.
    /// </summary>    
    public class CookieSigningInContext : CookieResultContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="handler">The handler</param>
        /// <param name="context">The HTTP request context</param>
        /// <param name="ticket">Initializes Ticket property</param>
        /// <param name="cookieOptions">Initializes options for the authentication cookie.</param>
        public CookieSigningInContext(CookieAuthenticationHandler handler, HttpContext context, CookieOptions cookieOptions, AuthenticationTicket ticket)
            : base(handler, context, ticket)
            => CookieOptions = cookieOptions;

        /// <summary>
        /// The options for creating the outgoing cookie.
        /// May be replace or altered during the SigningIn call.
        /// </summary>
        public CookieOptions CookieOptions { get; set; }
    }
}
