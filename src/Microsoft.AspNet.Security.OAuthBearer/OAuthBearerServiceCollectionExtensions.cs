// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.OAuthBearer;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods to add OAuth Bearer authentication capabilities to an IServiceCollection
    /// </summary>
    public static class OAuthBearerServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureOAuthBearerAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<OAuthBearerAuthenticationOptions> configure)
        {
            return services.ConfigureOptions(configure);
        }
    }
}
