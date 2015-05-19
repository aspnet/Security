// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authorization;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureAuthorization([NotNull] this IServiceCollection services, [NotNull] Action<AuthorizationOptions> configure)
        {
            return services.Configure(configure);
        }

        public static IServiceCollection AddAuthorization([NotNull] this IServiceCollection services)
        {
            return services.AddAuthorization(configureOptions: null);
        }

        public static IServiceCollection AddAuthorization([NotNull] this IServiceCollection services, Action<AuthorizationOptions> configureOptions)
        {
            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Transient<IAuthorizationService, DefaultAuthorizationService>());
            services.AddTransient<IAuthorizationHandler, PassThroughAuthorizationHandler>();
            return services;
        }

        //policy.AddRequirement(new MagicRequirement(context, req => { context.Fail() }));

        // Add this come up name
        public class MagicRequirement : AuthorizationHandler<MagicRequirement>, IAuthorizationRequirement
        {
            public Action<AuthorizationContext, MagicRequirement> Handler { get;  }

            public MagicRequirement(Action<AuthorizationContext, MagicRequirement> handleMe)
            {
                Handler = handleMe;
            }

            protected override void Handle(AuthorizationContext context, MagicRequirement requirement)
            {
                Handler.Invoke(context, requirement);
            }
        }
    }
}