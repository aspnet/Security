// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Security
{


    // TODO: make policy options read only
    public class AuthorizationPolicyOptions
    {
        IDictionary<string, IAuthorizationPolicy> Policies { get; } = new Dictionary<string, IAuthorizationPolicy>();

        // Sample usage
        public void IBeExample()
        {
            var services = new ServiceCollection();
            services.Configure<AuthorizationPolicyOptions>(options =>
            {
                options.Policies["name"] = new AuthorizationPolicy("Bearer", "X509")
                                                .Requires("XClaim", "X", "Y", "Z")
                                                .Requires("YClaim", "A", "B", "C");
            });
        }
    }

    //public interface IAuthorizationPolicy
    //{
    //    // Auth types requested by this policy
    //    IEnumerable<string> AuthenticationTypes { get; }

    //    IEnumerable<AuthorizationClaimRequirement> Requirements { get; }
    //}

    //// Must contain a claim with the specified name, and at least one of the required values
    //public class AuthorizationClaimRequirement
    //{
    //    public string ClaimName { get; set; }
    //    public IEnumerable<string> ClaimValueRequirement { get; set; }
    //}

    // Default implementation will take AuthorizationPolicyOptions and IEnumerable<IAuthorizationPolicyHandler>
    // Calls AuthorizeAsync on handlers that apply (TODO: IAuthorizationPolicyHandler<T>)
    public interface IAuthorizationService
    {
        Task<bool> AuthorizeAsync(IAuthorizationPolicy policy, ClaimsPrincipal user, params object[] resources);
    }

    // This guy does the work, default implementation will verify requirements from context.User
    public interface IAuthorizationPolicyHandler
    {
        // REVIEW: should this be void and just manipulate Authorized on context instead?
        Task<bool> AuthorizeAsync(AuthorizationContext context);
    }


/// <summary>
/// AuthorizeAttributes just contain data mapping to a particular Authorize call on the IAuthorizationManager
/// which will be in DI
/// </summary>



    //public interface IAuthorizationManager
    //{
    //    // Authorize based on policy type
    //    Task<bool> AuthorizeAsync<TPolicy>(ClaimsPrincipal user, params object[] resources) where TPolicy : IAuthorizationPolicy;

    //    // DefaultAuthManager Impl will get IEnumerable<IAuthorizationPolicy> from DI

    //    // Authorize based on specific policy name
    //    Task<bool> AuthorizeAsync(string policy, ClaimsPrincipal user, params object[] resources);
    //    IDictionary<string, IAuthorizationPolicy> PolicyMap { get; }
    //}

    //// [Authorize(Role = "foo")] would map to this with claims = ClaimTypes.Role, Value = "foo");
    //// [Authorize(Role = "foo")] would map to this with claims = ClaimTypes.Role, Value = "foo");
    //// [Authorize(Policy = "policy")] would map to this a particular policy);
    //public class RequiredAnyClaimsPolicy : IAuthorizationPolicy
    //{
    //    private readonly IEnumerable<Claim> _claims;

    //    public RequiredAnyClaimsPolicy(string name, IEnumerable<Claim> claims)
    //    {
    //        Name = name;
    //        _claims = claims;
    //    }

    //    public string Name { get; private set; }

    //    public Task AuthorizeAsync(AuthorizationContext context)
    //    {
    //        context.Authorized = ClaimsMatch(context.User.Claims.ToList(), _claims);
    //        return Task.FromResult(0);
    //    }

    //    private static bool ClaimsMatch([NotNull] IEnumerable<Claim> x, [NotNull] IEnumerable<Claim> y)
    //    {
    //        return x.Any(claim =>
    //                    y.Any(userClaim =>
    //                        string.Equals(claim.Type, userClaim.Type, StringComparison.OrdinalIgnoreCase) &&
    //                        string.Equals(claim.Value, userClaim.Value, StringComparison.Ordinal)
    //                    )
    //                );

    //    }
    //}

    //public class AllowSpecificUsersPolicy : IAuthorizationPolicy
    //{
    //    private readonly IEnumerable<string> _allowedUserIds;

    //    public AllowSpecificUsersPolicy(string name, IEnumerable<string> allowedUserIds)
    //    {
    //        Name = name;
    //        _allowedUserIds = allowedUserIds;
    //    }

    //    public string Name { get; private set; }

    //    public Task AuthorizeAsync(AuthorizationContext context)
    //    {
    //        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
    //        if (userIdClaim != null) {
    //            context.Authorized = _allowedUserIds.Any(id => id == userIdClaim.Value);
    //        }
    //        return Task.FromResult(0);
    //    }
    //}


    // OLD CODE below

    ///// <summary>
    ///// Checks claims based permissions for a user.
    ///// </summary>
    //public interface IAuthorizationService
    //{
    //    /// <summary>
    //    /// Checks if a user has specific claims for a specific context obj.
    //    /// </summary>
    //    /// <param name="claims">The claims to check against a specific user.</param>
    //    /// <param name="user">The user to check claims against.</param>
    //    /// <param name="resource">The resource the claims should be check with.</param>
    //    /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
    //    Task<bool> AuthorizeAsync(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource);

    //    /// <summary>
    //    /// Checks if a user has specific claims for a specific context obj.
    //    /// </summary>
    //    /// <param name="claims">The claims to check against a specific user.</param>
    //    /// <param name="user">The user to check claims against.</param>
    //    /// <param name="resource">The resource the claims should be check with.</param>
    //    /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
    //    bool Authorize(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource);

    //}
}