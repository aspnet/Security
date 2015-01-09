// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    // This guy does the work, default implementation will verify requirements from context.User
    public interface IAuthorizationPolicyHandler
    {
        // REVIEW: should this be void and just manipulate Authorized on context instead?
        Task<bool> AuthorizeAsync(AuthorizationContext context);
    }
}
