// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CookieExtensions
    {
        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder)
            => builder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddCookie(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, Action<CookieAuthenticationOptions> configureOptions) 
            => builder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, string authenticationScheme, Action<CookieAuthenticationOptions> configureOptions)
            => builder.AddCookie(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<CookieAuthenticationOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());
            return builder.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }

        /// <summary>
        /// Try to add a cookie with the specified scheme only if that scheme has not been registered. 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="displayName"></param>
        public static AuthenticationBuilder TryAddCookie(this AuthenticationBuilder builder, string authenticationScheme, string displayName)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());
            builder.Services.TryAddTransient<CookieAuthenticationHandler>();
            builder.Services.Configure<AuthenticationOptions>(o =>
            {
                if (!o.SchemeMap.ContainsKey(authenticationScheme))
                {
                    o.AddScheme(authenticationScheme, scheme => {
                        scheme.HandlerType = typeof(CookieAuthenticationHandler);
                        scheme.DisplayName = displayName;
                    });
                }
            });
            return builder;
        }
    }
}
