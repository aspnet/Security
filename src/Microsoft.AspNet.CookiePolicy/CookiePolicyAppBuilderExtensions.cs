// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.CookiePolicy;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods provided by the cookie policy middleware
    /// </summary>
    public static class CookiePolicyAppBuilderExtensions
    {
        /// <summary>
        /// Adds a claims transformation middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder passed to your configuration method</param>
        /// <returns>The original app parameter</returns>
        public static IApplicationBuilder UseCookiePolicy(this IApplicationBuilder app)
        {
            return app.UseCookiePolicy(configureOptions: o => { });
        }

        /// <summary>
        /// Adds a claims transformation middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder passed to your configuration method</param>
        /// <param name="configureOptions">Used to configure the options for the middleware</param>
        /// <returns>The original app parameter</returns>
        public static IApplicationBuilder UseCookiePolicy(this IApplicationBuilder app, Action<CookiePolicyOptions> configureOptions)
        {
            return app.UseMiddleware<CookiePolicyMiddleware>(configureOptions);
        }
    }
}