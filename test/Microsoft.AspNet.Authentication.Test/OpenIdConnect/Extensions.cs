// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// Extension specifies <see cref="CustomOpenIdConnectAuthenticationMiddleware"/> as the middleware.
    /// </summary>
    public static class OpenIdConnectAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="customConfigureOption">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <param name="loggerFactory">custom loggerFactory</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseCustomOpenIdConnectAuthentication(this IApplicationBuilder app, CustomConfigureOptions customConfigureOption, IUrlEncoder encoder, ILoggerFactory loggerFactory, OpenIdConnectAuthenticationHandler handler = null)
        {
            return app.UseMiddleware<CustomOpenIdConnectAuthenticationMiddleware>(customConfigureOption, encoder, loggerFactory, handler);
        }

        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="options">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <param name="loggerFactory">custom loggerFactory</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseCustomOpenIdConnectAuthentication(this IApplicationBuilder app, IOptions<OpenIdConnectAuthenticationOptions> options, IUrlEncoder encoder, ILoggerFactory loggerFactory, OpenIdConnectAuthenticationHandler handler = null)
        {
            return app.UseMiddleware<CustomOpenIdConnectAuthenticationMiddleware>(options, encoder, loggerFactory, handler);
        }
    }
}
