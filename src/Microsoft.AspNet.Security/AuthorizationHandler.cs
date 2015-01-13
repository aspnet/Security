// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    // Music store use case

    // await AuthorizeAsync<Album>(user, "Edit", albumInstance);

    // No policy name needed because this is auto based on resource (operation is the policy name)
    //RegisterOperation which auto generates the policy for Authorize<T>
    //bool AuthorizeAsync<TResource>(ClaimsPrincipal, string operation, TResource instance)
    //bool AuthorizeAsync<TResource>(IAuthorization, ClaimsPrincipal, string operation, TResource instance)

    public abstract class AuthorizationHandler<TResource> : IAuthorizationHandler where TResource : class
    {
        public virtual Task HandleAsync(AuthorizationContext context)
        {
            var resource = context.Resource as TResource;
            if (resource != null)
            {
                return HandleAsync(context, resource);
            }

            return Task.FromResult(0);

        }

        public abstract Task HandleAsync(AuthorizationContext context, TResource resource);
    }
}