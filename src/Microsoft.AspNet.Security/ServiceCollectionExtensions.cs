// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Security
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureAuthorization([NotNull] this IServiceCollection services, [NotNull] Action<AuthorizationOptions> configure)
        {
            return services.Configure(configure);
        }
    }
}
