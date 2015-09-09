// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="FacebookMiddleware"/>.
    /// </summary>
    public static class FacebookAppBuilderExtensions
    {
        /// <summary>
        /// Authenticate users using Facebook.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseFacebookAuthentication([NotNull] this IApplicationBuilder app, Action<FacebookOptions> configureOptions = null)
        {
            return app.UseMiddleware<FacebookMiddleware>(
                 new ConfigureOptions<FacebookOptions>(configureOptions ?? (o => { })));
        }
    }
}
