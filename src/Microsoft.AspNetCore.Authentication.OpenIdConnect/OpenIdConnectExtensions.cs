// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdConnectExtensions
    {
        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder)
            => builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder, Action<OpenIdConnectOptions> configureOptions)
            => builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder, string authenticationScheme, Action<OpenIdConnectOptions> configureOptions)
            => builder.AddOpenIdConnect(authenticationScheme, OpenIdConnectDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<OpenIdConnectOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>());
            return builder.AddRemoteScheme<OpenIdConnectOptions, OpenIdConnectHandler>(authenticationScheme, displayName, configureOptions);
        }

        /// <summary>
        /// Add OpenIdConnect authentication with a default cookie to use as the default scheme.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder UseOpenIdConnectSignIn(this AuthenticationBuilder builder, Action<OpenIdConnectOptions> configureOptions)
        {
            builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, o =>
            {
                configureOptions?.Invoke(o);
                // Override instead of default since this method is opinionated on the cookie scheme name.
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            return builder.UseRemoteSignInCookie(CookieAuthenticationDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
