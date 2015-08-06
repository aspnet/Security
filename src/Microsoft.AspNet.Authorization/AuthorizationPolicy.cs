// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Authorization
{
    public class AuthorizationPolicy
    {
        public AuthorizationPolicy([NotNull] IEnumerable<IAuthorizationRequirement> requirements, [NotNull] IEnumerable<string> activeAuthenticationSchemes)
        {
            if (requirements.Count() == 0)
            {
                throw new InvalidOperationException(Resources.Exception_AuthorizationPolicyEmpty);
            }
            Requirements = new List<IAuthorizationRequirement>(requirements).AsReadOnly();
            ActiveAuthenticationSchemes = new List<string>(activeAuthenticationSchemes).AsReadOnly();
        }

        public IReadOnlyList<IAuthorizationRequirement> Requirements { get; }
        public IReadOnlyList<string> ActiveAuthenticationSchemes { get; }

        public static AuthorizationPolicy Combine([NotNull] params AuthorizationPolicy[] policies)
        {
            return Combine((IEnumerable<AuthorizationPolicy>)policies);
        }

        public static AuthorizationPolicy Combine([NotNull] IEnumerable<AuthorizationPolicy> policies)
        {
            var builder = new AuthorizationPolicyBuilder();
            foreach (var policy in policies)
            {
                builder.Combine(policy);
            }
            return builder.Build();
        }

        public static AuthorizationPolicy Combine([NotNull] AuthorizationOptions options, [NotNull] IEnumerable<AuthorizeAttribute> attributes)
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            var any = false;
            foreach (var authorizeAttribute in attributes.OfType<AuthorizeAttribute>())
            {
                any = true;
                var requireAnyAuthenticated = true;
                if (!string.IsNullOrEmpty(authorizeAttribute.Policy))
                {
                    var policy = options.GetPolicy(authorizeAttribute.Policy);
                    if (policy == null)
                    {
                        throw new InvalidOperationException(Resources.FormatException_AuthorizationPolicyNotFound(authorizeAttribute.Policy));
                    }
                    policyBuilder.Combine(policy);
                    requireAnyAuthenticated = false;
                }
                var rolesSplit = authorizeAttribute.Roles?.Split(',');
                if (rolesSplit != null && rolesSplit.Any())
                {
                    policyBuilder.RequireRole(rolesSplit);
                    requireAnyAuthenticated = false;
                }
                var authTypesSplit = authorizeAttribute.ActiveAuthenticationSchemes?.Split(',');
                if (authTypesSplit != null && authTypesSplit.Any())
                {
                    foreach (var authType in authTypesSplit)
                    {
                        policyBuilder.ActiveAuthenticationSchemes.Add(authType);
                    }
                }
                if (requireAnyAuthenticated)
                {
                    policyBuilder.RequireAuthenticatedUser();
                }
            }
            return any ? policyBuilder.Build() : null;
        }
    }
}