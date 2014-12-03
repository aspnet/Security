// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Security
{
    public interface IAuthorizationPolicy
    {
        // Auth types requested by this policy
        IEnumerable<string> AuthenticationTypes { get; }

        IEnumerable<AuthorizationClaimRequirement> Requirements { get; }
    }

    // Must contain a claim with the specified name, and at least one of the required values
    public class AuthorizationClaimRequirement
    {
        public string ClaimType { get; set; }
        public IEnumerable<string> ClaimValueRequirement { get; set; }
    }
}
