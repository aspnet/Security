// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Authorization
{
    public class AuthorizationOptions
    {
        private IDictionary<string, AuthorizationPolicyBuilder> PolicyMap { get; } = new Dictionary<string, AuthorizationPolicyBuilder>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The initial default policy is to require any authenticated user
        /// </summary>
        public AuthorizationPolicyBuilder DefaultPolicy { get; set; } = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();

        public void AddPolicy(string name, Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configurePolicy == null)
            {
                throw new ArgumentNullException(nameof(configurePolicy));
            }

            var policyBuilder = new AuthorizationPolicyBuilder();
            configurePolicy(policyBuilder);
            PolicyMap[name] = policyBuilder;
        }

        public AuthorizationPolicyBuilder GetPolicy(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return PolicyMap.ContainsKey(name) ? PolicyMap[name] : null;
        }
    }
}