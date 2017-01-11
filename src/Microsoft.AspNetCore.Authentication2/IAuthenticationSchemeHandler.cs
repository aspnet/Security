// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        Task<AuthenticationRequestStatus> HandleRequestAsync();
    }

    // REVIEW: Name?  Or just return to a bool for Skip/Handled
    public class AuthenticationRequestStatus
    {
        /// <summary>
        /// If true the request is handled and middleware execution should stop.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// If true, skip this handler, and go to the next
        /// </summary>
        public bool Skipped { get; private set; }

        public static AuthenticationRequestStatus Skip = new AuthenticationRequestStatus { Skipped = true };
        public static AuthenticationRequestStatus Handle = new AuthenticationRequestStatus { Handled = true };
    }
}
