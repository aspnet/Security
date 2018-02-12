// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TwitterExtensions
    {
        public static AuthenticationBuilder AddTwitter(this AuthenticationBuilder builder)
            => builder.AddTwitter(TwitterDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddTwitter(this AuthenticationBuilder builder, Action<TwitterOptions> configureOptions)
            => builder.AddTwitter(TwitterDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddTwitter(this AuthenticationBuilder builder, string authenticationScheme, Action<TwitterOptions> configureOptions)
            => builder.AddTwitter(authenticationScheme, TwitterDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddTwitter(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<TwitterOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TwitterOptions>, TwitterPostConfigureOptions>());
            return builder.AddRemoteScheme<TwitterOptions, TwitterHandler>(authenticationScheme, displayName, configureOptions);
        }

        /// <summary>
        /// Add Twitter authentication with a default cookie to use as the default scheme.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder UseTwitterSignIn(this AuthenticationBuilder builder, Action<TwitterOptions> configureOptions)
        {
            builder.AddTwitter(TwitterDefaults.AuthenticationScheme, o =>
            {
                configureOptions?.Invoke(o);
                // Override instead of default since this method is opinionated on the cookie scheme name.
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            return builder.UseRemoteSignInCookie(CookieAuthenticationDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme, TwitterDefaults.AuthenticationScheme);
        }
    }
}
