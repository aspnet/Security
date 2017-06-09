// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    /// <summary>
    /// Context object passed to the ICookieAuthenticationEvents method SignedIn.
    /// </summary>    
    public class CookieSignedInContext : CookieResultContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="context">The HTTP request context</param>
        /// <param name="ticket">Initializes Ticket property</param>
        public CookieSignedInContext(CookieAuthenticationHandler handler, HttpContext context, AuthenticationTicket ticket) : base(handler, context, ticket)
        { }
    }
}
