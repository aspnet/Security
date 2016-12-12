// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        protected AuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes, IAuthenticationManager2 auth)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (schemes == null)
            {
                throw new ArgumentNullException(nameof(schemes));
            }

            if (auth == null)
            {
                throw new ArgumentNullException(nameof(auth));
            }

            _next = next;
            Schemes = schemes;
            Authentication = auth;
        }

        public IAuthenticationSchemeProvider Schemes { get; set; }

        public IAuthenticationManager2 Authentication { get; set; }

        public async Task Invoke(HttpContext context)
        {
            // Only is responsible for automatic authentication now
            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var ticket = await Authentication.AuthenticateAsync(defaultAuthenticate.Name);
                context.User = ticket.Principal;
            }
            await _next(context);
        }
    }
}