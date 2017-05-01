// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Base class for authorization handlers that need to be called for a specific requirement type.
    /// </summary>
    public interface IAuthorizationPolicyEvaluator
    {
        Task<ClaimsPrincipal> AuthenticateUser(HttpContext context, AuthorizationPolicy policy);
        Task<AuthorizationPolicyResult> AuthorizeAsync(HttpContext context, AuthorizationPolicy policy);
    }
}