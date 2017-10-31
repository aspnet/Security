// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    public class SecondaryTokenValidatedContext : TokenValidatedContext
    {
        /// <summary>
        /// Creates a <see cref="TokenValidatedContext"/>
        /// </summary>
        public SecondaryTokenValidatedContext(HttpContext context, AuthenticationScheme scheme, OpenIdConnectOptions options, ClaimsPrincipal principal, AuthenticationProperties properties)
            : base(context, scheme, options, principal, properties) { }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> containing additional user claims.
        /// </summary>
        public ClaimsPrincipal SecondaryPrincipal { get; set; }

    }
}