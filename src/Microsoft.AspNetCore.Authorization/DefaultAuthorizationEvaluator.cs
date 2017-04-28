// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Determines whether an authorization request was successful or not.
    /// </summary>
    public class DefaultAuthorizationEvaluator : IAuthorizationEvaluator
    {
        /// <summary>
        /// Determines whether the authorization result was successful or not.
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <returns>The <see cref="AuthorizeResult"/>.</returns>
        public AuthorizeResult Evaluate(AuthorizationHandlerContext context)
            => context.HasSucceeded
                ? AuthorizeResult.Success()
                : AuthorizeResult.Failed(context.HasFailed
                    ? AuthorizeFailure.ExplicitFail()
                    : AuthorizeFailure.Failed(context.PendingRequirements));
    }
}
