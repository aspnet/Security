// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Provides context information to handler providers.
    /// </summary>
    public class TicketReceivedContext : RemoteAuthenticateResultContext<RemoteAuthenticationOptions>
    {
        public TicketReceivedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            RemoteAuthenticationOptions options,
            AuthenticationTicket ticket)
            : base(context, scheme, options)
        {
            Ticket = ticket;
        }

        public string ReturnUri { get; set; }
    }
}
