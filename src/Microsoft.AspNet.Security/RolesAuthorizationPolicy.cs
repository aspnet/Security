// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.AspNet.Security
{
    public class RolesAuthorizationPolicy : AuthorizationPolicy
    {
        public RolesAuthorizationPolicy(params string[] authTypes) : base(authTypes) { }

        public RolesAuthorizationPolicy RequiresRole([NotNull] params string[] roles)
        {
            Requires(ClaimTypes.Role, roles);
            return this;
        }
    }
}
