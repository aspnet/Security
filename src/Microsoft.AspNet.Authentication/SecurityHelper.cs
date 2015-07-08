// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Helper code used when implementing authentication middleware
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// Add all ClaimsIdenities from an additional ClaimPrincipal to the ClaimsPrincipal
        /// Merges a new claims principal, placing all new identities first, and eliminating
        /// any empty unauthenticated identities from context.User
        /// </summary>
        /// <param name="identity"></param>
        public static ClaimsPrincipal MergeUserPrincipal([NotNull] ClaimsPrincipal existingPrincipal, [NotNull] ClaimsPrincipal additionalPrincipal)
        {
            return Microsoft.Framework.Internal.SecurityHelper.MergeUserPrincipal(existingPrincipal, additionalPrincipal);
        }
    }
}
