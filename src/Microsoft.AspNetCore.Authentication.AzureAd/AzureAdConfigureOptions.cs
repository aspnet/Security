// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    internal class AzureAdConfigureOptions : ConfigureDefaultOptions<AzureAdOptions>
    {
        public AzureAdConfigureOptions(IConfiguration config) :
            base(AzureDefaults.AzureAdAuthenticationScheme,
                options => config.GetSection("Microsoft:AspNetCore:Authentication:Schemes:"+ AzureDefaults.AzureAdAuthenticationScheme).Bind(options))
        { }
    }
}
