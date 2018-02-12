// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MicrosoftAccountExtensions
    {
        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder)
            => builder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, Action<MicrosoftAccountOptions> configureOptions)
            => builder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, string authenticationScheme, Action<MicrosoftAccountOptions> configureOptions)
            => builder.AddMicrosoftAccount(authenticationScheme, MicrosoftAccountDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<MicrosoftAccountOptions> configureOptions)
            => builder.AddOAuth<MicrosoftAccountOptions, MicrosoftAccountHandler>(authenticationScheme, displayName, configureOptions);

        /// <summary>
        /// Add MicrosoftAccount authentication with a default cookie to use as the default scheme.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder UseMicrosoftAccountSignIn(this AuthenticationBuilder builder, Action<MicrosoftAccountOptions> configureOptions)
        {
            builder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, o =>
            {
                configureOptions?.Invoke(o);
                // Override instead of default since this method is opinionated on the cookie scheme name.
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            return builder.UseRemoteSignInCookie(CookieAuthenticationDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme, MicrosoftAccountDefaults.AuthenticationScheme);
        }
    }
}