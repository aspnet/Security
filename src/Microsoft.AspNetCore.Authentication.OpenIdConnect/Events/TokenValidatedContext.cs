// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    public class TokenValidatedContext : RemoteAuthenticateResultContext<OpenIdConnectOptions>
    {
        /// <summary>
        /// Creates a <see cref="TokenValidatedContext"/>
        /// </summary>
        public TokenValidatedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            OpenIdConnectOptions options)
            : base(context, scheme, options)
        {
        }

        public OpenIdConnectMessage AuthorizationResponse { get; set; }

        public JwtSecurityToken SecurityToken { get; set; }

        public OpenIdConnectMessage TokenEndpointResponse { get; set; }

        public string Nonce { get; set; }
    }
}