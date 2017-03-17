// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationService(IAuthenticationSchemeProvider schemes, IAuthenticationHandlerProvider handlers, IClaimsTransformation transform)
        {
            Schemes = schemes;
            Handlers = handlers;
            Transform = transform;
        }

        public IAuthenticationSchemeProvider Schemes { get; }


        public IAuthenticationHandlerProvider Handlers { get; }

        public IClaimsTransformation Transform { get; }

        public virtual async Task<AuthenticateResult> AuthenticateAsync(HttpContext httpContext, string authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                var defaultScheme = await Schemes.GetDefaultAuthenticateSchemeAsync();
                authenticationScheme = defaultScheme?.Name;
                if (authenticationScheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultAuthenticateScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to authenticate for the scheme: {authenticationScheme}");
            }

            var context = new AuthenticateContext(httpContext, authenticationScheme);
            var result = await handler.AuthenticateAsync(context);
            if (result.Succeeded)
            {
                var transformed = await Transform.TransformAsync(result.Principal);
                return AuthenticateResult.Success(new AuthenticationTicket(transformed, result.Properties, result.Ticket.AuthenticationScheme));
            }
            return result;
        }

        public virtual async Task ChallengeAsync(HttpContext httpContext, string authenticationScheme, AuthenticationProperties properties, ChallengeBehavior behavior)
        {
            if (authenticationScheme == null)
            {
                var defaultChallengeScheme = await Schemes.GetDefaultChallengeSchemeAsync();
                authenticationScheme = defaultChallengeScheme?.Name;
                if (authenticationScheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultChallengeScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }

            var challengeContext = new ChallengeContext(httpContext, authenticationScheme, properties, behavior);
            await handler.ChallengeAsync(challengeContext);
        }

        public virtual async Task SignInAsync(HttpContext httpContext, string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (authenticationScheme == null)
            {
                var defaultScheme = await Schemes.GetDefaultSignInSchemeAsync();
                authenticationScheme = defaultScheme?.Name;
                if (authenticationScheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultAuthenticateScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(httpContext, authenticationScheme);
            if (handler == null)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }

            var signInContext = new SignInContext(httpContext, authenticationScheme, principal, properties);
            await handler.SignInAsync(signInContext);
        }

        public virtual async Task SignOutAsync(HttpContext httpContext, string authenticationScheme, AuthenticationProperties properties)
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

        // Deny access (typically a 403)
        public virtual Task ForbidAsync(HttpContext httpContext, string authenticationScheme, AuthenticationProperties properties)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return ChallengeAsync(httpContext, authenticationScheme, properties, ChallengeBehavior.Forbidden);
        }

    }
}
