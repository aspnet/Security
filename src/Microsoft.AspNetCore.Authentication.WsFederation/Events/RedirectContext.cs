// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace Microsoft.AspNetCore.Authentication.WsFederation
{
    /// <summary>
    /// When a user configures the <see cref="WsFederationHandler"/> to be notified prior to redirecting to an IdentityProvider
    /// an instance of <see cref="RedirectContext"/> is passed to the 'RedirectToAuthenticationEndpoint' or 'RedirectToEndSessionEndpoint' events.
    /// </summary>
    public class RedirectContext : PropertiesContext<WsFederationOptions>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scheme"></param>
        /// <param name="options"></param>
        /// <param name="properties"></param>
        public RedirectContext(
            HttpContext context,
            AuthenticationScheme scheme,
            WsFederationOptions options,
            AuthenticationProperties properties)
            : base(context, scheme, options, properties) { }

        /// <summary>
        /// The <see cref="WsFederationMessage"/> used to compose the redirect.
        /// </summary>
        public WsFederationMessage ProtocolMessage { get; set; }

        /// <summary>
        /// If true, will skip any default logic for this redirect.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// Skips any default logic for this redirect.
        /// </summary>
        public void HandleResponse() => Handled = true;
    }
}