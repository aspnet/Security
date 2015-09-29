// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNet.Http.Authentication;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Contains the result of an Authenticate call
    /// </summary>
    public class AuthenticateResult
    {


        /// <summary>
        /// The authentication ticket.
        /// </summary>
        public AuthenticationTicket Ticket { get; set; }

        /// <summary>
        /// Holds error information caused by authentication.
        /// </summary>
        public ErrorContext Error { get; set; }
    }
}
