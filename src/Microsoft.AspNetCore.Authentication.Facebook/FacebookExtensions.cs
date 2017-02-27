// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FacebookAuthenticationOptionsExtensions
    {
        public static IServiceCollection AddFacebookAuthentication(this IServiceCollection services, Action<FacebookOptions> configureOptions) =>
            services.AddFacebookAuthentication(FacebookDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddFacebookAuthentication(this IServiceCollection services, string authenticationScheme, Action<FacebookOptions> configureOptions)
        {
            return services.AddRemoteScheme<FacebookOptions, FacebookHandler>(authenticationScheme, configureOptions, o => new PathString[] { o.CallbackPath });
        }
    }
}
