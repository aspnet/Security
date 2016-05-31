// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Base class for authorization handlers that need to be called for a specific requirement type.
    /// </summary>
    /// <typeparam name="TRequirement">The type of the requirement to handle.</typeparam>
    public abstract class AuthorizationHandler<TRequirement> : IAuthorizationHandler
            where TRequirement : IAuthorizationRequirement
    {
        async Task IAuthorizationHandler.HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var req in context.Requirements.OfType<TRequirement>())
            {
                await HandleAsync(context, req);
            }
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected virtual void Handle(AuthorizationHandlerContext context, TRequirement requirement)
        {
            return;
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected virtual Task HandleAsync(AuthorizationHandlerContext context, TRequirement requirement)
        {
            Handle(context, requirement);
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Base class for authorization handlers that need to be called for specific requirement and
    /// resource types.
    /// </summary>
    /// <typeparam name="TRequirement">The type of the requirement to evaluate.</typeparam>
    /// <typeparam name="TResource">The type of the resource to evaluate.</typeparam>
    public abstract class AuthorizationHandler<TRequirement, TResource> : IAuthorizationHandler
        where TRequirement : IAuthorizationRequirement
    {
        async Task IAuthorizationHandler.HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.Resource is TResource)
            {
                foreach (var req in context.Requirements.OfType<TRequirement>())
                {
                    await HandleAsync(context, req, (TResource)context.Resource);
                }
            }
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement and resource.
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        /// <param name="resource">The resource to evaluate.</param>
        protected virtual void Handle(AuthorizationHandlerContext context, TRequirement requirement, TResource resource)
        {
            return;
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement and resource.
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        /// <param name="resource">The resource to evaluate.</param>
        protected virtual Task HandleAsync(AuthorizationHandlerContext context, TRequirement requirement, TResource resource)
        {
            Handle(context, requirement, resource);
            return Task.FromResult(0);
        }
    }
}