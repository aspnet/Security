// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
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
}
