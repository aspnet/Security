// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="GoogleAuthenticationMiddleware"/>.
    /// </summary>
    public static class GoogleAppBuilderExtensions
    {
        /// <summary>
        /// Authenticate users using Google OAuth 2.0.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="configureOptions">configures the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseGoogleAuthentication([NotNull] this IApplicationBuilder app, [NotNull] Action<GoogleAuthenticationOptions> configureOptions)
        {
            var options = new GoogleAuthenticationOptions();
            configureOptions(options);
            return app.UseGoogleAuthentication(options);
        }

        /// <summary>
        /// Authenticate users using Google OAuth 2.0.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="options">the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseGoogleAuthentication([NotNull] this IApplicationBuilder app, [NotNull] GoogleAuthenticationOptions options)
        {
            return app.UseMiddleware<GoogleAuthenticationMiddleware>(options);
        }
    }
}