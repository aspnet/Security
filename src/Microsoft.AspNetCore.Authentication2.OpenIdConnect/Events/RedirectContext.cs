// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2.OpenIdConnect
{
    /// <summary>
    /// When a user configures the <see cref="OpenIdConnectHandler"/> to be notified prior to redirecting to an IdentityProvider
    /// an instance of <see cref="RedirectContext"/> is passed to the 'RedirectToAuthenticationEndpoint' or 'RedirectToEndSessionEndpoint' events.
    /// </summary>
    public class RedirectContext : BaseOpenIdConnectContext
    {
        public RedirectContext(HttpContext context, OpenIdConnectOptions options, AuthenticationProperties2 properties)
            : base(context, options)
        {
            Properties = properties;
        }

        public AuthenticationProperties2 Properties { get; }
    }
}