// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Checks policy based permissions for a user
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks if a user meets a specific set of requirements for the specified resource
        /// </summary>
        /// <param name="authorizationData"></param>
        /// <param name="resource"></param>
        /// <param name="requirements"></param>
        /// <returns></returns>
        Task<bool> AuthorizeAsync(object authorizationData, object resource, IEnumerable<IAuthorizationRequirement> requirements);

        /// <summary>
        /// Checks if a user meets a specific authorization policy
        /// </summary>
        /// <param name="authorizationData">The data to check the policy against.</param>
        /// <param name="resource">The resource the policy should be checked with.</param>
        /// <param name="policyName">The name of the policy to check against a specific context.</param>
        /// <returns><value>true</value> when the user fulfills the policy, <value>false</value> otherwise.</returns>
        Task<bool> AuthorizeAsync(object authorizationData, object resource, string policyName);
    }
}