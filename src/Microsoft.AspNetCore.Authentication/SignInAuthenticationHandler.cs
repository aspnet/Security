// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Adds support for SignInAsync
    /// </summary>
    public abstract class SignInAuthenticationHandler<TOptions> : SignOutAuthenticationHandler<TOptions>, IAuthenticationSignInHandler
            where TOptions : AuthenticationSchemeOptions, new()
    {
        private bool _inSignIn;

        public SignInAuthenticationHandler(IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        { }

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            if (_inSignIn)
            {
                throw new InvalidOperationException("SignIn for scheme:[" + Scheme.Name + "] resulted in a recursive call back to itself.");
            }

            _inSignIn = true;
            try
            {
                var target = ResolveTarget(Options.ForwardSignIn);
                return (target != null)
                    ? Context.SignInAsync(target, user, properties)
                    : HandleSignInAsync(user, properties ?? new AuthenticationProperties());
            }
            finally
            {
                _inSignIn = false;
            }
        }

        /// <summary>
        /// Override this method to handle SignIn.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="properties"></param>
        /// <returns>A Task.</returns>
        protected abstract Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties);

    }
}