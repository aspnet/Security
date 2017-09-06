// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Authorization.Policy
{
    /// <summary>
    /// Defines the set of data needed to apply authorization rules to a resource.
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        /// The authentication policy to be used for this authorization.
        /// </summary>
        public string AuthenticationPolicy { get; set; }

        public object Resource { get; set; }

        /// <summary>
        /// Requirements that must be met for authorization to succeed.
        /// </summary>
        public IEnumerable<IAuthorizationRequirement> Requirements { get; set; }
    }
}
