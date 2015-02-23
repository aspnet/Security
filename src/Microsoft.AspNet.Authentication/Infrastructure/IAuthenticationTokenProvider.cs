// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Threading.Tasks;

namespace Microsoft.AspNet.Authentication.Infrastructure
{
    public interface IAuthenticationTokenProvider
    {
        void Create(AuthenticationTokenCreateContext context);
        Task CreateAsync(AuthenticationTokenCreateContext context);
        void Receive(AuthenticationTokenReceiveContext context);
        Task ReceiveAsync(AuthenticationTokenReceiveContext context);
    }
}
