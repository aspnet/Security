// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

namespace Microsoft.AspNetCore.Authentication2
{
    /// <summary>
    /// Provides context information to middleware providers.
    /// </summary>
    public class TicketReceivedContext : BaseControlContext
    {
        public TicketReceivedContext(HttpContext context, RemoteAuthenticationOptions options, AuthenticationTicket2 ticket)
            : base(context)
        {
            Options = options;
            Ticket = ticket;
            if (ticket != null)
            {
                Principal = ticket.Principal;
                Properties = ticket.Properties;
            }
        }

        public ClaimsPrincipal Principal { get; set; }
        public AuthenticationProperties2 Properties { get; set; }
        public RemoteAuthenticationOptions Options { get; set; }

        public string ReturnUri { get; set; }
    }
}
