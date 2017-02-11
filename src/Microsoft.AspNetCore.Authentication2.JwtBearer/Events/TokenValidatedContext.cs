// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Authentication2.JwtBearer
{
    public class TokenValidatedContext : BaseJwtBearerContext
    {
        public TokenValidatedContext(HttpContext context, JwtBearerOptions options)
            : base(context, options)
        {
        }

        public SecurityToken SecurityToken { get; set; }
    }
}
