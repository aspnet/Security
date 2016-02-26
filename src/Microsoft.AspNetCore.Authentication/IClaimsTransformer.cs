// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Used for claims transformation.
    /// </summary>
    public interface IClaimsTransformer
    {
        /// <summary>
        /// Transform a ClaimsPrincipal.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal);
    }
}
