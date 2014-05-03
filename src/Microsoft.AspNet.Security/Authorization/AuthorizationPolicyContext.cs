﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace Microsoft.AspNet.Security.Authorization
{
    /// <summary>
    /// Contains authorization information used by <see cref="IAuthorizationPolicy"/>.
    /// </summary>
    public class AuthorizationPolicyContext
    {
        public AuthorizationPolicyContext(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource )
        {
            Claims = (claims ?? Enumerable.Empty<Claim>()).ToList();
            User = user;
            Resource = resource;

            UserClaims = new List<Claim>();

            if(user != null)
            {
                // user claims are copied to a new and mutable list
                UserClaims = user.Claims.ToList();
            }
        }

        /// <summary>
        /// The list of claims the <see cref="IAuthorizationService"/> is checking.
        /// </summary>
        public IList<Claim> Claims { get; private set; }

        /// <summary>
        /// The user to check the claims against.
        /// </summary>
        public ClaimsPrincipal User { get; private set; }

        /// <summary>
        /// The claims of the user.
        /// </summary>
        /// <remarks>
        /// This list can be modified by policies for retries.
        /// </remarks>
        public IList<Claim> UserClaims { get; private set; }

        /// <summary>
        /// An optional resource associated to the check.
        /// </summary>
        public object Resource { get; private set; }

        /// <summary>
        /// Gets or set whether the permission will be granted to the user.
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// When set to <value>true</value>, the authorization check will be processed again.
        /// </summary>
        public bool Retry { get; set; }
    }
}
