// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public class VirtualAuthenticationHandler : SignInAuthenticationHandler<AuthenticationSchemeOptions>
    {
        public VirtualAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Context.AuthenticateAsync();

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            => Context.SignInAsync(user, properties);

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
            => Context.SignOutAsync(properties);

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
            => Context.ForbidAsync(properties);
    }
}