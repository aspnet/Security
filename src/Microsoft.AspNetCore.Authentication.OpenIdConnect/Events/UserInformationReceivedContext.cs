// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    public class UserInformationReceivedContext : RemoteAuthenticateResultContext<OpenIdConnectOptions>
    {
        public UserInformationReceivedContext(HttpContext context, AuthenticationScheme scheme, OpenIdConnectOptions options)
            : base(context, scheme, options)
        {
        }

        public OpenIdConnectMessage ProtocolMessage { get; set; }

        public JObject User { get; set; }
    }
}
