// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features.Authentication;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Handler that applies ClaimsTransformation to authentication
    /// </summary>
    public class ClaimsTransformationAuthenticationHandler : IAuthenticationHandler
    {
        private readonly IClaimsTransformer _transform;

        public ClaimsTransformationAuthenticationHandler(IClaimsTransformer transform)
        {
            _transform = transform;
        }

        public IAuthenticationHandler PriorHandler { get; set; }

        public void Authenticate(AuthenticateContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.Authenticate(context);
                if (_transform != null && context?.Principal != null)
                {
                    context.Authenticated(
                        _transform.Transform(context.Principal),
                        context.Properties,
                        context.Description);
                }
            }
        }

        public async Task AuthenticateAsync(AuthenticateContext context)
        {
            if (PriorHandler != null)
            {
                await PriorHandler.AuthenticateAsync(context);
                if (_transform != null && context?.Principal != null)
                {
                    context.Authenticated(
                        await _transform.TransformAsync(context.Principal),
                        context.Properties,
                        context.Description);
                }
            }
        }

        public void Challenge(ChallengeContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.Challenge(context);
            }
        }

        public void GetDescriptions(DescribeSchemesContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.GetDescriptions(context);
            }
        }

        public void SignIn(SignInContext context)
        {
            if (PriorHandler != null)
            {
                PriorHandler.SignIn(context);
            }
        }

        public void SignOut(SignOutContext context)
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
