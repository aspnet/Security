// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Authentication
{
    public class ClaimsTransformationMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsTransformationMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<ClaimsTransformationOptions> options)
        {
            // REVIEW: do we need to take ConfigureOptions<ClaimsTransformationOptions>??
            Options = options.Options;
            _next = next;
        }

        public ClaimsTransformationOptions Options { get; set; }

        public async Task Invoke(HttpContext context)
        {
            var handler = new ClaimsTransformationAuthenticationHandler(Options.Transformation);
            handler.RegisterAuthenticationHandler(context.GetAuthentication());
            try {
                if (Options.Transformation != null)
                {
                    context.User = Options.Transformation.Invoke(context.User);
                }
                await _next(context);
            }
            finally
            {
                handler.UnregisterAuthenticationHandler(context.GetAuthentication());
            }
        }
    }
}