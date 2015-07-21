// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="TwitterAuthenticationMiddleware"/>
    /// </summary>
    public static class TwitterAppBuilderExtensions
    {
        /// <summary>
        /// Authenticate users using Twitter.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="configureOptions">configures the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseTwitterAuthentication([NotNull] this IApplicationBuilder app, [NotNull] Action<TwitterAuthenticationOptions> configureOptions)
        {
            var options = new TwitterAuthenticationOptions();
            configureOptions(options);
            return app.UseTwitterAuthentication(options);
        }

        /// <summary>
        /// Authenticate users using Twitter.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="options">the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseTwitterAuthentication([NotNull] this IApplicationBuilder app, [NotNull] TwitterAuthenticationOptions options)
        {
            return app.UseMiddleware<TwitterAuthenticationMiddleware>(options);
        }
    }
}
