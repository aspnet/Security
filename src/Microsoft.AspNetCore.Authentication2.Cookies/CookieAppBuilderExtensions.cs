// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.AspNetCore.Authentication2.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add cookie authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class CookieAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="AuthenticationMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables cookie authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseCookieAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = app.ApplicationServices.GetRequiredService<IOptions<CookieAuthenticationOptions>>();
            return app.UseCookieAuthentication(options.Value);
        }

        /// <summary>
        /// Adds the <see cref="AuthenticationMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables cookie authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="CookieAuthenticationOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseCookieAuthentication(this IApplicationBuilder app, CookieAuthenticationOptions options)
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
            return app.UseLegacyAuthentication(options, context => new CookieAuthenticationHandler(
                context.RequestServices.GetRequiredService<ILoggerFactory>(),
                context.RequestServices.GetRequiredService<UrlEncoder>()));
        }
    }
}