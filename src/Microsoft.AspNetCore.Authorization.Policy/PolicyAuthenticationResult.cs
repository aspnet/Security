// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.AspNetCore.Authorization.Policy
{
    public class PolicyAuthenticationResult
    {
        private PolicyAuthenticationResult() { }

        /// <summary>
        /// Authentication from at least one scheme was successful.
        /// </summary>
        public bool Succeeded { get; private set; }

        public static PolicyAuthenticationResult Success()
            => new PolicyAuthenticationResult { Succeeded = true };

        public static PolicyAuthenticationResult Failed()
            => new PolicyAuthenticationResult();
    }
}