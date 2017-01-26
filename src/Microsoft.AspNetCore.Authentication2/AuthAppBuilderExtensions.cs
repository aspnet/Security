// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class AuthAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="AuthenticationMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            // We only every want one instance of this middleware in the app
            if (!app.Properties.ContainsKey(".AspNetCore.AuthenticationMiddleware"))
            {
                app.Properties[".AspNetCore.AuthenticationMiddleware"] = true;
                return app.UseMiddleware<AuthenticationMiddleware>();
            }

            return app;
        }

        public static IApplicationBuilder UseLegacyAuthentication(this IApplicationBuilder app, AuthenticationSchemeOptions options, Func<HttpContext, IAuthenticationSchemeHandler> resolveHandler)
        {
            var schemeProvider = app.ApplicationServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var sharedOptions = app.ApplicationServices.GetRequiredService<IOptions<AuthenticationOptions2>>();
            var settings = new Dictionary<string, object>();
            settings["Options"] = options;
            var scheme = new AuthenticationScheme(options.AuthenticationScheme, typeof(IAuthenticationSchemeHandler), settings, sharedOptions.Value);
            scheme.ResolveHandlerFunc = resolveHandler;
            schemeProvider.AddScheme(scheme);
            return app.UseAuthentication();
        }
    }
}