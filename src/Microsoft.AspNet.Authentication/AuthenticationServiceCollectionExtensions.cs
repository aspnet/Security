// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication;

namespace Microsoft.Framework.DependencyInjection
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureClaimsTransformation([NotNull] this IServiceCollection services, [NotNull] Action<ClaimsTransformationOptions> configure)
        {
            return services.Configure(configure);
        }
    }
}
