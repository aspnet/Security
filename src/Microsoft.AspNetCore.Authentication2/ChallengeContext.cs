// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class ChallengeContext : BaseAuthenticationContext
    {
        public ChallengeContext(HttpContext httpContext, string authenticationScheme)
            : this(httpContext, authenticationScheme, properties: null, behavior: ChallengeBehavior.Automatic)
        {
        }

        public ChallengeContext(HttpContext httpContext, string authenticationScheme, AuthenticationProperties2 properties, ChallengeBehavior behavior)
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