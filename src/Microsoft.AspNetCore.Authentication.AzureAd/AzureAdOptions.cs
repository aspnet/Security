// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    public class AzureAdOptions : OpenIdConnectOptions
    {
        public string Instance { get; set; }
        public string TenantId { get; set; }
    }
}