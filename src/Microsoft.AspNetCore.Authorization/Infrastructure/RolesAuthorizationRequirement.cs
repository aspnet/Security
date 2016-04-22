// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authorization.Infrastructure
{
    // Must belong to with one of specified roles
    // If AllowedRoles is null or empty, that means any role is valid
    public class RolesAuthorizationRequirement : AuthorizationHandler<RolesAuthorizationRequirement>, IAuthorizationRequirement
    {
        public RolesAuthorizationRequirement(IEnumerable<string> allowedRoles)
        {
            if (allowedRoles == null)
            {
                throw new ArgumentNullException(nameof(allowedRoles));
            }

            if (allowedRoles.Count() == 0)
            {
                throw new InvalidOperationException(Resources.Exception_RoleRequirementEmpty);
            }
            AllowedRoles = allowedRoles;
        }

        public IEnumerable<string> AllowedRoles { get; }

        protected override void Handle(AuthorizationContext context, RolesAuthorizationRequirement requirement)
        {
            var user = context.AuthorizationData as ClaimsPrincipal;
            if (user == null)
            {
                context.Fail();
                return;
            }

            bool found = false;
            if (requirement.AllowedRoles == null || !requirement.AllowedRoles.Any())
            {
                // Review: What do we want to do here?  No roles requested is auto success?
            }
            else
            {
                found = requirement.AllowedRoles.Any(r => user.IsInRole(r));
            }
            if (found)
            {
                context.Succeed(requirement);
            }
        }

    }
}
