// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2.JwtBearer
{
    public class AuthenticationFailedContext : BaseJwtBearerContext
    {
        public AuthenticationFailedContext(HttpContext context, JwtBearerOptions options)
            : base(context, options)
        {
        }

        public Exception Exception { get; set; }
    }
}