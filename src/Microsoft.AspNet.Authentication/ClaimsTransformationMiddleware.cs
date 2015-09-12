// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            RequestDelegate next,
            IOptions<ClaimsTransformationOptions> options,
            ConfigureOptions<ClaimsTransformationOptions> configureOptions)
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
            if (configureOptions != null)
            {
                configureOptions.Configure(Options);
            }
            _next = next;
        }

        public ClaimsTransformationOptions Options { get; set; }

        public async Task Invoke(HttpContext context)
        {
            var handler = new ClaimsTransformationAuthenticationHandler(Options.Transformer);
            handler.RegisterAuthenticationHandler(context.GetAuthentication());
            try
            {
                if (Options.Transformer != null)
                {
                    context.User = await Options.Transformer.TransformAsync(context.User);
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