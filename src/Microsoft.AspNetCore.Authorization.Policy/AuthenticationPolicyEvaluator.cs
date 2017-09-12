// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Implements <see cref="IAuthenticationPolicyEvaluator"/>.
    /// </summary>
    public class AuthenticationPolicyEvaluator : IAuthenticationPolicyEvaluator
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="policyProvider">The The <see cref="IAuthenticationPolicyProvider"/>.</param>
        public AuthenticationPolicyEvaluator(IAuthenticationPolicyProvider policyProvider)
        {
            Policies = policyProvider;
        }

        /// <summary>
        /// Used to lookup AuthenticationPolicies.
        /// </summary>
        public IAuthenticationPolicyProvider Policies { get; }

        /// <summary>
        /// Authenticate for the specified authentication policy.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="policyName">The name of the authentication policy.</param>
        /// <returns>The result.</returns>
        public virtual async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string policyName)
        {
            var policy = await Policies.GetAsync(context, policyName);
            if (policy != null && policy.AuthenticateSchemes.Any())
            {
                ClaimsPrincipal principal = null;
                foreach (var s in policy.AuthenticateSchemes)
                {
                    var auth = await context.AuthenticateAsync(s);
                    if (auth.Succeeded)
                    {
                        if (principal == null)
                        {
                            principal = auth.Principal;
                        }
                        else
                        {
                            // REVIEW: what about the auth properties, we just dropped them...
                            principal = SecurityHelper.MergeUserPrincipal(principal, auth.Principal);
                        }
                    }
                }
                if (principal != null)
                {
                    return AuthenticateResult.Success(new AuthenticationTicket(principal, "TBD: merged schemes?"));
                }
            }
            // If no policy or schemes, just fallback to default behavior.
            return await context.AuthenticateAsync();
        }

        /// <summary>
        /// Challenge the specified authentication policy.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="policyName">The name of the authentication policy.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        public virtual async Task ChallengeAsync(HttpContext context, string policyName, AuthenticationProperties properties)
        {
            var policy = await Policies.GetAsync(context, policyName);
            if (policy != null && policy.ChallengeSchemes.Any())
            {
                foreach (var s in policy.AuthenticateSchemes)
                {
                    await context.ChallengeAsync(s, properties);
                }
                return;
            }
            // If no policy or schemes, just fallback to default behavior.
            await context.ChallengeAsync(properties);
        }

        /// <summary>
        /// Forbid the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="policyName">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        public virtual async Task ForbidAsync(HttpContext context, string policyName, AuthenticationProperties properties)
        {
            var policy = await Policies.GetAsync(context, policyName);
            if (policy != null && policy.ForbidSchemes.Any())
            {
                foreach (var s in policy.AuthenticateSchemes)
                {
                    await context.ForbidAsync(s, properties);
                }
                return;
            }
            // If no policy or schemes, just fallback to default behavior.
            await context.ForbidAsync(properties);
        }
    }
}
