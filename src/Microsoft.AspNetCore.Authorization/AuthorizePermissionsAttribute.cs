// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization
{
    public class AuthorizationPermissionsRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePermissionsAttribute"/> class with the specified policy. 
        /// </summary>
        /// <param name="permissions">The permissions to require for authorization.</param>
        public AuthorizationPermissionsRequirement(params Enum[] permissions)
        {
            RequiredPermissions = permissions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePermissionsAttribute"/> class with the specified policy. 
        /// </summary>
        /// <param name="permissions">The permissions to require for authorization.</param>
        public AuthorizationPermissionsRequirement(IEnumerable<Enum> permissions)
        {
            RequiredPermissions = permissions;
        }

        /// <summary>
        /// The authentication schemes to challenge if authorization fails.
        /// </summary>
        public IEnumerable<Enum> RequiredPermissions { get; }
    }

    public abstract class AuthorizationPermissionsHandler : AuthorizationHandler<AuthorizationPermissionsRequirement>
    {
        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationPermissionsRequirement requirement)
        {
            if (await CheckPermissionsAsync(context, requirement.RequiredPermissions))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Returns whether authorization is successful for all permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="permissions">The required permissions.</param>
        /// <returns>Whether authorization is successful for all permissions.</returns>
        protected virtual async Task<bool> CheckPermissionsAsync(AuthorizationHandlerContext context, IEnumerable<Enum> permissions)
        {
            foreach (var permission in permissions)
            {
                if (!await CheckPermissionAsync(context, permission))
                {
                    return false;
                }
            }
            return true;
        }

        protected abstract Task<bool> CheckPermissionAsync(AuthorizationHandlerContext context, Enum permission);
    }

    public interface IAuthorizePermissionsData
    {
        /// <summary>
        /// The required permissions for authorization to be successful.
        /// </summary>
        IEnumerable<Enum> RequiredPermissions { get; }

        /// <summary>
        /// The authentication schemes to challenge if authorization fails.
        /// </summary>
        string AuthenticationSchemes { get; set; }
    }

    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires the specified authorization permissions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizePermissionsAttribute : Attribute, IAuthorizePermissionsData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePermissionsAttribute"/> class with the specified policy. 
        /// </summary>
        /// <param name="permissions">The permissions to require for authorization.</param>
        public AuthorizePermissionsAttribute(params Enum[] permissions)
        {
            RequiredPermissions = permissions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePermissionsAttribute"/> class with the specified policy. 
        /// </summary>
        /// <param name="permissions">The permissions to require for authorization.</param>
        public AuthorizePermissionsAttribute(IEnumerable<Enum> permissions)
        {
            RequiredPermissions = permissions;
        }

        public IEnumerable<Enum> RequiredPermissions { get; }

        /// <summary>
        /// The authentication schemes to challenge if authorization fails.
        /// </summary>
        public string AuthenticationSchemes { get; set; }
    }
}
