// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;

namespace Microsoft.Framework.DependencyInjection
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddWebEncoders();
            services.AddDataProtection();
            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<SharedAuthenticationOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            return services.AddAuthentication();
        }

        public static IServiceCollection AddClaimsTransformation(this IServiceCollection services, Action<ClaimsTransformationOptions> configure)
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

        public static IServiceCollection AddClaimsTransformation(this IServiceCollection services, Func<ClaimsPrincipal, ClaimsPrincipal> transform)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            return services.Configure<ClaimsTransformationOptions>(o => o.Transformer = new ClaimsTransformer { TransformSyncDelegate = transform });
        }

        public static IServiceCollection AddClaimsTransformation(this IServiceCollection services, Func<ClaimsPrincipal, Task<ClaimsPrincipal>> asyncTransform)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (asyncTransform == null)
            {
                throw new ArgumentNullException(nameof(asyncTransform));
            }

            return services.Configure<ClaimsTransformationOptions>(o => o.Transformer = new ClaimsTransformer { TransformAsyncDelegate = asyncTransform });
        }
    }
}
