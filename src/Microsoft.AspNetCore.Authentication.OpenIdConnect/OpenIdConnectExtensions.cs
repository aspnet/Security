// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdConnectExtensions
    {
        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder)
            => builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder, Action<OpenIdConnectOptions> configureOptions)
            => builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder, string authenticationScheme, Action<OpenIdConnectOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>());
            return builder.AddRemoteScheme<OpenIdConnectOptions, OpenIdConnectHandler>(authenticationScheme, authenticationScheme, configureOptions);
        }

        // REMOVE once callers have been updated
        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services)
            => services.AddOpenIdConnectAuthentication(OpenIdConnectDefaults.AuthenticationScheme, _ => { });

        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, Action<OpenIdConnectOptions> configureOptions) 
            => services.AddOpenIdConnectAuthentication(OpenIdConnectDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, string authenticationScheme, Action<OpenIdConnectOptions> configureOptions)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>());
            return services.AddRemoteScheme<OpenIdConnectOptions, OpenIdConnectHandler>(authenticationScheme, authenticationScheme, configureOptions);
        }
    }
}
