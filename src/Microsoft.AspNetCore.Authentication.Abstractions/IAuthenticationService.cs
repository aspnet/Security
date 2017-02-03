// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme);
        Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties, ChallengeBehavior behavior);
        Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties);

        // Should SignIn/SignOut live in a separate service? If yes, we could have a parallel stack of
        // SignInScheme/Builder/SignInHandler
        Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties);
        Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties);
    }
}
