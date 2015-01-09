// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    public class DefaultAuthoriziationPolicyHandler : IAuthorizationPolicyHandler
    {
        public Task<bool> AuthorizeAsync([NotNull] AuthorizationContext context)
        {
            // TODO: optimize this
            var filteredIdentities = context.User.Identities;
            if (context.Policy.AuthenticationTypes != null && context.Policy.AuthenticationTypes.Any())
            {
                filteredIdentities = filteredIdentities.Where(id => context.Policy.AuthenticationTypes.Contains(id.AuthenticationType));
            }
            foreach (var requires in context.Policy.Requirements)
            {
                bool found = false;
                foreach (var identity in filteredIdentities)
                {
                    // Just check for presence of the claim type if no values specified
                    if (requires.ClaimValueRequirement == null || !requires.ClaimValueRequirement.Any())
                    {
                        found = identity.Claims.Any(c => string.Equals(c.Type, requires.ClaimType, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        found = identity.Claims.Any(c => string.Equals(c.Type, requires.ClaimType, StringComparison.OrdinalIgnoreCase)
                                                         && requires.ClaimValueRequirement.Contains(c.Value, StringComparer.Ordinal));
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (!found)
                {
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }
    }
}