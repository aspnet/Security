// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Authentication.Cookies
{
    /// <summary>
    /// Default values related to cookie-based authentication middleware
    /// </summary>
    public static class CookieAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for CookieAuthenticationOptions.AuthenticationScheme
        /// </summary>
        public const string AuthenticationScheme = "Cookies";

        /// <summary>
        /// The prefix used to provide a default CookieAuthenticationOptions.CookieName
        /// </summary>
        public static readonly string CookiePrefix = ".AspNet.";

        /// <summary>
        /// The default value used by CookieAuthenticationMiddleware for the
        /// CookieAuthenticationOptions.LoginPath
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "By design")]
        public static readonly PathString LoginPath = new PathString("/Account/Login");

        /// <summary>
        /// The default value used by CookieAuthenticationMiddleware for the
        /// CookieAuthenticationOptions.LogoutPath
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Justification = "By design")]
        public static readonly PathString LogoutPath = new PathString("/Account/Logout");

        /// <summary>
        /// The default value used by CookieAuthenticationMiddleware for the
        /// CookieAuthenticationOptions.AccessDeniedPath
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "By design")]
        public static readonly PathString AccessDeniedPath = new PathString("/Account/AccessDenied");

        /// <summary>
        /// The default value of the CookieAuthenticationOptions.ReturnUrlParameter
        /// </summary>
        public static readonly string ReturnUrlParameter = "ReturnUrl";
    }
}
