// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features.Authentication;

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

        private void ApplyTransform(AuthenticateContext context)
        {
            if (_transform != null)
            {
                // REVIEW: this cast seems really bad (missing interface way to get the result back out?)
                var authContext = context as AuthenticateContext;
                if (authContext?.Principal != null)
                {
                    context.Authenticated(
                        _transform.Invoke(authContext.Principal),
                        authContext.Properties,
                        authContext.Description);
                }
            }

        }

        public async Task AuthenticateAsync(AuthenticateContext context)
        {
            if (PriorHandler != null)
            {
                await PriorHandler.AuthenticateAsync(context);
                ApplyTransform(context);
            }
        }

        public Task ChallengeAsync(ChallengeContext context)
        {
            if (PriorHandler != null)
            {
                return PriorHandler.ChallengeAsync(context);
            }
            return Task.FromResult(0);
        }

        public void GetDescriptions(DescribeSchemesContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.GetDescriptions(context);
            }
        }

        public Task SignInAsync(SignInContext context)
        {
            if (PriorHandler != null)
            {
                return PriorHandler.SignInAsync(context);
            }
            return Task.FromResult(0);
        }

        public Task SignOutAsync(SignOutContext context)
        {
            if (PriorHandler != null)
            {
                return PriorHandler.SignOutAsync(context);
            }
            return Task.FromResult(0);
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
