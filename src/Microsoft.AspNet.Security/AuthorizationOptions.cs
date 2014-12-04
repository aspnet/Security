// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Security
{
    // TODO: make policies read only
    public class AuthorizationOptions
    {
        private IDictionary<string, IAuthorizationPolicy> PolicyMap { get; } = new Dictionary<string, IAuthorizationPolicy>();

        public void AddPolicy([NotNull] string name, [NotNull] IAuthorizationPolicy policy)
        {
            PolicyMap[name] = policy;
        }

        public IAuthorizationPolicy GetPolicy([NotNull] string name)
        {
            return PolicyMap.ContainsKey(name) ? PolicyMap[name] : null;
        }
    }
}