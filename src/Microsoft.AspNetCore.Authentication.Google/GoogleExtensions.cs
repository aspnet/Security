// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GoogleExtensions
    {
        public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services, Action<GoogleOptions> configureOptions) =>
            services.AddGoogleAuthentication(GoogleDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services, string authenticationScheme, Action<GoogleOptions> configureOptions) =>
            services.AddRemoteScheme<GoogleOptions, GoogleHandler>(authenticationScheme, configureOptions, o => new PathString[] { o.CallbackPath });
    }
}
