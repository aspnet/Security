// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNet.Security
{
    /// <summary>
    /// Contains authorization information used by <see cref="IAuthorizationPolicyHandler"/>.
    /// </summary>
    public class AuthorizationContext
    {
        public AuthorizationContext(
            [NotNull] IAuthorizationPolicy policy, 
            [NotNull] ClaimsPrincipal user, 
            IEnumerable<object> resources)
        {
            Policy = policy;
            User = user;
            Resources = resources;
        }

        public IAuthorizationPolicy Policy { get; private set; }
        public ClaimsPrincipal User { get; private set; }
        public IEnumerable<object> Resources { get; private set; }
        public bool Authorized { get; set; }
    }
}

