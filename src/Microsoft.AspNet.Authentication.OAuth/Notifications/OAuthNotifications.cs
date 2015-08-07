// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Authentication.OAuth
{
    /// <summary>
    /// Default <see cref="IOAuthNotifications"/> implementation.
    /// </summary>
    public class OAuthNotifications : IOAuthNotifications
    {
        /// <summary>
        /// Gets or sets the function that is invoked when the AccessTokenReceived method is invoked.
        /// </summary>
        public Func<OAuthAccessTokenReceivedContext, Task> OnAccessTokenReceived { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Gets or sets the function that is invoked when the ReturnEndpoint method is invoked.
        /// </summary>
        public Func<OAuthReturnEndpointContext, Task> OnReturnEndpoint { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Gets or sets the delegate that is invoked when the ApplyRedirect method is invoked.
        /// </summary>
        public Action<OAuthApplyRedirectContext> OnApplyRedirect { get; set; } = context => context.Response.Redirect(context.RedirectUri);

        /// <summary>
        /// Invoked after the provider successfully authorizes your application. This can be used to retrieve user information.
        /// This notification may not be invoked by sub-classes of OAuthAuthenticationHandler if they override CreateTicketAsync.
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task AccessTokenReceived(OAuthAccessTokenReceivedContext context) => OnAccessTokenReceived(context);

        /// <summary>
        /// Invoked prior to the <see cref="ClaimsIdentity"/> being saved in a local cookie and the browser being redirected to the originally requested URL.
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="ClaimsIdentity"/></param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task ReturnEndpoint(OAuthReturnEndpointContext context) => OnReturnEndpoint(context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the OAuth middleware.
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge.</param>
        public virtual void ApplyRedirect(OAuthApplyRedirectContext context) => OnApplyRedirect(context);
    }
}
