// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public abstract class BaseContext
    {
        protected BaseContext(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpContext = context;
        }

        public HttpContext HttpContext { get; }

        public HttpRequest Request
        {
            get { return HttpContext.Request; }
        }

        public HttpResponse Response
        {
            get { return HttpContext.Response; }
        }
    }

    public abstract class BaseAuthenticationContext : BaseContext
    {
        protected BaseAuthenticationContext(HttpContext context, string authenticationScheme, AuthenticationProperties2 properties) : base(context)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            AuthenticationScheme = authenticationScheme;
            Properties = properties ?? new AuthenticationProperties2();
        }

        public string AuthenticationScheme { get; }

        /// <summary>
        /// Contains the extra meta-data arriving with the authentication. May be altered.
        /// </summary>
        public AuthenticationProperties2 Properties { get; protected set; }
    }
}
