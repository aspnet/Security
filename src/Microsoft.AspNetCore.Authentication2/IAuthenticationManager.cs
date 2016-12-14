// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication2
{
    public interface IAuthenticationManager2
    {
        Task<AuthenticationTicket2> AuthenticateAsync(string scheme);
        Task ChallengeAsync(string scheme, AuthenticationProperties2 properties, ChallengeBehavior behavior);
        Task ForbidAsync(string scheme, AuthenticationProperties2 properties);

        // Should SignIn/SignOut live in a separate service? If yes, we could have a parallel stack of
        // SignInScheme/Builder/SignInHandler
        Task SignInAsync(string scheme, ClaimsPrincipal principal, AuthenticationProperties2 properties);
        Task SignOutAsync(string scheme, AuthenticationProperties2 properties);
    }

    // Should probably revisit the (auto) challenge behavior as part of this
    public enum ChallengeBehavior
    {
        Automatic,
        Unauthorized,
        Forbidden
    }
}
