// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up authentication services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class AuthenticationServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddAuthentication(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddAuthenticationCore();
            services.AddDataProtection();
            services.AddWebEncoders();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            return new AuthenticationBuilder(services);
        }

        public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, Action<AuthenticationOptions> configureOptions) {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var builder = services.AddAuthentication();
            services.Configure(configureOptions);
            return builder;
        }

        // REMOVE below once callers have been updated
        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, string displayName, Action<AuthenticationSchemeBuilder> configureScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
        {
            services.AddAuthentication(o =>
            {
                o.AddScheme(authenticationScheme, scheme => {
                    scheme.HandlerType = typeof(THandler);
                    scheme.DisplayName = displayName;
                    configureScheme?.Invoke(scheme);
                });
            });
            if (configureOptions != null)
            {
                services.Configure(authenticationScheme, configureOptions);
            }
            services.AddTransient<THandler>();
            return services;
        }

        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
            => services.AddScheme<TOptions, THandler>(authenticationScheme, displayName: null, configureScheme: null, configureOptions: configureOptions);

        public static IServiceCollection AddScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, string displayName, Action<TOptions> configureOptions)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
            => services.AddScheme<TOptions, THandler>(authenticationScheme, displayName, configureScheme: null, configureOptions: configureOptions);

        public static IServiceCollection AddRemoteScheme<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, string displayName, Action<TOptions> configureOptions)
            where TOptions : RemoteAuthenticationOptions, new()
            where THandler : RemoteAuthenticationHandler<TOptions>
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TOptions>, EnsureSignInScheme<TOptions>>());
            return services.AddScheme<TOptions, THandler>(authenticationScheme, displayName, configureScheme: null, configureOptions: configureOptions);
        }

        // Used to ensure that there's always a default data protection provider
        private class EnsureSignInScheme<TOptions> : IPostConfigureOptions<TOptions> where TOptions : RemoteAuthenticationOptions
        {
            private readonly AuthenticationOptions _authOptions;

            public EnsureSignInScheme(IOptions<AuthenticationOptions> authOptions)
            {
                _authOptions = authOptions.Value;
            }

            public void PostConfigure(string name, TOptions options)
            {
                options.SignInScheme = options.SignInScheme ?? _authOptions.DefaultSignInScheme;
            }
        }

    }
}
