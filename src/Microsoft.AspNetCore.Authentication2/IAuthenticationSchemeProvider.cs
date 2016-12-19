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

        // Maybe can remove this
        //Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync();

        /// <summary>
        /// Returns the schemes in priority order for request handling.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AuthenticationScheme>> GetPriorityOrderedSchemes();
    }
}