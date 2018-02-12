// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FacebookAuthenticationOptionsExtensions
    {
        public static AuthenticationBuilder AddFacebook(this AuthenticationBuilder builder)
            => builder.AddFacebook(FacebookDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddFacebook(this AuthenticationBuilder builder, Action<FacebookOptions> configureOptions)
            => builder.AddFacebook(FacebookDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddFacebook(this AuthenticationBuilder builder, string authenticationScheme, Action<FacebookOptions> configureOptions)
            => builder.AddFacebook(authenticationScheme, FacebookDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddFacebook(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<FacebookOptions> configureOptions)
            => builder.AddOAuth<FacebookOptions, FacebookHandler>(authenticationScheme, displayName, configureOptions);

        /// <summary>
        /// Add facebook authentication with a default cookie to use as the default scheme.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder UseFacebookSignIn(this AuthenticationBuilder builder, Action<FacebookOptions> configureOptions)
        {
            builder.AddFacebook(FacebookDefaults.AuthenticationScheme, o =>
            {
                configureOptions?.Invoke(o);
                // Override instead of default since this method is opinionated on the cookie scheme name.
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            return builder.UseRemoteSignInCookie(CookieAuthenticationDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme, FacebookDefaults.AuthenticationScheme);
        }
    }
}
