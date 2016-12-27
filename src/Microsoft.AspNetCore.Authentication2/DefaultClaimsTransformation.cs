// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication2
{
    public class DefaultClaimsTransformation : IClaimsTransformation
    {
        private readonly AuthenticationOptions2 _options;

        public DefaultClaimsTransformation(IOptions<AuthenticationOptions2> options)
        {
            _options = options.Value;
        }

        public virtual Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (_options.ClaimsTransform != null)
            {
                return _options.ClaimsTransform(principal);
            }
            return Task.FromResult(principal);
        }
    }
}
