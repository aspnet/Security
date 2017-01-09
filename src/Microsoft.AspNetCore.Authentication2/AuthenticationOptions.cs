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
        private readonly IList<AuthenticationSchemeBuilder> _schemes = new List<AuthenticationSchemeBuilder>();

        /// <summary>
        /// Returns the schemes in the order they were added (important for request handling priority)
        /// </summary>
        public IEnumerable<AuthenticationSchemeBuilder> Schemes
        {
            get
            {
                return _schemes;
            }
        }

        public IDictionary<string, AuthenticationSchemeBuilder> SchemeMap { get; } = new Dictionary<string, AuthenticationSchemeBuilder>(); // case sensitive?

        public void AddScheme(string name, Action<AuthenticationSchemeBuilder> configureBuilder)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (configureBuilder == null)
            {
                throw new ArgumentNullException(nameof(configureBuilder));
            }
            if (SchemeMap.ContainsKey(name))
            {
                throw new InvalidOperationException("Scheme already exists: " + name);
            }

            var builder = new AuthenticationSchemeBuilder(name);
            configureBuilder(builder);
            _schemes.Add(builder);
            SchemeMap[name] = builder;
        }

        public void ConfigureScheme(string name, Action<AuthenticationSchemeBuilder> configureBuilder)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (configureBuilder == null)
            {
                throw new ArgumentNullException(nameof(configureBuilder));
            }
            if (!SchemeMap.ContainsKey(name))
            {
                throw new InvalidOperationException("Scheme does not exists: " + name);
            }

            configureBuilder(SchemeMap[name]);
        }

        public string DefaultAuthenticationScheme { get; set; }

        public string DefaultSignInScheme { get; set; }

        //public string DefaultChallengeScheme { get; set; }

        /// <summary>
        /// Will be called after a successful authentication.
        /// </summary>
        public Func<ClaimsPrincipal, Task<ClaimsPrincipal>> ClaimsTransform { get; set; }
    }
}
