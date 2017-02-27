// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
        }

        public IAuthenticationSchemeProvider Schemes { get; set; }

        public async Task Invoke(HttpContext context)
        {
            var oldFeature = context.Features.Get<IAuthenticationFeature>();
            try
            {
                context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                {
                    OriginalPath = context.Request.Path,
                    OriginalPathBase = context.Request.PathBase
                });

                var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
                if (defaultAuthenticate != null)
                {
                    var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
                    if (result?.Ticket?.Principal != null)
                    {
                        context.User = result.Ticket.Principal;
                    }
                }

                // Give each scheme a chance to handle the request if it requested
                var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerResolver>();
                foreach (var scheme in await Schemes.GetRequestHandlerSchemes())
                {
                    var handler = await handlers.ResolveHandlerAsync(context, scheme.Name);
                    if (await handler.HandleRequestAsync())
                    {
                        return;
                    }
                }

                await _next(context);
            }
            finally
            {
                context.Features.Set(oldFeature);
            }
        }
    }
}