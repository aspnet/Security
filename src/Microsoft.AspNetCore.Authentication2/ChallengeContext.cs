// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication2
{
    public class ChallengeContext
    {
        public ChallengeContext(string authenticationScheme)
            : this(authenticationScheme, properties: null, behavior: ChallengeBehavior.Automatic)
        {
        }

        public ChallengeContext(string authenticationScheme, AuthenticationProperties2 properties, ChallengeBehavior behavior)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }

            AuthenticationScheme = authenticationScheme;
            Properties = properties ?? new AuthenticationProperties2();
            Behavior = behavior;
        }

        public string AuthenticationScheme { get; }

        public ChallengeBehavior Behavior { get; }

        public AuthenticationProperties2 Properties { get; }

        public bool Accepted { get; private set; }

        public void Accept()
        {
            Accepted = true;
        }
    }
}