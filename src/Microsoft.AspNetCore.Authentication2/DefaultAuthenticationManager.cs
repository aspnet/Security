// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class DefaultAuthenticationManager : IAuthenticationManager2
    {
        // TODO: figure out some other way to get the context??
        public DefaultAuthenticationManager(SchemeHandlerCache cache)
        {
            Handlers = cache;
        }

        public SchemeHandlerCache Handlers { get; }

        public virtual async Task<AuthenticateResult> AuthenticateAsync(HttpContext httpContext, string authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to authenticate for the scheme: {authenticationScheme}");
            }

            var context = new AuthenticateContext(httpContext, authenticationScheme);
            return await handler.AuthenticateAsync(context);
        }

        public virtual async Task ChallengeAsync(HttpContext httpContext, string authenticationScheme, AuthenticationProperties2 properties, ChallengeBehavior behavior)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }

            var challengeContext = new ChallengeContext(httpContext, authenticationScheme, properties, behavior);
            await handler.ChallengeAsync(challengeContext);
        }

        public virtual async Task SignInAsync(HttpContext httpContext, string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties2 properties)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }

            var signInContext = new SignInContext(httpContext, authenticationScheme, principal, properties);
            await handler.SignInAsync(signInContext);
        }

        public virtual async Task SignOutAsync(HttpContext httpContext, string authenticationScheme, AuthenticationProperties2 properties)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }

            var signOutContext = new SignOutContext(httpContext, authenticationScheme, properties);
            await handler.SignOutAsync(signOutContext);
        }

        public virtual Task SignInAsync(HttpContext httpContext, string authenticationScheme, ClaimsPrincipal principal)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return SignInAsync(httpContext, authenticationScheme, principal, properties: null);
        }

        public virtual Task ForbidAsync(HttpContext httpContext, string authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return ForbidAsync(httpContext, authenticationScheme, properties: null);
        }

        // Deny access (typically a 403)
        public virtual Task ForbidAsync(HttpContext httpContext, string authenticationScheme, AuthenticationProperties2 properties)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return ChallengeAsync(httpContext, authenticationScheme, properties, ChallengeBehavior.Forbidden);
        }

    }
}
