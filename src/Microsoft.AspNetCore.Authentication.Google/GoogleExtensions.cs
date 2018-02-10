// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GoogleExtensions
    {
        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder)
            => builder.AddGoogle(GoogleDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, Action<GoogleOptions> configureOptions)
            => builder.AddGoogle(GoogleDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, string authenticationScheme, Action<GoogleOptions> configureOptions)
            => builder.AddGoogle(authenticationScheme, GoogleDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<GoogleOptions> configureOptions)
            => builder.AddOAuth<GoogleOptions, GoogleHandler>(authenticationScheme, displayName, configureOptions);

        /// <summary>
        /// Add google authentication with a default cookie to use as the default scheme.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder UseGoogleSignIn(this AuthenticationBuilder builder, Action<GoogleOptions> configureOptions)
        {
            builder.AddGoogle(GoogleDefaults.AuthenticationScheme, o =>
            {
                configureOptions?.Invoke(o);
                // Override instead of default since this method is opinionated on the cookie scheme name.
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            builder.TryAddCookie(CookieAuthenticationDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, o => o.ForwardChallenge = GoogleDefaults.AuthenticationScheme);
            builder.Services.Configure<AuthenticationOptions>(o => o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            return builder;
        }
    }
}
