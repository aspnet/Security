﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.OpenIdConnect;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods for using <see cref="OpenIdConnectAuthenticationMiddleware"/>
    /// </summary>
    public static class OpenIdConnectAuthenticationExtensions
    {
        public static IServiceCollection ConfigureOpenIdConnectAuthentication(this IServiceCollection services, Action<OpenIdConnectAuthenticationOptions> configure)
        {
            return services.ConfigureOptions(configure);
        }
    }
}