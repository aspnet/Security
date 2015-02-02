// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security
{
    public class ClaimsTransformationMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsTransformationMiddleware([NotNull] RequestDelegate next, [NotNull] IOptions<ClaimsTransformationOptions> options, Action<ClaimsTransformationOptions> configureOptions = null)
        {
            Options = options.Options;
            _next = next;
            if (configureOptions != null)
            {
                configureOptions(Options);
            }
        }

        public ClaimsTransformationOptions Options { get; set; }

        // REVIEW: should this use auto request services like the other auth middlewares?
        public async Task Invoke(HttpContext context)
        {

            if (Options.TransformAsync != null)
            {
                context.User = await Options.TransformAsync(context.User);
            }
            await _next(context);
        }
    }
}
