// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Interfaces.Authentication;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Base class for the per-request work performed by automatic authentication middleware.
    /// </summary>
    /// <typeparam name="TOptions">Specifies which type for of AutomaticAuthenticationOptions property</typeparam>
    public abstract class AutomaticAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions> where TOptions : AutomaticAuthenticationOptions
    {
        public virtual bool ShouldConvertChallengeToForbidden()
        {
            // Return 403 iff 401 and this handler's authenticate was called
            // and the challenge is for the authentication type
            return Response.StatusCode == 401 &&
                AuthenticateCalled &&
                ChallengeContext != null &&
                ShouldHandleChallenge(ChallengeContext.AuthenticationSchemes);
        }

        protected async override Task InitializeCoreAsync()
        {
            if (Options.AutomaticAuthentication)
            {
                AuthenticationTicket ticket = await AuthenticateAsync();
                if (ticket != null && ticket.Principal != null)
                {
                    SecurityHelper.AddUserPrincipal(Context, ticket.Principal);
                }
            }
        }

        public override void SignOut(ISignOutContext context)
        {
            // Empty or null auth scheme is allowed for automatic Authentication
            if (Options.AutomaticAuthentication && string.IsNullOrWhiteSpace(context.AuthenticationScheme))
            {
                SignInContext = null;
                SignOutContext = context;
                context.Accept();
            }

            base.SignOut(context);
        }

        public override void Challenge(IChallengeContext context)
        {
            // Null or Empty scheme allowed for automatic authentication
            if (Options.AutomaticAuthentication && 
                (context.AuthenticationSchemes == null || !context.AuthenticationSchemes.Any()))
            {
                ChallengeContext = context;
                context.Accept(BaseOptions.AuthenticationScheme, BaseOptions.Description.Dictionary);
            }

            base.Challenge(context);
        }

        /// <summary>
        /// Automatic Authentication Handlers can handle empty authentication schemes
        /// </summary>
        /// <returns></returns>
        public override bool ShouldHandleChallenge(IEnumerable<string> authenticationSchemes)
        {
            if (base.ShouldHandleChallenge(authenticationSchemes))
            {
                return true;
            }

            return Options.AutomaticAuthentication &&
                (authenticationSchemes == null || !authenticationSchemes.Any());
        }

        /// <summary>
        /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <returns></returns>
        protected override Task ApplyResponseChallengeAsync()
        {
            // If authenticate was called and the the status is still 401, authZ failed so set 403 and stop
            if (ShouldConvertChallengeToForbidden())
            {
                Response.StatusCode = 403;
                return Task.FromResult(0);
            }
            return base.ApplyResponseChallengeAsync();
        }

    }
}