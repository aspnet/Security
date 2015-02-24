// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Interfaces.Authentication;

namespace Microsoft.AspNet.Authentication.Infrastructure
{
    /// <summary>
    /// Base class for the per-request work performed by automatic authentication middleware.
    /// </summary>
    /// <typeparam name="TOptions">Specifies which type for of AutomaticAuthenticationOptions property</typeparam>
    public abstract class AutomaticAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions> where TOptions : AutomaticAuthenticationOptions
    {
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
                SignInIdentityContext = null;
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
        public override bool ShouldHandleChallenge()
        {
            if (base.ShouldHandleChallenge())
            {
                return true;
            }

            return Options.AutomaticAuthentication &&
                (ChallengeContext?.AuthenticationSchemes == null || !ChallengeContext.AuthenticationSchemes.Any());
        }
    }
}