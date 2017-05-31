// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    public class AzureAdOptions : OpenIdConnectOptions
    {
        // Set             oidcOptions.UseTokenLifetime = true; in config

        public string Instance { get; set; }
        public string TenantId { get; set; }
    }
}