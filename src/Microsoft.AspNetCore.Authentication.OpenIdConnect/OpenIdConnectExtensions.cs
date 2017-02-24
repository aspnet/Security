// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdConnectExtensions
    {
        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, Action<OpenIdConnectOptions> configureOptions) =>
            services.AddOpenIdConnectAuthentication(OpenIdConnectDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, string authenticationScheme, Action<OpenIdConnectOptions> configureOptions) =>
            services.AddScheme<OpenIdConnectOptions, OpenIdConnectHandler>(authenticationScheme, configureOptions, canHandleRequests: true);
    }
}
