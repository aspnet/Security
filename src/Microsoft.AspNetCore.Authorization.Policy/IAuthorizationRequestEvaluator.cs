// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authorization.Policy
{
    public interface IAuthorizationRequestEvaluator
    {
        /// <summary>
        /// Attempts authorization.
        /// </summary>
        /// <param name="authorization">The <see cref="AuthorizationRequest"/>.</param>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>Returns <see cref="PolicyAuthorizationResult.Success"/> if authorization succeeds.
        /// Otherwise returns <see cref="PolicyAuthorizationResult.Forbid"/> if <see cref="AuthenticateResult.Succeeded"/>, otherwise
        /// returns  <see cref="PolicyAuthorizationResult.Challenge"/></returns>
        Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationRequest authorization, HttpContext context);
    }
}