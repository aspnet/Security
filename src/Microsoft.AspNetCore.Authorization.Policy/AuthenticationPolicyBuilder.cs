// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Used to build <see cref="AuthenticationPolicy"/>s.
    /// </summary>
    public class AuthenticationPolicyBuilder
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the policy being built.</param>
        public AuthenticationPolicyBuilder(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the Policy being built.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The default scheme that will be used if none are specified.
        /// </summary>
        public string DefaultScheme { get; set; }

        /// <summary>
        /// The list of schemes that will be used for authentication.
        /// </summary>
        public IList<string> AuthenticateSchemes { get; set; } = new List<string>();

        /// <summary>
        /// The list of schemes that will be used for challenges.
        /// </summary>
        public IList<string> ChallengeSchemes { get; set; } = new List<string>();

        /// <summary>
        /// The list of schemes that will be used for forbids.
        /// </summary>
        public IList<string> ForbidSchemes { get; set; } = new List<string>();

        private IEnumerable<string> ResolveDefault(IList<string> schemes)
        {
            if (schemes.Count == 0 && DefaultScheme != null)
            {
                return new string[1] { DefaultScheme };
            }
            return schemes;
        }

        /// <summary>
        /// Builds the <see cref="AuthenticationPolicy"/> instance.
        /// </summary>
        /// <returns></returns>
        public AuthenticationPolicy Build()
            => new AuthenticationPolicy(
                ResolveDefault(AuthenticateSchemes),
                ResolveDefault(ChallengeSchemes),
                ResolveDefault(ForbidSchemes));
    }
}
