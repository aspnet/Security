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
            services.TryAddSingleton<IClaimsTransformation, DefaultClaimsTransformation>(); // Can be replaced with scoped ones that use DbContext
            services.TryAddScoped<SchemeHandlerCache>(); // Add interface for the shared instance cache?
            services.TryAddSingleton<IAuthenticationSchemeProvider, DefaultAuthenticationSchemeProvider>();
            services.Configure(configureOptions);
            return services;
        }

        // REVIEW: rename to just AddScheme?
        public static IServiceCollection AddSchemeHandler<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationSchemeHandler<TOptions>
        {
            services.AddAuthentication(o =>
            {
                o.AddScheme(authenticationScheme, b =>
                {
                    b.HandlerType = typeof(THandler);
                    var options = new TOptions();
                    configureOptions?.Invoke(options);
                    b.Settings["Options"] = options;
                });
            });
            services.AddTransient<THandler>();
            return services;
        }

        // REVIEW: rename to just ConfigureScheme?
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureSchemeHandler<TOptions>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
        {
            services.Configure<AuthenticationOptions2>(o =>
            {
                if (o.SchemeMap.ContainsKey(authenticationScheme))
                {
                    var options = o.SchemeMap[authenticationScheme].Settings["Options"] as TOptions;
                    if (options == null)
                    {
                        throw new InvalidOperationException("Unable to find options in authenticationScheme settings for: " + authenticationScheme);
                    }
                    configureOptions?.Invoke(options);
                }
                else
                {
                    throw new InvalidOperationException("No scheme registered for " + authenticationScheme);
                }

            });
            return services;
        }
    }
}
