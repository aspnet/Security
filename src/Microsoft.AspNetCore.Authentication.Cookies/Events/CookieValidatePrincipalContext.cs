// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    /// <summary>
    /// Context object passed to the CookieAuthenticationEvents ValidatePrincipal method.
    /// </summary>
    public class CookieValidatePrincipalContext : CookieResultContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="context"></param>
        /// <param name="ticket">Contains the initial values for identity and extra data</param>
        public CookieValidatePrincipalContext(CookieAuthenticationHandler handler, HttpContext context, AuthenticationTicket ticket)
            : base(handler, context, ticket)
        { }

        /// <summary>
        /// If true, the cookie will be renewed
        /// </summary>
        public bool ShouldRenew { get; set; }

        /// <summary>
        /// Called to replace the claims principal. The supplied principal will replace the value of the 
        /// Principal property, which determines the identity of the authenticated request.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> used as the replacement</param>
        public void ReplacePrincipal(ClaimsPrincipal principal)
        {
            Ticket = new AuthenticationTicket(principal, Ticket?.Properties, Scheme.Name);
        }

        /// <summary>
        /// Called to reject the incoming principal. This may be done if the application has determined the
        /// account is no longer active, and the request should be treated as if it was anonymous.
        /// </summary>
        public void RejectPrincipal()
        {
            Ticket = null;
        }
    }
}
