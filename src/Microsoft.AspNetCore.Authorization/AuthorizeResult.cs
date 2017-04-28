// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Encapsulates the result of <see cref="IAuthorizationService.AuthorizeAsync(ClaimsPrincipal, object, IEnumerable{IAuthorizationRequirement})"/>.
    /// </summary>
    public class AuthorizeResult
    {
        private AuthorizeResult() { }

        /// <summary>
        /// True if authorization was successful.
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Contains information about why authorization failed.
        /// </summary>
        public AuthorizeFailure Failure { get; private set; }

        /// <summary>
        /// Returns a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static AuthorizeResult Success() => new AuthorizeResult { Succeeded = true };

        public static AuthorizeResult Failed(AuthorizeFailure failure) => new AuthorizeResult { Failure = failure };

    }
}
