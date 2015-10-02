// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.WebUtilities;

namespace Microsoft.AspNet.Authentication
{
    public abstract class RemoteAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions> where TOptions : RemoteAuthenticationOptions
    {
        /// <summary>
        /// Called during initialize to authenticate implicitly.
        /// </summary>
        /// <returns>True if request processing should continue</returns>
        protected override async Task HandleErrorAsync(ErrorContext context)
        {
            context.ErrorHandlerUri = context.ErrorHandlerUri ?? Options.ErrorHandlerPath;
            await Options.RemoteEvents.Error(context);
        }

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
            if (authResult?.Error != null)
            {
                await HandleErrorAsync(authResult.Error);
                return authResult.Error.IsRequestComplete;
            }
            var ticket = authResult?.Ticket;
            if (ticket == null)
            {
                var error = new ErrorContext(Context, "Invalid return state, unable to redirect.");
                await Options.RemoteEvents.Error(error);
                return error.IsRequestComplete;
            }

            var context = new SigningInContext(Context, ticket)
            {
                SignInScheme = Options.SignInScheme,
                RedirectUri = ticket.Properties.RedirectUri,
            };
            ticket.Properties.RedirectUri = null;

            await Options.RemoteEvents.SigningIn(context);

            if (!context.IsRequestComplete && context.SignInScheme != null && context.Principal != null)
            {
                var signInContext = new SignInContext(context.SignInScheme, context.Principal, context.Properties?.Items);
                await Context.Authentication.SignInAsync(signInContext);
                if (signInContext.IsRequestCompleted)
                {
                    context.CompleteRequest();
                }
            }

            if (!context.IsRequestComplete && context.RedirectUri != null)
            {
                if (context.Principal == null)
                {
                    // TODO: need to override this error behavior to redirect with query string

                    var error = new ErrorContext(Context, "Authentication failure.")
                    {
                        ErrorHandlerUri = QueryHelpers.AddQueryString(context.RedirectUri, "error", "access_denied")
                    };
                    await Options.RemoteEvents.Error(error);
                    return error.IsRequestComplete;
                }
                Response.Redirect(context.RedirectUri);
                context.CompleteRequest();
            }

            return context.IsRequestComplete;
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