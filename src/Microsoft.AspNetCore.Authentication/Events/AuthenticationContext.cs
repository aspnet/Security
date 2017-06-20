// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Base context for events that produce AuthenticateResults.
    /// </summary>
    public abstract class AuthenticationContext<TOptions> : BaseContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scheme">The authentication scheme.</param>
        /// <param name="options">The authentication options associated with the scheme.</param>
        protected AuthenticationContext(HttpContext context, AuthenticationScheme scheme, TOptions options)
            : base(context, scheme, options) { }

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
        public AuthenticationProperties Properties
        {
            get => Ticket?.Properties;

            set
            {
                if (Ticket != null)
                {
                    Ticket = new AuthenticationTicket(Principal, value, Scheme.Name);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="AuthenticateResult"/> result.
        /// </summary>
        public AuthenticateResult Result { get; private set; }

        public void Success(AuthenticationTicket ticket) => Result = AuthenticateResult.Success(ticket);

        /// <summary>
        /// Indicates that there was no information returned for this authentication scheme.
        /// </summary>
        public void Ignore() => Result = AuthenticateResult.Ignore();

        /// <summary>
        /// Indicates that there was a failure during authentication.
        /// </summary>
        /// <param name="failure"></param>
        public void Fail(Exception failure) => Result = AuthenticateResult.Fail(failure);

        /// <summary>
        /// Indicates that there was a failure during authentication.
        /// </summary>
        /// <param name="failureMessage"></param>
        public void Fail(string failureMessage) => Result = AuthenticateResult.Fail(failureMessage);
    }
}
