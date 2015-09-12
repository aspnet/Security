// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.Framework.Configuration;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods for using <see cref="MicrosoftAccountAuthenticationMiddleware"/>
    /// </summary>
    public static class MicrosoftAccountServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoftAccountAuthentication(this IServiceCollection services, Action<MicrosoftAccountAuthenticationOptions> configure)
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

        public static IServiceCollection AddMicrosoftAccountAuthentication(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return services.Configure<MicrosoftAccountAuthenticationOptions>(config);
        }
    }
}
