// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="HttpContext" />.
    /// </summary>
    public static class AuthenticationHttpContextExtensions
    {
        /// <summary>
        /// Adds authentication services to the specified <see cref="HttpContext" />. 
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static Task<AuthenticationTicket2> AuthenticateAsync(this HttpContext context, string scheme);
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        Task ChallengeAsync(string scheme, AuthenticationProperties2 properties, ChallengeBehavior behavior);
        Task ForbidAsync(string scheme, AuthenticationProperties2 properties);

        // Should SignIn/SignOut live in a separate service? If yes, we could have a parallel stack of
        // SignInScheme/Builder/SignInHandler
        Task SignInAsync(string scheme, ClaimsPrincipal principal);
        Task SignOutAsync(string scheme, AuthenticationProperties2 properties);

    }
}
