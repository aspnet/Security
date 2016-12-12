// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2.Cookies;

namespace Microsoft.AspNetCore.Authentication2
{
    /// <summary>
    /// Extension methods for setting up cookie authentication services in an <see cref="AuthenticationOptions2" />.
    /// </summary>
    public static class CookieAuthenticationOptionsExtensions
    {
        public static AuthenticationOptions2 AddCookies(this AuthenticationOptions2 options, string scheme, Action<CookieAuthenticationOptions> configureOptions) {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var cookieOptions = new CookieAuthenticationOptions();
            configureOptions(cookieOptions);
            options.AddScheme(scheme, b =>
            {
                b.HandlerType = typeof(CookieAuthenticationHandler);
                b.Settings["Options"] = cookieOptions;
            });
            return options;
        }
    }
}
