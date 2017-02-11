// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2.JwtBearer
{
    public class MessageReceivedContext : BaseJwtBearerContext
    {
        public MessageReceivedContext(HttpContext context, JwtBearerOptions options)
            : base(context, options)
        {
        }

        /// <summary>
        /// Bearer Token. This will give application an opportunity to retrieve token from an alternation location.
        /// </summary>
        public string Token { get; set; }
    }
}