// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.Framework.Configuration;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods to add Jwt Bearer authentication capabilities to an HTTP application pipeline
    /// </summary>
    public static class JwtBearerServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureJwtBearerAuthentication(this IServiceCollection services, Action<JwtBearerAuthenticationOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return services.Configure(configure);
        }

        public static IServiceCollection ConfigureJwtBearerAuthentication(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return services.ConfigureJwtBearerAuthentication(config);
        }
    }
}
