// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication2
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
            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Ticket?.Principal != null)
                {
                    context.User = result.Ticket.Principal;
                }
            }

            // TODO: revisit this, is registration order the best way?, should we force schemes to register unique
            // handled paths instead to have better routing?
            // Give each scheme a chance to handle the request
            var handlers = context.RequestServices.GetRequiredService<SchemeHandlerCache>();

            //authy.GetHandlerScheme()

            foreach (var scheme in await Schemes.GetPriorityOrderedSchemesAsync())
            {
                var handler = await handlers.GetHandlerAsync(context, scheme.Name);
                var result = await handler.HandleRequestAsync();
                if (result.Handled)
                {
                    return;
                }
            }

            await _next(context);
        }
    }
}