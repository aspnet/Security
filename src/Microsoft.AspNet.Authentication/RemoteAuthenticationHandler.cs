// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Authentication
{
    public abstract class RemoteAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions> where TOptions : RemoteAuthenticationOptions
    {
        public override async Task<bool> HandleRequestAsync()
        {
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                return await HandleRemoteCallbackAsync();
            }
            return false;
        }

        public virtual async Task<bool> HandleRemoteCallbackAsync()
        {
            var authResult = await HandleAuthenticateOnceAsync();
            var ticket = authResult?.Ticket;
            if (authResult?.Error != null || ticket == null)
            {
                ErrorContext errorContext;
                if (ticket == null || ticket.Principal == null)
                {
                    errorContext = new ErrorContext(Context, new Exception("Invalid return state, unable to redirect."));
                }
                else
                {
                    errorContext = new ErrorContext(Context, authResult.Error);
                }

                await Options.RemoteEvents.Error(errorContext);
                if (errorContext.HandledResponse)
                {
                    return true;
                }
                if (errorContext.Skipped)
                {
                    return false;
                }

                Context.Response.StatusCode = 500;
                return true;
            }

            var signingInContext = new TicketReceivedContext(Context, ticket)
            {
                SignInScheme = Options.SignInScheme,
                ReturnUri = ticket.Properties.RedirectUri,
            };
            // REVIEW: is this safe or good?
            ticket.Properties.RedirectUri = null;

            await Options.RemoteEvents.TicketReceived(signingInContext);

            if (signingInContext.HandledResponse)
            {
                Logger.LogVerbose("The SigningIn event returned Handled.");
                return true;
            }
            else if (signingInContext.Skipped)
            {
                Logger.LogVerbose("The SigningIn event returned Skipped.");
                return false;
            }

            if (signingInContext.Principal != null)
            {
                var signInContext = new SignInContext(signingInContext.SignInScheme, signingInContext.Principal, signingInContext.Properties?.Items);
                await Context.Authentication.SignInAsync(signInContext);
            }

            if (signingInContext.ReturnUri != null)
            {
                Response.Redirect(signingInContext.ReturnUri);
                return true;
            }

            return false;
        }

        protected override Task HandleSignOutAsync(SignOutContext context)
        {
            throw new NotSupportedException();
        }

        protected override Task HandleSignInAsync(SignInContext context)
        {
            throw new NotSupportedException();
        }

        protected override Task HandleForbiddenAsync(ChallengeContext context)
        {
            throw new NotSupportedException();
        }
    }
}