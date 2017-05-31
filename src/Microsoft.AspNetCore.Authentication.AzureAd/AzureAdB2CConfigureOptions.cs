// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    internal class AzureAdB2CConfigureOptions : ConfigureDefaultOptions<AzureAdB2COptions>
    {
        private readonly IConfiguration _config;

        public AzureAdB2CConfigureOptions(IConfiguration config) => _config = config;

        public override void Configure(string name, AzureAdB2COptions options)
        {
            _config.GetSection("Microsoft:AspNetCore:Authentication:Schemes:" + AzureDefaults.AzureAdB2CAuthenticationScheme).Bind(options);
        }
    }
}
