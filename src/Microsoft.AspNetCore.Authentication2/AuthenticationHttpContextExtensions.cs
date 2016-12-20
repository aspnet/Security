// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication2
{
    public static class AuthenticationHttpContextExtensions
    {
        public static Task<AuthenticateResult> AuthenticateAsync(this HttpContext context, string scheme) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().AuthenticateAsync(context, scheme);

        public static Task ChallengeAsync(this HttpContext context, string scheme) =>
            context.ChallengeAsync(scheme, properties: null);

        public static Task ChallengeAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties) =>
            context.ChallengeAsync(scheme, properties: properties, behavior: ChallengeBehavior.Automatic);

        public static Task ChallengeAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties, ChallengeBehavior behavior) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().ChallengeAsync(context, scheme, properties, behavior);

        public static Task ForbidAsync(this HttpContext context, string scheme) =>
            context.ForbidAsync(scheme, properties: null);

        public static Task ForbidAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().ChallengeAsync(context, scheme, properties, ChallengeBehavior.Forbidden);

        public static Task SignInAsync(this HttpContext context, string scheme, ClaimsPrincipal principal) =>
            context.SignInAsync(scheme, principal, properties: null);

        public static Task SignInAsync(this HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties2 properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().SignInAsync(context, scheme, principal, properties);

        public static Task SignOutAsync(this HttpContext context, string scheme) => context.SignOutAsync(scheme, properties: null);

        public static Task SignOutAsync(this HttpContext context, string scheme, AuthenticationProperties2 properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationManager2>().SignOutAsync(context, scheme, properties);
    }
}
