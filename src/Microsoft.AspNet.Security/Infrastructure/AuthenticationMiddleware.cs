// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security.Infrastructure
{
    public abstract class AuthenticationMiddleware<TOptions> where TOptions : AuthenticationOptions, new()
    {
        private readonly RequestDelegate _next;

        protected AuthenticationMiddleware([NotNull] RequestDelegate next, [NotNull] IOptionsAccessor<TOptions> options, OptionsAction<TOptions> optionsAction)
        {
            Options = options.GetNamedOptions(optionsAction.Name);
            optionsAction.Invoke(Options);
            _next = next;
        }

        public string AuthenticationType { get; set; }

        public TOptions Options { get; set; }

        public async Task Invoke(HttpContext context)
        {
            AuthenticationHandler<TOptions> handler = CreateHandler();
            await handler.Initialize(Options, context);
            if (!await handler.InvokeAsync())
            {
                await _next(context);
            }
            await handler.TeardownAsync();
        }

        protected abstract AuthenticationHandler<TOptions> CreateHandler();
    }
}
