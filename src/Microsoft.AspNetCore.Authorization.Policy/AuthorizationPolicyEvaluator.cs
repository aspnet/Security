// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Authorization
{
    public class AuthorizationPolicyEvaluator : IAuthorizationPolicyEvaluator
    {
        private readonly IAuthorizationService _authorization;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authorization">The authorization service.</param>
        public AuthorizationPolicyEvaluator(IAuthorizationService authorization)
        {
            _authorization = authorization;
        }

        public virtual async Task<ClaimsPrincipal> AuthenticateUser(HttpContext context, AuthorizationPolicy policy)
        {
            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                ClaimsPrincipal newPrincipal = null;
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    var result = await context.AuthenticateAsync(scheme);
                    if (result != null && result.Succeeded)
                    {
                        newPrincipal = SecurityHelper.MergeUserPrincipal(newPrincipal, result.Principal);
                    }
                }

                if (newPrincipal == null)
                {
                    newPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                }

                return newPrincipal;
            }
            return null;
        }

        public virtual async Task<AuthorizationPolicyResult> AuthorizeAsync(HttpContext context, AuthorizationPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            var authResult = await _authorization.AuthorizeAsync(context.User, context, policy);
            if (authResult.Succeeded)
            {
                return AuthorizationPolicyResult.Success();
            }

            // Challenge always (until we have some way to determine forbidden)
            return AuthorizationPolicyResult.Challenge();
        }
    }

    //// AuthorizeFilter becomes:
    //public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    //{
    //    var effectivePolicy = Policy;
    //    if (effectivePolicy == null)
    //    {
    //        effectivePolicy = await AuthorizationPolicy.CombineAsync(PolicyProvider, AuthorizeData);
    //    }

    //    if (effectivePolicy == null)
    //    {
    //        return;
    //    }

    //    var newPrincipal = await PolicyEvaluator.AuthenticateUser(context.HttpContext, effectivePolicy);
    //    context.HttpContext.User = newPrincipal ?? context.User;

    //    // Allow Anonymous skips all authorization
    //    if (context.Filters.Any(item => item is IAllowAnonymousFilter))
    //    {
    //        return;
    //    }

    //    var authResult = await PolicyEvaluator.AuthorizeAsync(context.HttpContext, effectivePolicy);
    //    if (authResult.Forbidden)
    //    {
    //        context.Result = new ForbiddenResult(effectivePolicy.AuthenticationSchemes.ToArray());
    //    }
    //    else if (authResult.Challenged)
    //    {
    //        context.Result = new ChallengeResult(effectivePolicy.AuthenticationSchemes.ToArray());
    //    }
    //}

    public class AuthorizationPolicyResult
    {
        private AuthorizationPolicyResult() { }

        /// <summary>
        /// If true, means the callee should challenge and try again.
        /// </summary>
        public bool Challenged { get; private set; }

        /// <summary>
        /// Authorization was forbidden.
        /// </summary>
        public bool Forbidden { get; private set; }

        /// <summary>
        /// Authorization was successful.
        /// </summary>
        public bool Succeeded { get; private set; }

        public static AuthorizationPolicyResult Challenge()
            => new AuthorizationPolicyResult { Challenged = true };

        public static AuthorizationPolicyResult Forbid()
            => new AuthorizationPolicyResult { Forbidden = true };

        public static AuthorizationPolicyResult Success()
            => new AuthorizationPolicyResult { Succeeded = true };

    }
}