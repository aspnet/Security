// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up authentication services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<AuthenticationOptions2> configureOptions) {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddDataProtection();
            services.AddWebEncoders();
            services.TryAddScoped<IAuthenticationManager2, DefaultAuthenticationManager>();
            services.TryAddScoped<SchemeHandlerCache>(); // Add interface for the shared instance cache?
            services.TryAddSingleton<IAuthenticationSchemeProvider, DefaultAuthenticationSchemeProvider>();
            services.Configure(configureOptions);
            return services;
        }

        public static IServiceCollection AddSchemeHandler<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationSchemeHandler<TOptions>
        {
            services.AddAuthentication(o =>
            {
                o.AddScheme(authenticationScheme, b =>
                {
                    b.HandlerType = typeof(THandler);
                    b.Settings["ConfigureOptions"] = configureOptions;
                });
            });
            services.AddTransient<THandler>();
            return services;
        }
    }
}
