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

        public AuthorizationPolicy(params string[] authTypes)
        {
            AuthenticationTypes = authTypes;
        }

        // NOTE: null auth types means use all auth types
        public IEnumerable<string> AuthenticationTypes { get; private set; }

        public IEnumerable<AuthorizationClaimRequirement> Requirements { get { return _reqs; } }

        public AuthorizationPolicy Requires(string claimType, params string[] requiredValues)
        {
            _reqs.Add(new AuthorizationClaimRequirement
            {
                ClaimType = claimType,
                ClaimValueRequirement = requiredValues
            });
            return this;
        }
    }
}
