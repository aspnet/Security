// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods for using <see cref="MicrosoftAccountMiddleware"/>
    /// </summary>
    public static class MicrosoftAccountServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoftAccountAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<MicrosoftAccountOptions> configure)
        {
            return services.Configure(configure);
        }

        public static IServiceCollection AddMicrosoftAccountAuthentication([NotNull] this IServiceCollection services, [NotNull] IConfiguration config)
        {
            return services.Configure<MicrosoftAccountOptions>(config);
        }
    }
}
