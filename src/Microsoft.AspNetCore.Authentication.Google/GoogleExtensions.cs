// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GoogleExtensions
    {
        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder)
            => builder.AddGoogle(GoogleDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, Action<GoogleOptions> configureOptions)
            => builder.AddGoogle(GoogleDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, string authenticationScheme, Action<GoogleOptions> configureOptions)
            => builder.AddOAuth<GoogleOptions, GoogleHandler>(authenticationScheme, configureOptions);


        // REMOVE below once callers have been updated

        public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services) 
            => services.AddGoogleAuthentication(GoogleDefaults.AuthenticationScheme, _ => { });

        public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services, Action<GoogleOptions> configureOptions) 
            => services.AddGoogleAuthentication(GoogleDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services, string authenticationScheme, Action<GoogleOptions> configureOptions)
            => services.AddOAuthAuthentication<GoogleOptions, GoogleHandler>(authenticationScheme, configureOptions);
    }
}
