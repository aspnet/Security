// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.AzureAd;
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
    }
}
