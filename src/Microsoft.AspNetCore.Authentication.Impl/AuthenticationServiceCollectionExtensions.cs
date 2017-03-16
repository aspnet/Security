// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
            services.TryAddScoped<IAuthenticationService, AuthenticationService>();
            services.TryAddSingleton<IClaimsTransformation, NoopClaimsTransformation>(); // Can be replaced with scoped ones that use DbContext
            services.TryAddScoped<IAuthenticationHandlerProvider, AuthenticationHandlerProvider>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
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

        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<AuthenticationSchemeBuilder> configureScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
        {
            services.AddAuthentication(o =>
            {
                o.AddScheme(authenticationScheme, scheme => {
                    scheme.HandlerType = typeof(THandler);
                    configureScheme?.Invoke(scheme);
                });
            });
            if (configureOptions != null)
            {
                services.Configure(authenticationScheme, configureOptions);
            }
            services.AddTransient<THandler>();
            services.Validate<TOptions>(authenticationScheme, o => o.Validate());
            return services;
        }

        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
            => services.AddScheme<TOptions, THandler>(authenticationScheme, configureScheme: null, configureOptions: configureOptions);

        //public static IServiceCollection AddRemoteScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions, Func<TOptions, IEnumerable<PathString>> getCallbackPaths)
        //     where TOptions : RemoteAuthenticationOptions, new()
        //     where THandler : AuthenticationHandler<TOptions>
        //{
        //    services.AddAuthentication(o =>
        //            o.AddScheme(authenticationScheme,
        //                schemeBuilder => {
        //                    schemeBuilder.HandlerType = typeof(THandler);
        //                    // TODO: MUST fix this to pickup option settings
        //                    schemeBuilder.CallbackPaths = getCallbackPaths?.Invoke(new TOptions());
        //                }));
        //    if (configureOptions != null)
        //    {
        //        services.Configure(authenticationScheme, configureOptions);
        //    }
        //    services.AddTransient<THandler>();
        //    services.Validate<TOptions>(authenticationScheme, o => o.Validate());
        //    return services;
        //}

        //public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, TOptions options)
        //    where TOptions : AuthenticationSchemeOptions, new()
        //    where THandler : AuthenticationHandler<TOptions>
        //{
        //    services.AddAuthentication(o =>
        //    {
        //        o.AddScheme(authenticationScheme, b =>
        //        {
        //            b.HandlerType = typeof(THandler);
        //            b.Settings["Options"] = options;
        //        });
        //    });
        //    services.AddTransient<THandler>();
        //    return services;
        //}

        //public static IServiceCollection ConfigureScheme<TOptions>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
        //    where TOptions : AuthenticationSchemeOptions, new()
        //{
        //    services.Configure<AuthenticationOptions>(o =>
        //    {
        //        if (o.SchemeMap.ContainsKey(authenticationScheme))
        //        {
        //            var options = o.SchemeMap[authenticationScheme].Settings["Options"] as TOptions;
        //            if (options == null)
        //            {
        //                throw new InvalidOperationException("Unable to find options in authenticationScheme settings for: " + authenticationScheme);
        //            }
        //            configureOptions?.Invoke(options);
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("No scheme registered for " + authenticationScheme);
        //        }

        //    });
        //    return services;
        //}
    }
}
