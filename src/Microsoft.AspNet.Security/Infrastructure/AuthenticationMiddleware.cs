// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;
using Microsoft.AspNet.RequestContainer;

namespace Microsoft.AspNet.Security.Infrastructure
{
    public abstract class AuthenticationMiddleware<TOptions> : AutoRequestServicesMiddleware where TOptions : AuthenticationOptions, new()
    {
        protected AuthenticationMiddleware([NotNull] RequestDelegate next, [NotNull] IServiceProvider services, [NotNull] IOptions<TOptions> options, ConfigureOptions<TOptions> configureOptions) : base(next, services)
        {
            if (configureOptions != null)
            {
                Options = options.GetNamedOptions(configureOptions.Name);
                configureOptions.Configure(Options, configureOptions.Name);
            }
            else
            {
                Options = options.Options;
            }
        }

        public string AuthenticationType { get; set; }

        public TOptions Options { get; set; }

        public override async Task InvokeCore(HttpContext context)
        {
            AuthenticationHandler<TOptions> handler = CreateHandler();
            await handler.Initialize(Options, context);
            if (!await handler.InvokeAsync())
            {
                await Next(context);
            }
            await handler.TeardownAsync();
        }

        protected abstract AuthenticationHandler<TOptions> CreateHandler();
    }
}
