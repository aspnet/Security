// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up authentication services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.AddDataProtection();
            services.AddWebEncoders();
            services.TryAddScoped<IAuthenticationService, DefaultAuthenticationService>();
            services.TryAddSingleton<IClaimsTransformation, DefaultClaimsTransformation>(); // Can be replaced with scoped ones that use DbContext
            services.TryAddScoped<IAuthenticationHandlerProvider, DefaultAuthenticationHandlerProvider>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, DefaultAuthenticationSchemeProvider>();
            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<AuthenticationOptions> configureOptions) {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddAuthentication();
            services.Configure(configureOptions);
            return services;
        }

        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
        {
            services.AddAuthentication(o =>
            {
                o.AddScheme(authenticationScheme, 
                    schemeBuilder => BuildScheme<TOptions, THandler>(authenticationScheme, schemeBuilder, configureOptions));
            });
            services.AddTransient<THandler>();
            return services;
        }

        private static TOptions BuildScheme<TOptions, THandler>(string authenticationScheme, AuthenticationSchemeBuilder builder, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
        {
            builder.HandlerType = typeof(THandler);
            var options = new TOptions();

            // REVIEW: is there a better place for this default?
            options.DisplayName = authenticationScheme;
            options.ClaimsIssuer = authenticationScheme;

            configureOptions?.Invoke(options);
            options.Validate();

            // revisit the settings typing
            builder.Settings["Options"] = options;

            return options;
        }

        public static IServiceCollection AddRemoteScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions, Func<TOptions, IEnumerable<PathString>> getCallbackPaths)
             where TOptions : RemoteAuthenticationOptions, new()
             where THandler : AuthenticationHandler<TOptions>
        {
            services.AddAuthentication(o =>
                o.AddScheme(authenticationScheme,
                    schemeBuilder => {
                        var options = BuildScheme<TOptions, THandler>(authenticationScheme, schemeBuilder, configureOptions);
                        schemeBuilder.CallbackPaths = getCallbackPaths?.Invoke(options);
                    }));
            services.AddTransient<THandler>();
            return services;
        }

        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, TOptions options)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
        {
            services.AddAuthentication(o =>
            {
                o.AddScheme(authenticationScheme, b =>
                {
                    b.HandlerType = typeof(THandler);
                    b.Settings["Options"] = options;
                });
            });
            services.AddTransient<THandler>();
            return services;
        }

        public static IServiceCollection ConfigureScheme<TOptions>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
        {
            services.Configure<AuthenticationOptions>(o =>
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
