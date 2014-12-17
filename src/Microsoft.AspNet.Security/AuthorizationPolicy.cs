// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    public class AuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly List<AuthorizationClaimRequirement> _reqs = new List<AuthorizationClaimRequirement>();
        private readonly List<IAuthorizationPolicyHandler> _handlers = new List<IAuthorizationPolicyHandler>();

        public AuthorizationPolicy(params string[] authTypes)
        {
            AuthenticationTypes = authTypes;
        }

        // REVIEW: rename IncludedAuthenticationTypes?
        // NOTE: null or no auth types means use all auth types
        public IEnumerable<string> AuthenticationTypes { get; private set; }

        public IEnumerable<AuthorizationClaimRequirement> Requirements { get { return _reqs; } }

        public IEnumerable<IAuthorizationPolicyHandler> Handlers { get { return _handlers; } }

        public AuthorizationPolicy Requires([NotNull] string claimType, params string[] requiredValues)
        {
            _reqs.Add(new AuthorizationClaimRequirement
            {
                ClaimType = claimType,
                ClaimValueRequirement = requiredValues
            });
            return this;
        }

        public AuthorizationPolicy RequiresAny([NotNull] string claimType)
        {
            _reqs.Add(new AuthorizationClaimRequirement
            {
                ClaimType = claimType,
                ClaimValueRequirement = null
            });
            return this;
        }

        public AuthorizationPolicy AddHandler([NotNull] IAuthorizationPolicyHandler handler)
        {
            _handlers.Add(handler);
            return this;
        }
    }
}
