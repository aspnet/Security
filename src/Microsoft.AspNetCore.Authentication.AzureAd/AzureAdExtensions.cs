// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.AzureAd;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureAdExtensions
    {
        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services) 
            => services.AddAzureAdAuthentication(_ => { });

        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services, Action<AzureAdOptions> configureOptions)
        {
            services.AddSingleton<ConfigureDefaultOptions<AzureAdOptions>, AzureAdConfigureOptions>();
            services.AddSingleton<IInitializeOptions<AzureAdOptions>, AzureAdInitializeOptions>();
            return services.AddOpenIdConnectAuthentication(AzureAdDefaults.AuthenticationScheme, configureOptions);
        }
    }
}
