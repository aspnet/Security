// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            
            return app.UseAuthentication();
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

            var schemeProvider = app.ApplicationServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var sharedOptions = app.ApplicationServices.GetRequiredService<IOptions<AuthenticationOptions2>>();
            var settings = new Dictionary<string, object>();
            settings["Options"] = options;
            var scheme = new AuthenticationScheme(options.AuthenticationScheme, typeof(CookieAuthenticationHandler), settings, sharedOptions.Value);
            scheme.ResolveHandlerFunc = context => new CookieAuthenticationHandler(
                context.RequestServices.GetRequiredService<ILoggerFactory>(),
                context.RequestServices.GetRequiredService<UrlEncoder>());
            schemeProvider.AddScheme(scheme);
            return app.UseAuthentication();
        }
    }
}