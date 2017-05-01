// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Authorization
{
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