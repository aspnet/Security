// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Used to provide authentication using policies.
    /// </summary>
    public interface IAuthenticationPolicyEvaluator
    {
        /// <summary>
        /// Authenticate for the specified authentication policy.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="policyName">The name of the authentication policy.</param>
        /// <returns>The result.</returns>
        Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string policyName);

        /// <summary>
        /// Challenge the specified authentication policy.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="policyName">The name of the authentication policy.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        Task ChallengeAsync(HttpContext context, string policyName, AuthenticationProperties properties);

        /// <summary>
        /// Forbids the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="policyName">The name of the authentication policy.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        Task ForbidAsync(HttpContext context, string policyName, AuthenticationProperties properties);
    }
}
