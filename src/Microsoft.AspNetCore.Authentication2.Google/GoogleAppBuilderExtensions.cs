// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.AspNetCore.Authentication2.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add Google authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class GoogleAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="GoogleHandler"/> to the specified <see cref="IApplicationBuilder"/>,
        /// which enables Google authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseGoogleAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = app.ApplicationServices.GetRequiredService<IOptions<GoogleOptions>>();
            return app.UseGoogleAuthentication(options.Value);
        }

        /// <summary>
        /// Adds the <see cref="GoogleHandler"/> to the specified <see cref="IApplicationBuilder"/>,
        /// which enables Google authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="GoogleOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseGoogleAuthentication(this IApplicationBuilder app, GoogleOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Switch to ActivatorUtilities.CreateInstance if we follow this route
            return app.UseLegacyAuthentication(options, context => new GoogleHandler(
                context.RequestServices.GetRequiredService<ILoggerFactory>(),
                context.RequestServices.GetRequiredService<UrlEncoder>(),
                context.RequestServices.GetRequiredService<IDataProtectionProvider>(),
                context.RequestServices.GetRequiredService<ISystemClock>()));
        }
    }
}
