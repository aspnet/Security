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
    /// Forwards calls to another authentication scheme based.
    /// </summary>
    public class VirtualAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationSignInHandler
    {
        public VirtualAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task InitializeHandlerAsync()
        {
            if (!Options.SchemeForwarding.Enabled)
            {
                throw new InvalidOperationException("VirtualAuthenticationHandlers require options.SchemeForwarding.Enabled = true.");
            }
            return base.InitializeHandlerAsync();
        }

        /// <summary>
        /// This should never get called due to Options.Targets always forwarding.
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => throw new NotImplementedException();

        public virtual Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            => Context.SignInAsync(ResolveTarget(Options.SchemeForwarding.SignInTarget), user, properties);

        public virtual Task SignOutAsync(AuthenticationProperties properties)
            => Context.SignOutAsync(ResolveTarget(Options.SchemeForwarding.SignOutTarget), properties);
    }
}