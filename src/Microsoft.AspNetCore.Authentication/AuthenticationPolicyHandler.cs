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
    public class AuthenticationPolicyOptions : AuthenticationSchemeOptions {
        // Used as the fallback if any of the explicit schemes and the default scheme selected is null.
        public string DefaultScheme { get; set; }

        public string AuthenticateScheme { get; set; }
        public string ChallengeScheme { get; set; }
        public string ForbidScheme { get; set; }
        public string SignInScheme { get; set; }
        public string SignOutScheme { get; set; }

        /// <summary>
        /// Used to 
        /// </summary>
        public Func<HttpContext, string> DefaultSchemeSelector { get; set; }
    }

    /// <summary>
    /// Forwards calls to another authentication scheme based on <see cref="AuthenticationPolicyOptions"/>.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class AuthenticationPolicyHandler<TOptions> : AuthenticationHandler<TOptions>, IAuthenticationSignInHandler
        where TOptions : AuthenticationPolicyOptions, new()
    {
        public AuthenticationPolicyHandler(IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        private string ResolveDefaultScheme(string scheme)
            => scheme ?? Options.DefaultSchemeSelector?.Invoke(Context) ?? Options.DefaultScheme;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Context.AuthenticateAsync(ResolveDefaultScheme(Options.AuthenticateScheme));

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
            => Context.ChallengeAsync(ResolveDefaultScheme(Options.ChallengeScheme));

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
            => Context.ForbidAsync(ResolveDefaultScheme(Options.ForbidScheme));

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            => Context.SignInAsync(ResolveDefaultScheme(Options.SignInScheme), user, properties);

        public Task SignOutAsync(AuthenticationProperties properties)
            => Context.SignOutAsync(ResolveDefaultScheme(Options.SignOutScheme), properties);
    }
}