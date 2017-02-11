// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2.MicrosoftAccount;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MicrosoftAccountExtensions
    {
        public static IServiceCollection AddMicrosoftAccountAuthentication(this IServiceCollection services, Action<MicrosoftAccountOptions> configureOptions) =>
            services.AddMicrosoftAccountAuthentication(MicrosoftAccountDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddMicrosoftAccountAuthentication(this IServiceCollection services, string authenticationScheme, Action<MicrosoftAccountOptions> configureOptions) =>
            services.AddSchemeHandler<MicrosoftAccountOptions, MicrosoftAccountHandler>(authenticationScheme, configureOptions);
    }
}
