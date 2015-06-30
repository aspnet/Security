// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OpenIdConnect;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    ///  Allows for custom processing of ApplyResponseChallenge, ApplyResponseGrant and AuthenticateCore
    /// </summary>
    public class OpenIdConnectAuthenticationHandlerForTestingAuthenticate : OpenIdConnectAuthenticationHandler
    {
        private Func<Task> _applyResponseChallenge;
        private Func<Task> _applyResponseGrant;
        private Func<Task<AuthenticationTicket>> _authenticationCore;

        public OpenIdConnectAuthenticationHandlerForTestingAuthenticate(Func<Task> applyResponseChallenge = null, Func<Task> applyResponseGrant = null, Func<Task<AuthenticationTicket>> authenticationCore = null )
                    : base()
        {
            _applyResponseChallenge = applyResponseChallenge;
            _applyResponseGrant = applyResponseGrant;
            _authenticationCore = authenticationCore;
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            if (_applyResponseChallenge != null)
                await _applyResponseChallenge();
            else
                await base.ApplyResponseChallengeAsync();
        }

        protected override async Task ApplyResponseGrantAsync()
        {
            if (_applyResponseGrant != null)
                await _applyResponseGrant();
            else
                await base.ApplyResponseGrantAsync();
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            if (_authenticationCore != null)
                return await _authenticationCore();
            else
                return await base.AuthenticateCoreAsync();
        }

        public override bool ShouldHandleScheme(string authenticationScheme)
        {
            return true;
        }
    }
}
