// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class SignOutContext : BaseAuthenticationContext
    {
        public SignOutContext(HttpContext context, string authenticationScheme, AuthenticationProperties2 properties)
            : base(context, authenticationScheme, properties)
        {
        }
    }
}