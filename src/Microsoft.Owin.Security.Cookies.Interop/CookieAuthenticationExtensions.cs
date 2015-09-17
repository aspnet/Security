// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Cookies.Interop;

namespace Owin
{
    public static class CookieAuthenticationExtensions
    {
        public static IAppBuilder UseCookieAuthentication(
            this IAppBuilder app,
            CookieAuthenticationOptions options,
            DataProtectionProvider dataProtectionProvider,
            PipelineStage stage = PipelineStage.Authenticate,
            string authenticationScheme = null)
        {
            // In ASP.NET 5 cookie middleware and identity, there's a distinction between auth scheme
            // (of which there's one per cookie / ticket) and auth type (of which there's one per identity).
            // We'll try to perform auto-fixup here by using the defaults.
            if (authenticationScheme == null)
            {
                if ((options.AuthenticationType?.EndsWith(".AuthType", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                {
                    authenticationScheme = options.AuthenticationType.Substring(0, options.AuthenticationType.Length - ".AuthType".Length);
                }
                else
                {
                    authenticationScheme = options.AuthenticationType;
                }
            }

            var dataProtector = dataProtectionProvider.CreateProtector(
                "Microsoft.AspNet.Authentication.Cookies.CookieAuthenticationMiddleware", // full name of the ASP.NET 5 type
                authenticationScheme, "v2");
            options.TicketDataFormat = new AspNet5TicketDataFormat(new DataProtectorShim(dataProtector), authenticationScheme);

            return app.UseCookieAuthentication(options, stage);
        }
    }
}