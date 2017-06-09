// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    public class AuthenticationFailedContext : BaseRemoteAuthenticationContext<OpenIdConnectOptions>
    {
        public AuthenticationFailedContext(HttpContext context, AuthenticationScheme scheme, OpenIdConnectOptions options)
            : base(context, scheme, options)
        {
        }

        public OpenIdConnectMessage AuthorizationResponse { get; set; }

        public Exception Exception { get; set; }
    }
}