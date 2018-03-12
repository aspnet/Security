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
        /// Always configures this cookie as the default scheme and configures <see cref="AuthenticationSchemeOptions.ForwardChallenge"/>
        /// to point to <paramref name="challengeScheme"/>.
        /// </summary>
        public static AuthenticationBuilder UseRemoteSignInCookie(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string challengeScheme)
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
            builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, o => o.ForwardChallenge = challengeScheme);
            builder.Services.Configure<AuthenticationOptions>(o => o.DefaultScheme = authenticationScheme);
            return builder;
        }
    }
}
