// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Resolves the proper IAuthenticationHandler instance for the authenticationScheme and request.
    /// </summary>
    public interface IAuthenticationHandlerResolver
    {
        Task<IAuthenticationHandler> ResolveHandlerAsync(HttpContext context, string authenticationScheme);
    }
}