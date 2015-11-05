// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Authorization
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

        private List<Func<IServiceProvider, IAuthorizationRequirement>> _requirements = new List<Func<IServiceProvider, IAuthorizationRequirement>>();
        public IList<string> AuthenticationSchemes { get; set; } = new List<string>();

        public AuthorizationPolicyBuilder AddAuthenticationSchemes(params string[] schemes)
        {
            foreach (var authType in schemes)
            {
                AuthenticationSchemes.Add(authType);
            }
            return this;
        }

        public AuthorizationPolicyBuilder AddRequirement(Func<IServiceProvider, IAuthorizationRequirement> requirementFunc)
        {
            if (requirementFunc == null)
            {
                throw new ArgumentNullException(nameof(requirementFunc));
            }
            _requirements.Add(requirementFunc);
            return this;
        }

        public AuthorizationPolicyBuilder AddRequirement(IAuthorizationRequirement requirement)
        {
            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            return AddRequirement(services => requirement);
        }

        public AuthorizationPolicyBuilder AddRequirement<TRequirement>(params object[] arguments) where TRequirement : IAuthorizationRequirement
        {
            return AddRequirement(services => 
                (IAuthorizationRequirement)ActivatorUtilities.CreateInstance(services, typeof(TRequirement), arguments));
        }

        public AuthorizationPolicyBuilder Combine(AuthorizationPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            AddAuthenticationSchemes(policy.AuthenticationSchemes.ToArray());
            foreach (var req in policy.Requirements)
            {
                AddRequirement(req);
            }
            return this;
        }

        public AuthorizationPolicyBuilder Combine(AuthorizationPolicyBuilder policyBuilder)
        {
            if (policyBuilder == null)
            {
                throw new ArgumentNullException(nameof(policyBuilder));
            }

            AddAuthenticationSchemes(policyBuilder.AuthenticationSchemes.ToArray());
            _requirements.AddRange(policyBuilder._requirements);
            return this;
        }

        public AuthorizationPolicyBuilder RequireClaim(string claimType, params string[] requiredValues)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            return RequireClaim(claimType, (IEnumerable<string>)requiredValues);
        }

        public AuthorizationPolicyBuilder RequireClaim(string claimType, IEnumerable<string> requiredValues)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            AddRequirement<ClaimsAuthorizationRequirement>(claimType, requiredValues);
            return this;
        }

        public AuthorizationPolicyBuilder RequireClaim(string claimType)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            AddRequirement<ClaimsAuthorizationRequirement>(claimType, Enumerable.Empty<string>());
            return this;
        }

        public AuthorizationPolicyBuilder RequireRole(params string[] roles)
        {
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            return RequireRole((IEnumerable<string>)roles);
        }

        public AuthorizationPolicyBuilder RequireRole(IEnumerable<string> roles)
        {
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            AddRequirement<RolesAuthorizationRequirement>(roles);
            return this;
        }

        public AuthorizationPolicyBuilder RequireUserName(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            AddRequirement<NameAuthorizationRequirement>(userName);
            return this;
        }

        public AuthorizationPolicyBuilder RequireAuthenticatedUser()
        {
            AddRequirement<DenyAnonymousAuthorizationRequirement>();
            return this;
        }

        public AuthorizationPolicyBuilder RequireDelegate(Action<AuthorizationContext, DelegateRequirement> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            AddRequirement<DelegateRequirement>(handler);
            return this;
        }

        public AuthorizationPolicy Build()
        {
            return new AuthorizationPolicy(_requirements, AuthenticationSchemes.Distinct());
        }
    }
}