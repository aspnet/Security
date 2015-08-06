// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="OpenIdConnectAuthenticationMiddleware"/>
    /// </summary>
    public static class OpenIdConnectAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="configureOptions">configures the options for the middleware</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseOpenIdConnectAuthentication([NotNull] this IApplicationBuilder app, [NotNull] Action<OpenIdConnectAuthenticationOptions> configureOptions)
        {
            var options = new OpenIdConnectAuthenticationOptions();
            configureOptions(options);
            return app.UseOpenIdConnectAuthentication(options);
        }

        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <param name="options">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseOpenIdConnectAuthentication([NotNull] this IApplicationBuilder app, [NotNull] OpenIdConnectAuthenticationOptions options)
        {
            return app.UseMiddleware<OpenIdConnectAuthenticationMiddleware>(options);
        }
    }
}
