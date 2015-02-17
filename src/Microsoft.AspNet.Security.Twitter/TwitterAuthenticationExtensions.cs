﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.Twitter;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="TwitterAuthenticationMiddleware"/>
    /// </summary>
    public static class TwitterAuthenticationExtensions
    {
        public static IApplicationBuilder UseTwitterAuthentication([NotNull] this IApplicationBuilder app, Action<TwitterAuthenticationOptions> configureOptions = null, string optionsName = "")
        {
            return app.UseMiddleware<TwitterAuthenticationMiddleware>(
                 new ConfigureOptions<TwitterAuthenticationOptions>(configureOptions ?? (o => { }))
                 {
                     Name = optionsName
                 });
        }
    }
}
