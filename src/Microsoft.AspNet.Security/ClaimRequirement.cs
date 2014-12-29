// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    // Music store use case

    // await AuthorizeAsync<Album>(user, "Edit", albumInstance);

    // No policy name needed because this is auto based on resource (operation is the policy name)
    //RegisterOperation which auto generates the policy for Authorize<T>
    //bool AuthorizeAsync<TResource>(ClaimsPrincipal, string operation, TResource instance)
    //bool AuthorizeAsync<TResource>(IAuthorization, ClaimsPrincipal, string operation, TResource instance)

    // Must contain a claim with the specified name, and at least one of the required values
    // If ValueRequirement is null or empty, that means any claim is valid
    public class ClaimRequirement : IAuthorizationRequirement
    {
        public IEnumerable<string> AuthenticationTypesFilter { get; set; }
        public string ClaimType { get; set; }
        public IEnumerable<string> AllowedValues { get; set; }

        public Task<bool> CheckAsync(AuthorizationContext context)
        {
            if (context.User == null)
            {
                return Task.FromResult(false);
            }

            // TODO: optimize this
            var filteredIdentities = context.User.Identities;
            if (AuthenticationTypesFilter != null && AuthenticationTypesFilter.Any())
            {
                filteredIdentities = filteredIdentities.Where(id => AuthenticationTypesFilter.Contains(id.AuthenticationType));
            }
            bool found = false;
            if (AllowedValues == null || !AllowedValues.Any())
            {
                found = context.User.Claims.Any(c => string.Equals(c.Type, ClaimType, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                found = context.User.Claims.Any(c => string.Equals(c.Type, ClaimType, StringComparison.OrdinalIgnoreCase)
                                                 && AllowedValues.Contains(c.Value, StringComparer.Ordinal));
            }
            return Task.FromResult(found);
        }
    }
}
