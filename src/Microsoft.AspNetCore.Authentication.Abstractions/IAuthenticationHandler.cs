// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    // Created on a per request basis to handle one particular scheme.
    public interface IAuthenticationHandler
    {
        // Gives the handler access to the configuration data
        Task InitializeAsync(AuthenticationScheme scheme, HttpContext context);

        Task<AuthenticateResult> AuthenticateAsync(AuthenticateContext context);
        Task ChallengeAsync(ChallengeContext context);
        Task SignInAsync(SignInContext context);
        Task SignOutAsync(SignOutContext context);
    }
}
