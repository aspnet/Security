// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="TwitterAuthenticationMiddleware"/>
    /// </summary>
    public static class TwitterAppBuilderExtensions
    {
        public static IApplicationBuilder UseTwitterAuthentication(this IApplicationBuilder app, Action<TwitterAuthenticationOptions> configureOptions = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<TwitterAuthenticationMiddleware>(
                 new ConfigureOptions<TwitterAuthenticationOptions>(configureOptions ?? (o => { })));
        }
    }
}
