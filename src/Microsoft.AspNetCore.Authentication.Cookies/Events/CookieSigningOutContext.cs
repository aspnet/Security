// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Cookies
{
    /// <summary>
    /// Context object passed to the ICookieAuthenticationEvents method SigningOut    
    /// </summary>
    public class CookieSigningOutContext : HandlerContext<CookieAuthenticationHandler>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler">The cookie handler</param>
        /// <param name="context"></param>
        /// <param name="properties"></param>
        /// <param name="cookieOptions"></param>
        public CookieSigningOutContext(CookieAuthenticationHandler handler, HttpContext context, AuthenticationProperties properties, CookieOptions cookieOptions)
            : base(handler, context)
        {
            CookieOptions = cookieOptions;
            Properties = properties ?? new AuthenticationProperties();
        }

        public AuthenticationProperties Properties { get; set; }

        /// <summary>
        /// The options for creating the outgoing cookie.
        /// May be replace or altered during the SigningOut call.
        /// </summary>
        public CookieOptions CookieOptions { get; set; }

        /// <summary>
        /// Gets the options associated with the scheme.
        /// </summary>
        public CookieAuthenticationOptions Options => Handler.Options;

        /// <summary>
        /// The authentication scheme.
        /// </summary>
        public AuthenticationScheme Scheme => Handler.Scheme;
    }
}
