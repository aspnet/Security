// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    public class CookieAuthenticationMiddleware : AuthenticationMiddleware<CookieAuthenticationOptions>
    {
        private readonly IAntiforgery _antiforgery;

        public CookieAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            IAntiforgery antiforgery,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            IOptions<CookieAuthenticationOptions> options)
            : base(next, options, loggerFactory, urlEncoder)
        {
            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (antiforgery == null)
            {
                throw new ArgumentNullException(nameof(antiforgery));
            }

            _antiforgery = antiforgery;

            if (Options.Events == null)
            {
                Options.Events = new CookieAuthenticationEvents();
            }
            if (String.IsNullOrEmpty(Options.CookieName))
            {
                Options.CookieName = CookieAuthenticationDefaults.CookiePrefix + Options.AuthenticationScheme;
            }
            if (Options.TicketDataFormat == null)
            {
                var provider = Options.DataProtectionProvider ?? dataProtectionProvider;
                var dataProtector = provider.CreateProtector(typeof(CookieAuthenticationMiddleware).FullName, Options.AuthenticationScheme, "v2");
                Options.TicketDataFormat = new TicketDataFormat(dataProtector);
            }
            if (Options.CookieManager == null)
            {
                Options.CookieManager = new ChunkingCookieManager(urlEncoder);
            }
            if (!Options.LoginPath.HasValue)
            {
                Options.LoginPath = CookieAuthenticationDefaults.LoginPath;
            }
            if (!Options.LogoutPath.HasValue)
            {
                Options.LogoutPath = CookieAuthenticationDefaults.LogoutPath;
            }
            if (!Options.AccessDeniedPath.HasValue)
            {
                Options.AccessDeniedPath = CookieAuthenticationDefaults.AccessDeniedPath;
            }
        }

        protected override AuthenticationHandler<CookieAuthenticationOptions> CreateHandler()
        {
            return new CookieAuthenticationHandler(_antiforgery);
        }
    }
}