// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticateContext : BaseAuthenticationContext
    {
        public AuthenticateContext(HttpContext context, string authenticationScheme) : base(context, authenticationScheme, properties: null)
        { }
    }
}
