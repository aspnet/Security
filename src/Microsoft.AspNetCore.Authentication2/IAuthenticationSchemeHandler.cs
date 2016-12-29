// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    // Created on a per request basis to handle one particular scheme.
    public interface IAuthenticationSchemeHandler
    {
        // Gives the handler access to the configuration data
        Task InitializeAsync(AuthenticationScheme scheme, HttpContext context);

        Task<AuthenticateResult> AuthenticateAsync(AuthenticateContext context);
        Task ChallengeAsync(ChallengeContext context);
        Task SignInAsync(SignInContext context);
        Task SignOutAsync(SignOutContext context);

        Task<AuthenticationRequestResult> HandleRequestAsync();
    }

    public class AuthenticationRequestResult
    {
        /// <summary>
        /// If true the request is handled and middleware execution should stop.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// If true, skip this handler, and go to the next
        /// </summary>
        public bool Skipped { get; private set; }

        /// <summary>
        /// If true, continue with the rest of the middleware pipeline, but bypass the rest of the handlers.
        /// </summary>
        public bool Bypassed { get; private set; }

        public static AuthenticationRequestResult Skip = new AuthenticationRequestResult { Skipped = true };
        public static AuthenticationRequestResult Handle = new AuthenticationRequestResult { Handled = true };
        public static AuthenticationRequestResult Bypass = new AuthenticationRequestResult { Bypassed = true };
    }
}
