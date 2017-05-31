// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.AzureAd;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureExtensions
    {
        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services) 
            => services.AddAzureAdAuthentication(AzureDefaults.AzureAdAuthenticationScheme, _ => { });

        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services, Action<AzureAdOptions> configureOptions) 
            => services.AddAzureAdAuthentication(AzureDefaults.AzureAdAuthenticationScheme, configureOptions);

        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services, string authenticationScheme, Action<AzureAdOptions> configureOptions)
        {
            services.AddSingleton<ConfigureDefaultOptions<AzureAdOptions>, AzureAdConfigureOptions>();
            services.AddSingleton<IInitializeOptions<AzureAdOptions>, AzureAdInitializeOptions>();
            return services.AddOpenIdConnectAuthentication(authenticationScheme, configureOptions);
        }

        public static IServiceCollection AddAzureAdB2CAuthentication(this IServiceCollection services)
            => services.AddAzureAdB2CAuthentication(_ => { });

        public static IServiceCollection AddAzureAdB2CAuthentication(this IServiceCollection services, Action<AzureAdB2COptions> configureOptions)
        {
            services.AddSingleton<ConfigureDefaultOptions<AzureAdB2COptions>, AzureAdB2CConfigureOptions>();
            services.AddSingleton<IInitializeOptions<AzureAdB2COptions>, AzureAdB2CInitializeOptions>();
            services.AddOpenIdConnectAuthentication(AzureDefaults.AzureAdB2CSignInSignUpAuthenticationScheme, configureOptions);
            services.AddOpenIdConnectAuthentication(AzureDefaults.AzureAdB2CResetPasswordAuthenticationScheme, configureOptions);
            services.AddOpenIdConnectAuthentication(AzureDefaults.AzureAdB2CEditProfileAuthenticationScheme, configureOptions);
            return services;
        }
    }
}
