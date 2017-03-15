// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    public interface IAuthenticationSchemeProvider
    {
        Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync();
        Task<AuthenticationScheme> GetSchemeAsync(string name);
        Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync();
        Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync();
        Task<AuthenticationScheme> GetDefaultSignInSchemeAsync();
        void AddScheme(AuthenticationScheme scheme);
        void RemoveScheme(string name);

        /// <summary>
        /// Returns the schemes in priority order for request handling.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync(PathString requestPath);
    }
}