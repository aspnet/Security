// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication.Google;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add Google authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class GoogleAppBuilderExtensions
    {
        public static IApplicationBuilder UseGoogleAuthentication(this IApplicationBuilder app)
        {
            throw new NotSupportedException("This method is no longer supported, see TODO:fwlink");
        }

        public static IApplicationBuilder UseGoogleAuthentication(this IApplicationBuilder app, GoogleOptions options)
        {
            throw new NotSupportedException("This method is no longer supported, see TODO:fwlink");
        }
    }
}
