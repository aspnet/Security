// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="OpenIdConnectMiddleware"/>
    /// </summary>
    public static class OpenIdConnectExtensions
    {
        /// <summary>
        /// Adds the <see cref="OpenIdConnectMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="options">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseOpenIdConnectAuthentication([NotNull] this IApplicationBuilder app, Action<OpenIdConnectOptions> configureOptions)
        {

            var options = new OpenIdConnectOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            return app.UseOpenIdConnectAuthentication(options);
        }

        /// <summary>
        /// Adds the <see cref="OpenIdConnectMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="options">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseOpenIdConnectAuthentication([NotNull] this IApplicationBuilder app, [NotNull] OpenIdConnectOptions options)
        {
            return app.UseMiddleware<OpenIdConnectMiddleware>(options);
        }
    }
}
