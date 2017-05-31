// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.AzureAd;
using Microsoft.Extensions.Configuration;
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

        private class AzureAdConfigureOptions : ConfigureDefaultOptions<AzureAdOptions>
        {
            public AzureAdConfigureOptions(IConfiguration config) :
                base(AzureAdDefaults.AuthenticationScheme,
                    options => config.GetSection("Microsoft:AspNetCore:Authentication:Schemes:" + AzureAdDefaults.AuthenticationScheme).Bind(options))
            { }
        }

        private class AzureAdInitializeOptions : IInitializeOptions<AzureAdOptions>
        {
            public AzureAdInitializeOptions() { }

            public void Initialize(string name, AzureAdOptions options)
            {
                if (string.IsNullOrEmpty(options.Authority))
                {
                    if (string.IsNullOrEmpty(options.Instance))
                    {
                        throw new InvalidOperationException("AzureAdB2COptions requires Instance to be set.");
                    }
                    if (string.IsNullOrEmpty(options.TenantId))
                    {
                        throw new InvalidOperationException("AzureAdB2COptions requires TenantId to be set.");
                    }
                    options.Authority = $"{options.Instance}{options.TenantId}";
                }
            }
        }

    }
}
