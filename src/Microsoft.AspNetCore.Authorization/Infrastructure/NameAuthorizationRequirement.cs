// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authorization.Infrastructure
{
    /// <summary>
    /// Requirement that ensures a specific Name
    /// </summary>
    public class NameAuthorizationRequirement : AuthorizationHandler<NameAuthorizationRequirement>, IAuthorizationRequirement
    {
        public NameAuthorizationRequirement(string requiredName)
        {
            if (requiredName == null)
            {
                throw new ArgumentNullException(nameof(requiredName));
            }

            RequiredName = requiredName;
        }

        public string RequiredName { get; }

        protected override void Handle(AuthorizationContext context, NameAuthorizationRequirement requirement)
        {
            var user = context.AuthorizationData as ClaimsPrincipal;
            if (user == null)
            {
                context.Fail();
                return;
            }

            // REVIEW: Do we need to do normalization?  casing/loc?
            if (user.Identities.Any(i => string.Equals(i.Name, requirement.RequiredName)))
            {
                context.Succeed(requirement);
            }
        }
    }
}
