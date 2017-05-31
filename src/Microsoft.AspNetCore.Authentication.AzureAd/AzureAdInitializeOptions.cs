// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    internal class AzureAdInitializeOptions : IInitializeOptions<AzureAdOptions>
    {

        public AzureAdInitializeOptions() { }

        public void Initialize(string name, AzureAdOptions options)
        {
            if (string.IsNullOrEmpty(options.Authority))
            {
                options.Authority = $"{options.Instance}{options.TenantId}";
            }
        }
    }
}
