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
    public static class AzureAdB2CExtensions
    {
        public static IServiceCollection AddAzureAdB2CAuthentication(this IServiceCollection services)
            => services.AddAzureAdB2CAuthentication(_ => { });

        public static IServiceCollection AddAzureAdB2CAuthentication(this IServiceCollection services, Action<AzureAdB2COptions> configureOptions)
        {
            services.AddSingleton<ConfigureDefaultOptions<AzureAdB2COptions>, AzureAdB2CConfigureOptions>();
            services.AddSingleton<IInitializeOptions<AzureAdB2COptions>, AzureAdB2CInitializeOptions>();
            services.AddOpenIdConnectAuthentication(AzureAdB2CDefaults.SignInSignUpAuthenticationScheme, configureOptions);
            services.AddOpenIdConnectAuthentication(AzureAdB2CDefaults.ResetPasswordAuthenticationScheme, configureOptions);
            services.AddOpenIdConnectAuthentication(AzureAdB2CDefaults.EditProfileAuthenticationScheme, configureOptions);
            return services;
        }

        public static IServiceCollection AddAzureAdB2CBearerAuthentication(this IServiceCollection services)
        {
            services.AddAzureAdB2CAuthentication();
            services.AddSingleton<IInitializeOptions<JwtBearerOptions>, BearerInitializeOptions>();
            services.AddJwtBearerAuthentication(AzureAdB2CDefaults.BearerAuthenticationScheme, _ => { });
            return services;
        }

        private class AzureAdB2CConfigureOptions : ConfigureDefaultOptions<AzureAdB2COptions>
        {
            private readonly IConfiguration _config;

            public AzureAdB2CConfigureOptions(IConfiguration config) => _config = config;

            public override void Configure(string name, AzureAdB2COptions options)
            {
                _config.GetSection("Microsoft:AspNetCore:Authentication:Schemes:" + AzureAdB2CDefaults.AuthenticationScheme).Bind(options);
            }
        }

        private class BearerInitializeOptions : IInitializeOptions<JwtBearerOptions>
        {
            private readonly AzureAdB2COptions _b2cOptions;
            public BearerInitializeOptions(IOptionsSnapshot<AzureAdB2COptions> options)
            {
                _b2cOptions = options.Get(AzureAdB2CDefaults.AuthenticationScheme);
            }

            // Binds Audience/Authority to the AzureB2C ClientId + Authority
            public void Initialize(string name, JwtBearerOptions options)
            {
                if (name == AzureAdB2CDefaults.BearerAuthenticationScheme)
                {
                    if (string.IsNullOrEmpty(options.Audience))
                    {
                        options.Audience = _b2cOptions.ClientId;
                    }
                    if (string.IsNullOrEmpty(options.Authority))
                    {
                        options.Authority = _b2cOptions.Authority;
                    }
                }
            }
        }

        private class AzureAdB2CInitializeOptions : IInitializeOptions<AzureAdB2COptions>
        {
            public AzureAdB2CInitializeOptions() { }

            public void Initialize(string name, AzureAdB2COptions options)
            {
                if (string.IsNullOrEmpty(options.Authority))
                {
                    if (string.IsNullOrEmpty(options.Instance))
                    {
                        throw new InvalidOperationException("AzureAdB2COptions requires Instance to be set.");
                    }
                    if (string.IsNullOrEmpty(options.Domain))
                    {
                        throw new InvalidOperationException("AzureAdB2COptions requires Domain to be set.");
                    }

                    // Bind only to any of the AzureAdB2C policy schemes
                    if (name == AzureAdB2CDefaults.EditProfileAuthenticationScheme)
                    {
                        if (string.IsNullOrEmpty(options.EditProfilePolicyId))
                        {
                            throw new InvalidOperationException("AzureAdB2COptions requires EditProfilePolicyId to be set.");
                        }
                        options.Authority = $"{options.Instance}/{options.Domain}/{options.EditProfilePolicyId}/v2.0";
                    }
                    else if (name == AzureAdB2CDefaults.ResetPasswordAuthenticationScheme)
                    {
                        if (string.IsNullOrEmpty(options.ResetPasswordPolicyId))
                        {
                            throw new InvalidOperationException("AzureAdB2COptions requires ResetPasswordPolicyId to be set.");
                        }
                        options.Authority = $"{options.Instance}/{options.Domain}/{options.ResetPasswordPolicyId}/v2.0";
                    }
                    else if (name == AzureAdB2CDefaults.SignInSignUpAuthenticationScheme)
                    {
                        if (string.IsNullOrEmpty(options.SignInSignUpPolicyId))
                        {
                            throw new InvalidOperationException("AzureAdB2COptions requires SignInSignUpPolicyId to be set.");
                        }
                        options.Authority = $"{options.Instance}/{options.Domain}/{options.SignInSignUpPolicyId}/v2.0";
                    }
                }
            }
        }

    }
}
