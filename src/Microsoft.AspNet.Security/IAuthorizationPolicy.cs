// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Security
{
    // Consider merging IAuthorizationPolicy with IAuthorizationPolicyHandler?

    public interface IAuthorizationPolicy
    {
        // Auth types requested by this policy
        IEnumerable<string> AuthenticationTypes { get; }

        IEnumerable<AuthorizationClaimRequirement> Requirements { get; }

        IEnumerable<IAuthorizationPolicyHandler> Handlers { get; }
    }

    // Must contain a claim with the specified name, and at least one of the required values
    // If ValueRequirement is null or empty, that means any claim is valid
    public class AuthorizationClaimRequirement
    {
        public string ClaimType { get; set; }
        public IEnumerable<string> ClaimValueRequirement { get; set; }
    }
}
