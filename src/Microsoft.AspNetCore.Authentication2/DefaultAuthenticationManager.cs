// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class DefaultAuthenticationManager : IAuthenticationManager2
    {
        // TODO: figure out some other way to get the context??
        public DefaultAuthenticationManager(IAuthenticationSchemeProvider schemes, IHttpContextAccessor context)
        {
            Schemes = schemes;
            _accessor = context;
        }

        public IAuthenticationSchemeProvider Schemes { get; }

        // handler instance cache, need to initialize once per request, manager lifetime should be per request
        private Dictionary<string, IAuthenticationSchemeHandler> _handlerMap = new Dictionary<string, IAuthenticationSchemeHandler>();

        private readonly IHttpContextAccessor _accessor;

        private HttpContext Context => _accessor.HttpContext;

        private async Task<IAuthenticationSchemeHandler> ResolveHandler(string authenticationScheme)
        {
            if (_handlerMap.ContainsKey(authenticationScheme))
            {
                return _handlerMap[authenticationScheme];
            }

            var scheme = await Schemes.GetSchemeAsync(authenticationScheme);
            var handler = scheme?.ResolveHandler(Context);
            if (handler != null)
            {
                await handler.InitializeAsync(scheme, Context);
                _handlerMap[authenticationScheme] = handler;
            }
            return handler;
        }

        public virtual async Task<AuthenticationTicket2> AuthenticateAsync(string authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            var context = new AuthenticateContext(authenticationScheme);
            var handler = await ResolveHandler(authenticationScheme);
            if (handler != null)
            {
                await handler.AuthenticateAsync(context);
            }

            if (!context.Accepted)
            {
                throw new InvalidOperationException($"No authentication handler is configured to authenticate for the scheme: {context.AuthenticationScheme}");
            }

            return new AuthenticationTicket2(context.Principal,
                new AuthenticationProperties2(context.Properties),
                authenticationScheme);
        }

        public virtual async Task ChallengeAsync(string authenticationScheme, AuthenticationProperties2 properties, ChallengeBehavior behavior)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            var challengeContext = new ChallengeContext(authenticationScheme, properties, behavior);
            var handler = await ResolveHandler(authenticationScheme);
            if (handler != null)
            {
                await handler.ChallengeAsync(challengeContext);
            }

            if (!challengeContext.Accepted)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }
        }

        public virtual async Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties2 properties)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var signInContext = new SignInContext(authenticationScheme, principal, properties);
            var handler = await ResolveHandler(authenticationScheme);
            if (handler != null)
            {
                await handler.SignInAsync(signInContext);
            }

            if (!signInContext.Accepted)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }
        }

        public virtual async Task SignOutAsync(string authenticationScheme, AuthenticationProperties2 properties)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            var signOutContext = new SignOutContext(authenticationScheme, properties);
            var handler = await ResolveHandler(authenticationScheme);
            if (handler != null)
            {
                await handler.SignOutAsync(signOutContext);
            }

            if (!signOutContext.Accepted)
            {
                throw new InvalidOperationException($"No authentication handler is configured to handle the scheme: {authenticationScheme}");
            }
        }

        public virtual Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return SignInAsync(authenticationScheme, principal, properties: null);
        }

        public virtual Task ForbidAsync(string authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return ForbidAsync(authenticationScheme, properties: null);
        }

        // Deny access (typically a 403)
        public virtual Task ForbidAsync(string authenticationScheme, AuthenticationProperties2 properties)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return ChallengeAsync(authenticationScheme, properties, ChallengeBehavior.Forbidden);
        }

    }
}
