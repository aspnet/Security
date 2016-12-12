// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticationOptions2
    {
        public IDictionary<string, AuthenticationScheme> SchemeMap { get; } = new Dictionary<string, AuthenticationScheme>(); // case sensitive?

        public void AddScheme(string name, Action<AuthenticationSchemeBuilder> configureBuilder)
        {
            var builder = new AuthenticationSchemeBuilder(name);
            configureBuilder(builder);
            SchemeMap[name] = builder.Build();
        }

        public string DefaultAuthenticationScheme { get; set; }

        public string DefaultChallengeScheme { get; set; }
    }
}
