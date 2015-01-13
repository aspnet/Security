// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Security
{
    /// <summary>
    /// Contains authorization information used by <see cref="IAuthorizationPolicyHandler"/>.
    /// </summary>
    public class AuthorizationContext
    {
        private HashSet<IAuthorizationRequirement> _authorizedRequirements = new HashSet<IAuthorizationRequirement>();

        public AuthorizationContext(
            [NotNull] AuthorizationPolicy policy, 
            HttpContext context,
            object resource)
        {
            Policy = policy;
            Context = context;
            Resource = resource;
        }

        public AuthorizationPolicy Policy { get; private set; }
        public ClaimsPrincipal User { get { return Context.User; } }
        public HttpContext Context { get; private set; }
        public object Resource { get; private set; }

        public bool Allowed { get; private set; } = true;
        public void Deny()
        {
            Allowed = false;
        }

        public void RequirementSucceeded(IAuthorizationRequirement requirement)
        {
            _authorizedRequirements.Add(requirement);
        }

        public bool Authorized()
        {
            return Allowed && Policy.Requirements.All(req => _authorizedRequirements.Contains(req));
        }
    }
}

