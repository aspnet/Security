// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.AzureAd;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        public static IServiceCollection AddAzureAdBearerAuthentication(this IServiceCollection services)
        {
            services.AddAzureAdAuthentication();
            services.AddSingleton<IInitializeOptions<JwtBearerOptions>, BearerInitializeOptions>();
            services.AddSingleton<ConfigureDefaultOptions<JwtBearerOptions>, BearerConfigureOptions>();
            services.AddJwtBearerAuthentication(AzureAdDefaults.BearerAuthenticationScheme, _ => { });
            return services;
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
                        throw new InvalidOperationException("AzureAdOptions requires Instance to be set.");
                    }
                    if (string.IsNullOrEmpty(options.TenantId))
                    {
                        throw new InvalidOperationException("AzureAdOptions requires TenantId to be set.");
                    }
                    options.Authority = $"{options.Instance}{options.TenantId}";
                }
            }
        }

        private class BearerConfigureOptions : ConfigureDefaultOptions<JwtBearerOptions>
        {
            private readonly IConfiguration _config;

            public BearerConfigureOptions(IConfiguration config) => _config = config;

            public override void Configure(string name, JwtBearerOptions options)
            {
                _config.GetSection("Microsoft:AspNetCore:Authentication:Schemes:" + AzureAdDefaults.BearerAuthenticationScheme).Bind(options);
            }
        }

        private class BearerInitializeOptions : IInitializeOptions<JwtBearerOptions>
        {
            private readonly AzureAdOptions _adOptions;
            public BearerInitializeOptions(IOptionsSnapshot<AzureAdOptions> options)
            {
                _adOptions = options.Get(AzureAdDefaults.AuthenticationScheme);
            }

            // Binds Audience/Authority to the Azure ClientId + Authority
            public void Initialize(string name, JwtBearerOptions options)
            {
                if (name == AzureAdDefaults.BearerAuthenticationScheme)
                {
                    if (string.IsNullOrEmpty(options.Audience))
                    {
                        options.Audience = _adOptions.ClientId;
                    }
                    if (string.IsNullOrEmpty(options.Authority))
                    {
                        options.Authority = _adOptions.Authority;
                    }
                }
            }
        }


    }
}
