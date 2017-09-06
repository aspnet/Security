// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authorization.Policy
{
    public class AuthorizationRequestEvaluator : IAuthorizationRequestEvaluator
    {
        private readonly IAuthorizationService _authorization;
        private readonly IAuthenticationPolicyEvaluator _authentication;

        public AuthorizationRequestEvaluator(IAuthorizationService authorization, IAuthenticationPolicyEvaluator auth)
        {
            _authorization = authorization;
            _authentication = auth;
        }

        /// <summary>
        /// Attempts authorization using a given authentication and authorization policy.
        /// </summary>
        /// <param name="request">The <see cref="AuthorizationRequest"/>.</param>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>Returns <see cref="PolicyAuthorizationResult.Success"/> if authorization succeeds.
        /// Otherwise returns <see cref="PolicyAuthorizationResult.Forbid"/> if <see cref="AuthenticateResult.Succeeded"/>, otherwise
        /// returns  <see cref="PolicyAuthorizationResult.Challenge"/></returns>
        public virtual async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationRequest request, HttpContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // REVIEW: is it reasonable to always use context.User by default
            ClaimsPrincipal principal = context.User;
            var authenticateSuccess = false;
            if (request.AuthenticationPolicy != null)
            {
                var authenticateResult = await _authentication.AuthenticateAsync(context, request.AuthenticationPolicy);
                if (authenticateResult.Succeeded)
                {
                    principal = authenticateResult.Principal;
                    authenticateSuccess = true;
                }
            }
            else
            {
                authenticateSuccess = true;
            }

            var policy = new AuthorizationPolicyBuilder();
            foreach (var req in request.Requirements)
            {
                policy.Requirements.Add(req);
            }

            var result = await _authorization.AuthorizeAsync(principal, request.Resource, policy.Build());
            if (result.Succeeded)
            {
                return PolicyAuthorizationResult.Success();
            }

            // If authentication was successful, return forbidden, otherwise challenge
            return (authenticateSuccess) 
                ? PolicyAuthorizationResult.Forbid() 
                : PolicyAuthorizationResult.Challenge();
        }
    }
}