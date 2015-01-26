// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    // Music store use case

    // await AuthorizeAsync<Album>(user, "Edit", albumInstance);

    // No policy name needed because this is auto based on resource (operation is the policy name)
    //RegisterOperation which auto generates the policy for Authorize<T>
    //bool AuthorizeAsync<TResource>(ClaimsPrincipal, string operation, TResource instance)
    //bool AuthorizeAsync<TResource>(IAuthorization, ClaimsPrincipal, string operation, TResource instance)
    public abstract class AuthorizationHandler<TRequirement> : IAuthorizationHandler
        where TRequirement : IAuthorizationRequirement
    {
        public virtual async Task HandleAsync(AuthorizationContext context)
        {
            foreach (var req in context.Requirements.OfType<TRequirement>())
            {
                await HandleAsync(context, req);
            }
        }

        public abstract Task HandleAsync(AuthorizationContext context, TRequirement requirement);
    }

    public abstract class AuthorizationHandler<TRequirement, TResource> : IAuthorizationHandler
        where TResource : class
        where TRequirement : IAuthorizationRequirement
    {
        public virtual async Task HandleAsync(AuthorizationContext context)
        {
            var resource = context.Resource as TResource;
            // REVIEW: should we allow null resources?
            if (resource != null)
            {
                foreach (var req in context.Requirements.OfType<TRequirement>())
                {
                    await HandleAsync(context, req, resource);
                }
            }
        }

        public abstract Task HandleAsync(AuthorizationContext context, TRequirement requirement, TResource resource);
    }
}