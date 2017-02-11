// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication2
{
    public interface IAuthenticationSchemeProvider
    {
        Task<AuthenticationScheme> GetSchemeAsync(string name);
        Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync();
        Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync();
        void AddScheme(AuthenticationScheme scheme);

        /// <summary>
        /// Returns the schemes in priority order for request handling.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AuthenticationScheme>> GetPriorityOrderedSchemesAsync();
    }
}