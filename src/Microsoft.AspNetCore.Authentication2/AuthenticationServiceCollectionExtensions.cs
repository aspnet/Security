// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.AspNetCore.Http;
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IAuthenticationManager2, DefaultAuthenticationManager>();
            services.TryAddScoped<SchemeHandlerCache>(); // Add interface for the shared instance cache?
            services.TryAddSingleton<IAuthenticationSchemeProvider, DefaultAuthenticationSchemeProvider>();
            services.Configure(configureOptions);
            return services;
        }
    }
}
