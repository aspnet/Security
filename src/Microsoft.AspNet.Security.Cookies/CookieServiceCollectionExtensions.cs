// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.Cookies;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods provided by the cookies authentication middleware
    /// </summary>
    public static class CookieServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCookieAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<CookieAuthenticationOptions> configure)
        {
            return services.Configure(configure);
        }
    }
}