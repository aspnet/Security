// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure OpenIdConnect authentication options
    /// </summary>
    public static class OpenIdConnectServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureOpenIdConnectAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<OpenIdConnectAuthenticationOptions> configure)
        {
            return services.Configure(configure);
        }

        public static IServiceCollection ConfigureOpenIdConnectAuthentication([NotNull] this IServiceCollection services, [NotNull] IConfiguration config)
        {
            return services.Configure<OpenIdConnectAuthenticationOptions>(config);
        }
    }
}
