// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNet.Security
{
    public class AuthorizationPolicyBuilder
    {
        public IList<IAuthorizationRequirement> Requirements { get; set; } = new List<IAuthorizationRequirement>();
        //TODO: Need to find a real name for this,  Service will AuthenticateAsync for these if 
        public IList<string> UseOnlyTheseAuthenticationTypes { get; set; } = new List<string>();

        public AuthorizationPolicyBuilder RequiresClaim([NotNull] string claimType, params string[] requiredValues)
        {
            Requirements.Add(new ClaimRequirement
            {
                ClaimType = claimType,
                AllowedValues = requiredValues
            });
            return this;
        }

        public AuthorizationPolicyBuilder RequiresClaim([NotNull] string claimType)
        {
            Requirements.Add(new ClaimRequirement
            {
                ClaimType = claimType,
                AllowedValues = null
            });
            return this;
        }

        public AuthorizationPolicyBuilder RequiresRole([NotNull] params string[] roles)
        {
            RequiresClaim(ClaimTypes.Role, roles);
            return this;
        }

        public AuthorizationPolicy Build()
        {
            return new AuthorizationPolicy(Requirements, UseOnlyTheseAuthenticationTypes);
        }
    }

    // Music store use case

    // await AuthorizeAsync<Album>(user, "Edit", albumInstance);

    // No policy name needed because this is auto based on resource (operation is the policy name)
    //RegisterOperation which auto generates the policy for Authorize<T>
    //bool AuthorizeAsync<TResource>(ClaimsPrincipal, string operation, TResource instance)
    //bool AuthorizeAsync<TResource>(IAuthorization, ClaimsPrincipal, string operation, TResource instance)

    //public abstract class ResourceAuthorizationHandler<TResource> : IAuthorizationHandler where TResource : class
    //{
    //    public virtual Task HandleAsync(AuthorizationContext context)
    //    {
    //        var resource = context.Resource as TResource;
    //        if (resource != null)
    //        {
    //            return HandleAsync(context, resource);
    //        }

    //        return Task.FromResult(0);

    //    }

    //    public abstract Task HandleAsync(AuthorizationContext context, TResource resource);
    //}
}
