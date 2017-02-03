// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Authentication
{
    public class ChallengeContext : BaseAuthenticationContext
    {
        public ChallengeContext(HttpContext httpContext, string authenticationScheme)
            : this(httpContext, authenticationScheme, properties: null, behavior: ChallengeBehavior.Automatic)
        {
        }

        public ChallengeContext(HttpContext httpContext, string authenticationScheme, AuthenticationProperties properties, ChallengeBehavior behavior)
            : base(httpContext, authenticationScheme, properties)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            Behavior = behavior;
        }

        public ChallengeBehavior Behavior { get; }
    }
}