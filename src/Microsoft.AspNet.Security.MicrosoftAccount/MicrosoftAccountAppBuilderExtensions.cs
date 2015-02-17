// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.MicrosoftAccount;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="MicrosoftAccountAuthenticationMiddleware"/>
    /// </summary>
    public static class MicrosoftAccountAppBuilderExtensions
    {
        public static IApplicationBuilder UseMicrosoftAccountAuthentication([NotNull] this IApplicationBuilder app, Action<MicrosoftAccountAuthenticationOptions> configureOptions = null, string optionsName = "")
        {
            return app.UseMiddleware<MicrosoftAccountAuthenticationMiddleware>(
                 new ConfigureOptions<MicrosoftAccountAuthenticationOptions>(configureOptions ?? (o => { }))
                 {
                     Name = optionsName
                 });
        }
    }
}
