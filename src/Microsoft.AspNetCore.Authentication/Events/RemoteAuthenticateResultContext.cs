// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Base context for authentication.
    /// </summary>
    public abstract class RemoteAuthenticateResultContext<TOptions> : HandleRequestContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scheme">The authentication scheme.</param>
        /// <param name="options">The authentication options associated with the scheme.</param>
        protected RemoteAuthenticateResultContext(
            HttpContext context,
            AuthenticationScheme scheme,
            TOptions options)
            : base(context, scheme, options)
        {
        }

        /// <summary>
        /// Gets or set the <see cref="AuthenticationTicket"/> containing
        /// the user principal and the authentication properties.
        /// </summary>
        public AuthenticationTicket Ticket { get; set; }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> containing the user claims.
        /// </summary>
        public ClaimsPrincipal Principal => Ticket?.Principal;

        /// <summary>
        /// Gets or sets the <see cref="AuthenticationProperties"/>.
        /// </summary>
        public override AuthenticationProperties Properties
        {
            get => Ticket?.Properties ?? base.Properties;

            set
            {
                if (Ticket != null)
                {
                    Ticket = new AuthenticationTicket(Principal, value, Scheme.Name);
                }

                base.Properties = value;
            }
        }

        public Exception Failure { get; private set; }

        public void CompleteAuthentication(AuthenticationTicket ticket)
        {
            State = EventResultState.BypassDefaultLogic;
            Ticket = ticket;
        }

        public void SkipAuthentication()
        {
            State = EventResultState.BypassDefaultLogic;
        }

        public void RejectAuthentication(Exception failure)
        {
            State = EventResultState.BypassDefaultLogic;
            Failure = failure;
        }

        public void RejectAuthentication(string failureMessage)
        {
            State = EventResultState.BypassDefaultLogic;
            Failure = new Exception(failureMessage);
        }

        public bool IsProcessingComplete(out RemoteAuthenticationResult result)
        {
            if (State == EventResultState.HandleResponse)
            {
                result = RemoteAuthenticationResult.Handle();
                return true;
            }
            else if (State == EventResultState.SkipToNextMiddleware)
            {
                result = RemoteAuthenticationResult.Skip();
                return true;
            }
            else if (State == EventResultState.BypassDefaultLogic)
            {
                if (Ticket != null)
                {
                    result = RemoteAuthenticationResult.Success(Ticket);
                }
                else if (Failure != null)
                {
                    result = RemoteAuthenticationResult.Fail(Failure);
                }
                else
                {
                    result = RemoteAuthenticationResult.None();
                }
                return true;
            }
            result = null;
            return false;
        }
    }
}