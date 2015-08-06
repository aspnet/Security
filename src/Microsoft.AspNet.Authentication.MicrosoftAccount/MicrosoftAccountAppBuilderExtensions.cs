// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="MicrosoftAccountAuthenticationMiddleware"/>
    /// </summary>
    public static class MicrosoftAccountAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using MicrosoftAccount.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="configureOptions">configures the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseMicrosoftAccountAuthentication([NotNull] this IApplicationBuilder app, [NotNull] Action<MicrosoftAccountAuthenticationOptions> configureOptions)
        {
            var options = new MicrosoftAccountAuthenticationOptions();
            configureOptions(options);
            return app.UseMicrosoftAccountAuthentication(options);
        }

        /// <summary>
        /// Authenticate users using MicrosoftAccount.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="options">the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseMicrosoftAccountAuthentication([NotNull] this IApplicationBuilder app, [NotNull] MicrosoftAccountAuthenticationOptions options)
        {
            return app.UseMiddleware<MicrosoftAccountAuthenticationMiddleware>(options);
        }
    }
}
