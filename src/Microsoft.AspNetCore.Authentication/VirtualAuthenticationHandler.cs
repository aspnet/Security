// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Used to redirect authentication methods to another scheme
    /// </summary>
    public class VirtualSchemeOptions : AuthenticationSchemeOptions
    {
        public string DefaultTarget { get; set; }

        public string AuthenticateTarget { get; set; }
        public string ChallengeTarget { get; set; }
        public string ForbidTarget { get; set; }
        public string SignInTarget { get; set; }
        public string SignOutTarget { get; set; }

        /// <summary>
        /// Used to 
        /// </summary>
        public Func<HttpContext, string> DefaultTargetSelector { get; set; }

    }

    // REVIEW: Should we make this generic so people can derive easier with TOptions?

    /// <summary>
    /// Forwards calls to another authentication scheme based.
    /// </summary>
    public class VirtualAuthenticationHandler : AuthenticationHandler<VirtualSchemeOptions>, IAuthenticationSignInHandler
    {
        public VirtualAuthenticationHandler(IOptionsMonitor<VirtualSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        private string ResolveDefaultScheme(string scheme)
            => scheme ?? Options.DefaultTargetSelector?.Invoke(Context) ?? Options.DefaultTarget;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Context.AuthenticateAsync(ResolveDefaultScheme(Options.AuthenticateTarget));

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
            => Context.ChallengeAsync(ResolveDefaultScheme(Options.ChallengeTarget));

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
            => Context.ForbidAsync(ResolveDefaultScheme(Options.ForbidTarget));

        public virtual Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            => Context.SignInAsync(ResolveDefaultScheme(Options.SignInTarget), user, properties);

        public virtual Task SignOutAsync(AuthenticationProperties properties)
            => Context.SignOutAsync(ResolveDefaultScheme(Options.SignOutTarget), properties);
    }
}