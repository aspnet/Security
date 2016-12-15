// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticateContext : BaseAuthenticationContext
    {
        public AuthenticateContext(HttpContext context, string authenticationScheme) : base(context, authenticationScheme, properties: null)
        {
        }

        public bool Accepted { get; private set; }

        public ClaimsPrincipal Principal { get; private set; }

        public IDictionary<string, object> Description { get; private set; }

        public Exception Error { get; private set; }

        public virtual void Authenticated(ClaimsPrincipal principal, AuthenticationProperties2 properties)
        {
            Accepted = true;

            Principal = principal;
            Properties = properties;

            // Set defaults for fields we don't use in case multiple handlers modified the context.
            Error = null;
        }

        public virtual void NotAuthenticated()
        {
            Accepted = true;

            // Set defaults for fields we don't use in case multiple handlers modified the context.
            Description = null;
            Error = null;
            Principal = null;
            Properties = null;
        }

        public virtual void Failed(Exception error)
        {
            Accepted = true;

            Error = error;

            // Set defaults for fields we don't use in case multiple handlers modified the context.
            Description = null;
            Principal = null;
            Properties = null;
        }
    }
}
