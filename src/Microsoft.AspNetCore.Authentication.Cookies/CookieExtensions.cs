// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CookieExtensions
    {
        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services) => services.AddCookieAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services, string authenticationScheme) => services.AddCookieAuthentication(authenticationScheme, configureOptions: null);

        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services, Action<CookieAuthenticationOptions> configureOptions) =>
            services.AddCookieAuthentication(CookieAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services, string authenticationScheme, Action<CookieAuthenticationOptions> configureOptions)
        {
            // Makes sure that DataProtectionProvider is always set.
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeOptions<CookieAuthenticationOptions>, EnsureDataProtection>());
            services.Initialize<CookieAuthenticationOptions>(authenticationScheme, options =>
            {
                if (String.IsNullOrEmpty(options.CookieName))
                {
                    options.CookieName = CookieAuthenticationDefaults.CookiePrefix + authenticationScheme;
                }
                if (options.TicketDataFormat == null)
                {
                    if (options.DataProtectionProvider == null)
                    {
                        // This shouldn't happen normally due to the EnsureDataProtection initialize options.
                        throw new InvalidOperationException("DataProtectionProvider must be provided.");
                    }

                    // Note: the purpose for the data protector must remain fixed for interop to work.
                    var dataProtector = options.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", authenticationScheme, "v2");
                    options.TicketDataFormat = new TicketDataFormat(dataProtector);
                }
                if (options.CookieManager == null)
                {
                    options.CookieManager = new ChunkingCookieManager();
                }
                if (!options.LoginPath.HasValue)
                {
                    options.LoginPath = CookieAuthenticationDefaults.LoginPath;
                }
                if (!options.LogoutPath.HasValue)
                {
                    options.LogoutPath = CookieAuthenticationDefaults.LogoutPath;
                }
                if (!options.AccessDeniedPath.HasValue)
                {
                    options.AccessDeniedPath = CookieAuthenticationDefaults.AccessDeniedPath;
                }
            });
            return services.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(authenticationScheme, configureOptions);
        }

        // Used to ensure that there's always a default data protection provider
        private class EnsureDataProtection : IInitializeOptions<CookieAuthenticationOptions>
        {
            private readonly IDataProtectionProvider _dp;

            public EnsureDataProtection(IDataProtectionProvider dataProtection)
            {
                _dp = dataProtection;
            }

            public void Initialize(string name, CookieAuthenticationOptions options)
            {
                options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;
            }
        }

    }
}
