// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    public interface IAuthorizationManager
    {
        // User is asking for the following named actions
        Task<bool> AuthorizeAsync<TPolicy>(ClaimsPrincipal user, params object[] resources) where TPolicy : IAuthorizationPolicy;

        void Register<TPolicy>();
    }

    public class AuthorizationContext
    {
        //Support multiple? public IEnumerable<IAuthorizationAction> Policy { get; private set; }
        public IAuthorizationPolicy Policy { get; private set; }
        public ClaimsPrincipal User { get; private set; }
        public IEnumerable<object> Resources { get; private set; }
        public bool Authorized { get; set; }
    }

    public interface IAuthorizationPolicy 
    {
        // Unique name for the policy
        string Name { get; }

        Task AuthorizeAsync(AuthorizationContext context);
    }
    // TODO: Want to name it IAuthorizationPolicy but its taken, also could represent an action?

    // [Authorize(Role = "foo")] would map to this with claims = ClaimTypes.Role, Value = "foo");
    // [Authorize(Role = "foo")] would map to this with claims = ClaimTypes.Role, Value = "foo");
    // [Authorize(Policy = "policy")] would map to this a particular policy);
    public class RequiredAnyClaimsPolicy : IAuthorizationPolicy
    {
        private readonly IEnumerable<Claim> _claims;

        public RequiredAnyClaimsPolicy(string name, IEnumerable<Claim> claims)
        {
            Name = name;
            _claims = claims;
        }

        public string Name { get; private set; }

        public Task AuthorizeAsync(AuthorizationContext context)
        {
            context.Authorized = ClaimsMatch(context.User.Claims.ToList(), _claims);
            return Task.FromResult(0);
        }

        private static bool ClaimsMatch([NotNull] IEnumerable<Claim> x, [NotNull] IEnumerable<Claim> y)
        {
            return x.Any(claim =>
                        y.Any(userClaim =>
                            string.Equals(claim.Type, userClaim.Type, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(claim.Value, userClaim.Value, StringComparison.Ordinal)
                        )
                    );

        }
    }

    public class AllowSpecificUsersPolicy : IAuthorizationPolicy
    {
        private readonly IEnumerable<string> _allowedUserIds;

        public AllowSpecificUsersPolicy(string name, IEnumerable<string> allowedUserIds)
        {
            Name = name;
            _allowedUserIds = allowedUserIds;
        }

        public string Name { get; private set; }

        public Task AuthorizeAsync(AuthorizationContext context)
        {
            var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null) {
                context.Authorized = _allowedUserIds.Any(id => id == userIdClaim.Value);
            }
            return Task.FromResult(0);
        }
    }



    /// <summary>
    /// Checks claims based permissions for a user.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks if a user has specific claims for a specific context obj.
        /// </summary>
        /// <param name="claims">The claims to check against a specific user.</param>
        /// <param name="user">The user to check claims against.</param>
        /// <param name="resource">The resource the claims should be check with.</param>
        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
        Task<bool> AuthorizeAsync(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource);

        /// <summary>
        /// Checks if a user has specific claims for a specific context obj.
        /// </summary>
        /// <param name="claims">The claims to check against a specific user.</param>
        /// <param name="user">The user to check claims against.</param>
        /// <param name="resource">The resource the claims should be check with.</param>
        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
        bool Authorize(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource);

    }
}