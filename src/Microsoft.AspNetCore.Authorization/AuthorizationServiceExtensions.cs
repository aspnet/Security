// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        /// <summary>
        /// Checks if a user meets a specific requirement for the specified resource
        /// </summary>
        /// <param name="service">The <see cref="IAuthorizationService"/>.</param>
        /// <param name="authorizationData"></param>
        /// <param name="resource"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, object authorizationData, object resource, IAuthorizationRequirement requirement)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            return service.AuthorizeAsync(authorizationData, resource, new IAuthorizationRequirement[] { requirement });
        }

        /// <summary>
        /// Checks if a user meets a specific authorization policy
        /// </summary>
        /// <param name="service">The authorization service.</param>
        /// <param name="authorizationData">The data to check the policy against.</param>
        /// <param name="resource">The resource the policy should be checked with.</param>
        /// <param name="policy">The policy to check against a specific context.</param>
        /// <returns><value>true</value> when the user fulfills the policy, <value>false</value> otherwise.</returns>
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, object authorizationData, object resource, AuthorizationPolicy policy)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            return service.AuthorizeAsync(authorizationData, resource, policy.Requirements.ToArray());
        }

        /// <summary>
        /// Checks if a user meets a specific authorization policy
        /// </summary>
        /// <param name="service">The authorization service.</param>
        /// <param name="authorizationData">The data to check the policy against.</param>
        /// <param name="policy">The policy to check against a specific context.</param>
        /// <returns><value>true</value> when the user fulfills the policy, <value>false</value> otherwise.</returns>
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, object authorizationData, AuthorizationPolicy policy)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            return service.AuthorizeAsync(authorizationData, resource: null, policy: policy);
        }

        /// <summary>
        /// Checks if a user meets a specific authorization policy
        /// </summary>
        /// <param name="service">The authorization service.</param>
        /// <param name="authorizationData">The data to check the policy against.</param>
        /// <param name="policyName">The name of the policy to check against a specific context.</param>
        /// <returns><value>true</value> when the user fulfills the policy, <value>false</value> otherwise.</returns>
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, object authorizationData, string policyName)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (policyName == null)
            {
                throw new ArgumentNullException(nameof(policyName));
            }

            return service.AuthorizeAsync(authorizationData, resource: null, policyName: policyName);
        }
    }
}