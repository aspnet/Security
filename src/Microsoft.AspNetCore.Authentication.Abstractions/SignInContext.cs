// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    public class SignInContext : BaseAuthenticationContext
    {
        public SignInContext(HttpContext context, string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties properties)
            : base(context, authenticationScheme, properties)
        {
            Principal = principal ?? throw new ArgumentNullException(nameof(principal));
        }

        public ClaimsPrincipal Principal { get; }
    }
}