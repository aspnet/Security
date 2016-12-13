// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication2
{
    public static class AuthenticationHttpContextExtensions
    {
        public static Task<AuthenticationTicket2> AuthenticateAsync(this HttpContext context, string scheme) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().AuthenticateAsync(scheme);

        public static Task ChallengeAsync(this HttpContext context, string scheme) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().ChallengeAsync(scheme, properties: null, behavior: ChallengeBehavior.Automatic);

        public static Task ChallengeAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties, ChallengeBehavior behavior) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().ChallengeAsync(scheme, properties, behavior);

        public static Task ForbidAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().ChallengeAsync(scheme, properties, ChallengeBehavior.Forbidden);

        public static Task SignInAsync(this HttpContext context, string scheme, ClaimsPrincipal principal) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().SignInAsync(scheme, principal);

        public static Task SignOutAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().SignOutAsync(scheme, properties);
    }
}
