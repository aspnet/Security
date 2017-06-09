// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Provides context information to handler providers.
    /// </summary>
    public class TicketReceivedContext : RemoteResultContext<IAuthenticationHandler>
    {
        public TicketReceivedContext(IAuthenticationHandler handler, HttpContext context, AuthenticationTicket ticket)
            : base(handler, context)
        {
            Ticket = ticket;
        }

        public string ReturnUri { get; set; }
    }
}
