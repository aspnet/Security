// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Context passed for redirect events.
    /// </summary>
    public class RedirectContext<TOptions> : PropertiesContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context">The HTTP request context</param>
        /// <param name="scheme">The scheme data</param>
        /// <param name="options">The handler options</param>
        /// <param name="redirectUri">The initial redirect URI</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        public RedirectContext(
            HttpContext context,
            AuthenticationScheme scheme,
            TOptions options,
            AuthenticationProperties properties,
            string redirectUri)
            : base(context, scheme, options, properties)
        {
            Properties = properties;
            RedirectUri = redirectUri;
        }

        /// <summary>
        /// Gets or Sets the URI used for the redirect operation.
        /// </summary>
        /// <remarks>If you cannot let ASP.NET rewrite the <see cref="HttpRequest.PathBase"/>
        /// for you, such as through <c>ForwardedHeadersOptions</c></remarks> you can use this.
        public string RedirectUri { get; set; }
    }
}
