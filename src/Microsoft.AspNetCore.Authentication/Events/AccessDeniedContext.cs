// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Provides access denied failure context information to handler providers.
    /// </summary>
    public class AccessDeniedContext : HandleRequestContext<RemoteAuthenticationOptions>
    {
        public AccessDeniedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            RemoteAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        /// <summary>
        /// Additional state values for the authentication session.
        /// </summary>
        public AuthenticationProperties Properties { get; set; }
    }
}
