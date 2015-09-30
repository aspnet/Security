// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.WebUtilities;

namespace Microsoft.AspNet.Authentication
{
    public class RemoteAuthenticationOptions : AuthenticationOptions
    {
        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Twitter.
        /// </summary>
        /// <value>
        /// The back channel timeout.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// The HttpMessageHandler used to communicate with Twitter.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// The request path within the application's base path where the user-agent will be returned.
        /// The middleware will process this request when it arrives.
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the authentication scheme corresponding to the middleware
        /// responsible of persisting user's identity after a successful authentication.
        /// This value typically corresponds to a cookie middleware registered in the Startup class.
        /// When omitted, <see cref="SharedAuthenticationOptions.SignInScheme"/> is used as a fallback value.
        /// </summary>
        public string SignInScheme { get; set; }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string DisplayName
        {
            get { return Description.DisplayName; }
            set { Description.DisplayName = value; }
        }

        public Func<SigningInContext, Task> TicketReceived { get; set; } = context => Task.FromResult(0);
    }

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
            if (authResult?.Error != null)
            {
                return await HandleErrorAsync(authResult.Error);
            }
            var ticket = authResult?.Ticket;
            if (ticket == null)
            {
                return await HandleErrorAsync(new ErrorContext(Context, "Invalid return state, unable to redirect."));
            }

            var context = new SigningInContext(Context, ticket)
            {
                SignInScheme = Options.SignInScheme,
                RedirectUri = ticket.Properties.RedirectUri,
            };
            ticket.Properties.RedirectUri = null;

            await Options.TicketReceived(context);

            if (!context.IsRequestCompleted && context.SignInScheme != null && context.Principal != null)
            {
                var signInContext = new SignInContext(context.SignInScheme, context.Principal, context.Properties?.Items);
                await Context.Authentication.SignInAsync(signInContext);
                if (signInContext.IsRequestCompleted)
                {
                    context.CompleteRequest();
                }
            }

            if (!context.IsRequestCompleted && context.RedirectUri != null)
            {
                if (context.Principal == null)
                {
                    // TODO: need to override this error behavior to redirect with query string
                    return await HandleErrorAsync(new ErrorContext(Context, "Authentication failure.")
                    {
                        ErrorHandlerUri = QueryHelpers.AddQueryString(context.RedirectUri, "error", "access_denied")
                    });
                }
                Response.Redirect(context.RedirectUri);
                context.CompleteRequest();
            }

            return context.IsRequestCompleted;
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