// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Provides context information to middleware providers.
    /// </summary>
    public class TicketReceivedContext : BaseControlContext
    {
        public TicketReceivedContext(HttpContext context, AuthenticationTicket ticket)
            : base(context)
        {
            if (ticket != null)
            {
                Principal = ticket.Principal;
                Properties = ticket.Properties;
            }
        }

        public ClaimsPrincipal Principal { get; set; }
        public AuthenticationProperties Properties { get; set; }

        public string SignInScheme { get; set; }

        public string ReturnUri { get; set; }
    }
}
