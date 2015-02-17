﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.Twitter;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods for using <see cref="TwitterAuthenticationMiddleware"/>
    /// </summary>
    public static class TwitterServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureTwitterAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<TwitterAuthenticationOptions> configure)
        {
            return services.Configure(configure);
        }
    }
}
