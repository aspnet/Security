// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2.Cookies;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CookieAuthenticationOptionsExtensions
    {
        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services) => services.AddCookieAuthentication(configureOptions: null);

        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services, Action<CookieAuthenticationOptions> configureOptions) =>
            services.AddSchemeHandler<CookieAuthenticationOptions, CookieAuthenticationHandler>(configureOptions);
    }
}
