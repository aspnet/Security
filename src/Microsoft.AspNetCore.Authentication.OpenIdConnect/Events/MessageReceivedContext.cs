// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    public class MessageReceivedContext : RemoteResultContext<OpenIdConnectOptions>
    {
        public MessageReceivedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            OpenIdConnectOptions options)
            : base(context, scheme, options)
        {
        }

        public OpenIdConnectMessage AuthorizationResponse { get; set; }

        /// <summary>
        /// Bearer Token. This will give application an opportunity to retrieve token from an alternation location.
        /// </summary>
        public string Token { get; set; }
    }
}