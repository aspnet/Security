// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Security
{
    public class AuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly List<IAuthorizationRequirement> _reqs = new List<IAuthorizationRequirement>();

        public AuthorizationPolicy(params string[] authTypesFilter)
        {
            AuthenticationTypesFilter = authTypesFilter;
        }

        // REVIEW: rename IncludedAuthenticationTypes?
        // NOTE: null or no auth types means use all auth types
        public IEnumerable<string> AuthenticationTypesFilter { get; private set; }

        public IEnumerable<IAuthorizationRequirement> Requirements { get { return _reqs; } }

        public AuthorizationPolicy RequiresClaim([NotNull] string claimType, params string[] requiredValues)
        {
            _reqs.Add(new ClaimRequirement
            {
                AuthenticationTypesFilter = AuthenticationTypesFilter,
                ClaimType = claimType,
                ClaimValueRequirement = requiredValues
            });
            return this;
        }

        public AuthorizationPolicy RequiresClaim([NotNull] string claimType)
        {
            _reqs.Add(new ClaimRequirement
            {
                AuthenticationTypesFilter = AuthenticationTypesFilter,
                ClaimType = claimType,
                ClaimValueRequirement = null
            });
            return this;
        }

        public AuthorizationPolicy Requires([NotNull] IAuthorizationRequirement req)
        {
            _reqs.Add(req);
            return this;
        }
    }
}
