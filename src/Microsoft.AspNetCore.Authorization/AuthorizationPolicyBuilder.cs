// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Microsoft.AspNetCore.Authorization
{
    public class AuthorizationPolicyBuilder
    {
        public AuthorizationPolicyBuilder(params string[] authenticationSchemes)
        {
            AddAuthenticationSchemes(authenticationSchemes);
        }

        public AuthorizationPolicyBuilder(AuthorizationPolicy policy)
        {
            Combine(policy);
        }

        public IList<IAuthorizationRequirement> Requirements { get; set; } = new List<IAuthorizationRequirement>();
        public IList<string> AuthenticationSchemes { get; set; } = new List<string>();

        public AuthorizationPolicyBuilder AddAuthenticationSchemes(params string[] schemes)
        {
            foreach (var authType in schemes)
            {
                AuthenticationSchemes.Add(authType);
            }
            return this;
        }

        public AuthorizationPolicyBuilder AddRequirements(params IAuthorizationRequirement[] requirements)
        {
            foreach (var req in requirements)
            {
                Requirements.Add(req);
            }
            return this;
        }

        public AuthorizationPolicyBuilder Combine(AuthorizationPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            AddAuthenticationSchemes(policy.AuthenticationSchemes.ToArray());
            AddRequirements(policy.Requirements.ToArray());
            return this;
        }

        /// <summary>
        /// Requires that this Function returns true
        /// </summary>
        /// <param name="assert">Function that must return true</param>
        /// <returns></returns>
        public AuthorizationPolicyBuilder RequireAssertion(Func<AuthorizationContext, bool> assert)
        {
            if (assert == null)
            {
                throw new ArgumentNullException(nameof(assert));
            }

            Requirements.Add(new AssertionRequirement(assert));
            return this;
        }

        /// <summary>
        /// Requires that this Function returns true
        /// </summary>
        /// <param name="assert">Function that must return true</param>
        /// <returns></returns>
        public AuthorizationPolicyBuilder RequireAssertion(Func<AuthorizationContext, Task<bool>> assert)
        {
            if (assert == null)
            {
                throw new ArgumentNullException(nameof(assert));
            }

            Requirements.Add(new AssertionRequirement(assert));
            return this;
        }

        public AuthorizationPolicy Build()
        {
            return new AuthorizationPolicy(Requirements, AuthenticationSchemes.Distinct());
        }
    }
}