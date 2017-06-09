// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    public class CookieResultContext : ResultContext<CookieAuthenticationHandler>
    {
        public CookieResultContext(CookieAuthenticationHandler handler, HttpContext context, AuthenticationTicket ticket) : base(handler, context)
            => Ticket = ticket;

        /// <summary>
        /// Gets the options associated with the scheme.
        /// </summary>
        public CookieAuthenticationOptions Options => Handler.Options;

        /// <summary>
        /// The authentication scheme.
        /// </summary>
        public AuthenticationScheme Scheme => Handler.Scheme;
    }
}
