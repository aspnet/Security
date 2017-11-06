// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// AuthenticationPolicy assigns a set of schemes for all of the authentication actions.
    /// </summary>
    public class AuthenticationPolicy
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="authenticateSchemes"></param>
        /// <param name="challengeSchemes"></param>
        /// <param name="forbidSchemes"></param>
        public AuthenticationPolicy(IEnumerable<string> authenticateSchemes,
            IEnumerable<string> challengeSchemes,
            IEnumerable<string> forbidSchemes)
        {
            AuthenticateSchemes = authenticateSchemes ?? new string[0];
            ChallengeSchemes = challengeSchemes ?? new string[0];
            ForbidSchemes = forbidSchemes ?? new string[0];
        }

        /// <summary>
        /// The authentication schemes that should be used to construct the ClaimsPrincipal during authentication.
        /// </summary>
        public IEnumerable<string> AuthenticateSchemes { get; }

        /// <summary>
        /// The authentication schemes that should be challenged. 
        /// </summary>
        public IEnumerable<string> ChallengeSchemes { get; }

        /// <summary>
        /// The authentication schemes that should be forbidden.
        /// </summary>
        public IEnumerable<string> ForbidSchemes { get; }

        // REVIEW: Do we really need SignIn/SignOut schemes?
    }
}
