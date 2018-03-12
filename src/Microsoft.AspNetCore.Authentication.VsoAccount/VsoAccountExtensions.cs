// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.VsoAccount;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class VsoAccountExtensions
    {
        public static AuthenticationBuilder AddVsoAccount(this AuthenticationBuilder builder)
            => builder.AddVsoAccount(VsoAccountDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddVsoAccount(this AuthenticationBuilder builder, Action<VsoAccountOptions> configureOptions)
            => builder.AddVsoAccount(VsoAccountDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddVsoAccount(this AuthenticationBuilder builder, string authenticationScheme, Action<VsoAccountOptions> configureOptions)
            => builder.AddVsoAccount(authenticationScheme, VsoAccountDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddVsoAccount(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<VsoAccountOptions> configureOptions)
            => builder.AddOAuth<VsoAccountOptions, VsoAccountHandler>(authenticationScheme, displayName, configureOptions);
    }
}