// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticationOptions2
    {
        private readonly IList<AuthenticationScheme> _schemes = new List<AuthenticationScheme>();

        /// <summary>
        /// Returns the schemes in the order they were added (important for request handling priority)
        /// </summary>
        public IEnumerable<AuthenticationScheme> Schemes
        {
            get
            {
                return _schemes;
            }
        }

        public IDictionary<string, AuthenticationScheme> SchemeMap { get; } = new Dictionary<string, AuthenticationScheme>(); // case sensitive?

        public void AddScheme(string name, Action<AuthenticationSchemeBuilder> configureBuilder)
        {
            var builder = new AuthenticationSchemeBuilder(name);
            configureBuilder(builder);
            var scheme = builder.Build();
            _schemes.Add(scheme);
            SchemeMap[name] = scheme;
        }

        public string DefaultAuthenticationScheme { get; set; }

        //public string DefaultChallengeScheme { get; set; }

        /// <summary>
        /// Will be called after a successful authentication.
        /// </summary>
        public Func<ClaimsPrincipal, Task<ClaimsPrincipal>> ClaimsTransform { get; set; }
    }
}
