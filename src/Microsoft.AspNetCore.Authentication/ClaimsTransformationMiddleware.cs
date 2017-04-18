// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public class ClaimsTransformationMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsTransformationMiddleware(
            RequestDelegate next,
            IOptions<ClaimsTransformationOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Options = options.Value;
            _next = next;
        }

        public ClaimsTransformationOptions Options { get; set; }

        public async Task Invoke(HttpContext context)
        {
            var transform = Options.Transformer;
            if (transform == null && Options.TransformerType != null)
            {
                transform = context.RequestServices.GetRequiredService(Options.TransformerType) as IClaimsTransformer;
            }
            var handler = new ClaimsTransformationHandler(transform, context);
            handler.RegisterAuthenticationHandler(context.GetAuthentication());
            try
            {
                if (transform != null)
                {
                    var transformationContext = new ClaimsTransformationContext(context)
                    {
                        Principal = context.User
                    };
                    context.User = await transform.TransformAsync(transformationContext);
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