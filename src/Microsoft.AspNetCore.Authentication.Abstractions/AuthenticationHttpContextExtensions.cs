// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AuthenticationHttpContextExtensions
    {
        public static Task<AuthenticateResult> AuthenticateAsync(this HttpContext context, string scheme) =>
            context.RequestServices.GetRequiredService<IAuthenticationService>().AuthenticateAsync(context, scheme);

        public static Task ChallengeAsync(this HttpContext context, string scheme) =>
            context.ChallengeAsync(scheme, properties: null);

        public static Task ChallengeAsync(this HttpContext context, string scheme, AuthenticationProperties properties) =>
            context.ChallengeAsync(scheme, properties: properties, behavior: ChallengeBehavior.Automatic);

        public static Task ChallengeAsync(this HttpContext context, string scheme, AuthenticationProperties properties, ChallengeBehavior behavior) =>
            context.RequestServices.GetRequiredService<IAuthenticationService>().ChallengeAsync(context, scheme, properties, behavior);

        public static Task ForbidAsync(this HttpContext context, string scheme) =>
            context.ForbidAsync(scheme, properties: null);

        public static Task ForbidAsync(this HttpContext context, string scheme, AuthenticationProperties properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationService>().ChallengeAsync(context, scheme, properties, ChallengeBehavior.Forbidden);

        public static Task SignInAsync(this HttpContext context, string scheme, ClaimsPrincipal principal) =>
            context.SignInAsync(scheme, principal, properties: null);

        public static Task SignInAsync(this HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationService>().SignInAsync(context, scheme, principal, properties);

        public static Task SignOutAsync(this HttpContext context, string scheme) => context.SignOutAsync(scheme, properties: null);

        public static Task SignOutAsync(this HttpContext context, string scheme, AuthenticationProperties properties) =>
            context.RequestServices.GetRequiredService<IAuthenticationService>().SignOutAsync(context, scheme, properties);

        public static Task<string> GetTokenAsync(this HttpContext context, string signInScheme, string tokenName) =>
            context.RequestServices.GetRequiredService<IAuthenticationService>().GetTokenAsync(context, signInScheme, tokenName);
    }
}
