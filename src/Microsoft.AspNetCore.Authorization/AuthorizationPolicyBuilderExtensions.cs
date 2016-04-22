// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireClaim(this AuthorizationPolicyBuilder builder, string claimType, params string[] requiredValues)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            return builder.RequireClaim(claimType, (IEnumerable<string>)requiredValues);
        }

        public static AuthorizationPolicyBuilder RequireClaim(this AuthorizationPolicyBuilder builder, string claimType, IEnumerable<string> requiredValues)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            builder.Requirements.Add(new ClaimsAuthorizationRequirement(claimType, requiredValues));
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireClaim(this AuthorizationPolicyBuilder builder, string claimType)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            builder.Requirements.Add(new ClaimsAuthorizationRequirement(claimType, allowedValues: null));
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireRole(this AuthorizationPolicyBuilder builder, params string[] roles)
        {
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            return builder.RequireRole((IEnumerable<string>)roles);
        }

        public static AuthorizationPolicyBuilder RequireRole(this AuthorizationPolicyBuilder builder, IEnumerable<string> roles)
        {
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            builder.Requirements.Add(new RolesAuthorizationRequirement(roles));
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireUserName(this AuthorizationPolicyBuilder builder, string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            builder.Requirements.Add(new NameAuthorizationRequirement(userName));
            return builder;
        }

        public static AuthorizationPolicyBuilder RequireAuthenticatedUser(this AuthorizationPolicyBuilder builder)
        {
            builder.Requirements.Add(new DenyAnonymousAuthorizationRequirement());
            return builder;
        }
    }
}
