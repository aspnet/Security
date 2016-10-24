// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public class ClaimsTransformationMiddleware<TClaimsTransformer> where TClaimsTransformer : IClaimsTransformer
    {
        private readonly RequestDelegate _next;
        private readonly IClaimsTransformer _transform;

        public ClaimsTransformationMiddleware(
            RequestDelegate next,
            TClaimsTransformer transformer)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            _transform = transformer;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var handler = new ClaimsTransformationHandler(_transform, context);
            handler.RegisterAuthenticationHandler(context.GetAuthentication());
            try
            {
                if (_transform != null)
                {
                    var transformationContext = new ClaimsTransformationContext(context)
                    {
                        Principal = context.User
                    };
                    context.User = await _transform.TransformAsync(transformationContext);
                }
                await _next(context);
            }
            finally
            {
                handler.UnregisterAuthenticationHandler(context.GetAuthentication());
            }
        }
    }

    public class ClaimsTransformationMiddleware : ClaimsTransformationMiddleware<IClaimsTransformer>
    {
        public ClaimsTransformationMiddleware(
            RequestDelegate next,
            IOptions<ClaimsTransformationOptions> options) : base(next, options.Value.Transformer)
        { }

        public ClaimsTransformationOptions Options { get; set; }
    }
}