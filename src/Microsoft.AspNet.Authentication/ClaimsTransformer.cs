// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Authentication
{
    public class ClaimsTransformer : IClaimsTransformer
    {
        public Func<ClaimsPrincipal, Task<ClaimsPrincipal>> TransformDelegate { get; set; }

        public virtual Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (TransformDelegate != null)
            {
                return TransformDelegate(principal);
            }
            return Task.FromResult(principal);
        }
    }
}
