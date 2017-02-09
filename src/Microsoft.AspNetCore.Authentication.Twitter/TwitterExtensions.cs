// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TwitterExtensions
    {
        public static IServiceCollection AddTwitterAuthentication(this IServiceCollection services, Action<TwitterOptions> configureOptions) =>
            services.AddTwitterAuthentication(TwitterDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddTwitterAuthentication(this IServiceCollection services, string authenticationScheme, Action<TwitterOptions> configureOptions) =>
            services.AddSchemeHandler<TwitterOptions, TwitterHandler>(authenticationScheme, configureOptions);
    }
}
