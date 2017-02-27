// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class OAuthExtensions
    {
        public static IServiceCollection AddOAuthAuthentication(this IServiceCollection services, string authenticationScheme, Action<OAuthOptions> configureOptions) =>
            services.AddRemoteScheme<OAuthOptions, OAuthHandler<OAuthOptions>>(authenticationScheme, configureOptions, o => new PathString[] { o.CallbackPath });
    }
}
