// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security.Authorization
{
    /// <summary>
    /// This class provides a base implementation for <see cref="IAuthorizationPolicy" />
    /// </summary>
    public abstract class AuthorizationPolicy : IAuthorizationPolicy
    {
        public int Order { get; set; }
        
        public virtual async Task ApplyingAsync(AuthorizationPolicyContext context)
        {
            await Task.FromResult(0);
        }

        public virtual async Task ApplyAsync(AuthorizationPolicyContext context) 
        {
            await Task.FromResult(0);
        }

        public virtual async Task AppliedAsync(AuthorizationPolicyContext context)
        {
            await Task.FromResult(0);
        }
    }
}
