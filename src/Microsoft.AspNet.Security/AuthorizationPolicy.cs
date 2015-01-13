// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license ainformation.

using System.Collections.Generic;

namespace Microsoft.AspNet.Security
{
    public class AuthorizationPolicy
    {
        public AuthorizationPolicy(IEnumerable<IAuthorizationRequirement> requirements, IEnumerable<string> useOnlyTheseAuthenticationTypes)
        {
            Requirements = requirements;
            UseOnlyTheseAuthenticationTypes = useOnlyTheseAuthenticationTypes;
        }

        public IEnumerable<IAuthorizationRequirement> Requirements { get; private set; }
        public IEnumerable<string> UseOnlyTheseAuthenticationTypes { get; private set; }
    }
}
