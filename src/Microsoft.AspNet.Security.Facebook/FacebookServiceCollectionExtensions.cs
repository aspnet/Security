// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.Facebook;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods for using <see cref="FacebookAuthenticationMiddleware"/>.
    /// </summary>
    public static class FacebookServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureFacebookAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<FacebookAuthenticationOptions> configure)
        {
            return services.Configure(configure);
        }
    }
}
