// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Core.Authentication;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Handler that applies ClaimsTransformation to authentication
    /// </summary>
    public class ClaimsTransformationAuthenticationHandler : IAuthenticationHandler
    {
        private readonly Func<ClaimsPrincipal, ClaimsPrincipal> _transform;

        public ClaimsTransformationAuthenticationHandler(Func<ClaimsPrincipal, ClaimsPrincipal> transform)
        {
            _transform = transform;
        }

        public IAuthenticationHandler PriorHandler { get; set; }

        private void ApplyTransform(IAuthenticateContext context)
        {
            if (_transform != null)
            {
                var authContext = context as AuthenticateContext;
                if (context != null)
                {
                    authContext.Result = new AuthenticationResult(
                        _transform.Invoke(authContext.Result.Principal),
                        authContext.Result.Properties,
                        authContext.Result.Description);
                }
            }

        }

        public void Authenticate(IAuthenticateContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.Authenticate(context);
                ApplyTransform(context);
            }
        }

        public async Task AuthenticateAsync(IAuthenticateContext context)
        {
            if (PriorHandler != null)
            {
                await PriorHandler.AuthenticateAsync(context);
                ApplyTransform(context);
            }
        }

        public void Challenge(IChallengeContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.Challenge(context);
            }
        }

        public void GetDescriptions(IDescribeSchemesContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.GetDescriptions(context);
            }
        }

        public void SignIn(ISignInContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.SignIn(context);
            }
        }

        public void SignOut(ISignOutContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.SignOut(context);
            }
        }

        public void RegisterAuthenticationHandler(IHttpAuthenticationFeature auth)
        {
            PriorHandler = auth.Handler;
            auth.Handler = this;
        }

        public void UnregisterAuthenticationHandler(IHttpAuthenticationFeature auth)
        {
            auth.Handler = PriorHandler;
        }

    }
}
