// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authorization.Infrastructure
{
    // Must contain a claim with the specified name, and at least one of the required values
    // If AllowedValues is null or empty, that means any claim is valid
    public class ClaimsAuthorizationRequirement : AuthorizationHandler<ClaimsAuthorizationRequirement>, IAuthorizationRequirement
    {
        public ClaimsAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            ClaimType = claimType;
            AllowedValues = allowedValues;
        }

        public string ClaimType { get; }
        public IEnumerable<string> AllowedValues { get; }

        protected override void Handle(AuthorizationContext context, ClaimsAuthorizationRequirement requirement)
        {
            var user = context.AuthorizationData as ClaimsPrincipal;
            if (user == null)
            {
                context.Fail();
                return;
            }

            var found = false;

            if (requirement.AllowedValues == null || !requirement.AllowedValues.Any())
            {
                found = user.Claims.Any(c => string.Equals(c.Type, requirement.ClaimType, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                found = user.Claims.Any(c => string.Equals(c.Type, requirement.ClaimType, StringComparison.OrdinalIgnoreCase)
                                                    && requirement.AllowedValues.Contains(c.Value, StringComparer.Ordinal));
            }
            if (found)
            {
                context.Succeed(requirement, user.GetUserName());
            }

        }
    }
}
