// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication2
{
    /// <summary>
    /// Contains user identity information as well as additional authentication state.
    /// </summary>
    public class AuthenticationTicket2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationTicket2"/> class
        /// </summary>
        /// <param name="principal">the <see cref="ClaimsPrincipal"/> that represents the authenticated user.</param>
        /// <param name="properties">additional properties that can be consumed by the user or runtime.</param>
        /// <param name="authenticationScheme">the authentication middleware that was responsible for this ticket.</param>
        public AuthenticationTicket2(ClaimsPrincipal principal, AuthenticationProperties2 properties, string authenticationScheme)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            AuthenticationScheme = authenticationScheme;
            Principal = principal;
            Properties = properties ?? new AuthenticationProperties2();
        }

        /// <summary>
        /// Gets the authentication type.
        /// </summary>
        public string AuthenticationScheme { get; private set; }

        /// <summary>
        /// Gets the claims-principal with authenticated user identities.
        /// </summary>
        public ClaimsPrincipal Principal{ get; private set; }

        /// <summary>
        /// Additional state values for the authentication session.
        /// </summary>
        public AuthenticationProperties2 Properties { get; private set; }
    }
}
