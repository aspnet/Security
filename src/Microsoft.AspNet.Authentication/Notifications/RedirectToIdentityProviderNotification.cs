// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace Microsoft.AspNet.Authentication.Notifications
{
    /// <summary>
    /// When a use configures the <see cref="AuthenticationMiddleware{TOptions}"/> to be notification prior to redirecting to an IdentityProvider
    /// this notification is passed to the 'RedirectToIdentityProvider".
    /// </summary>
    /// <typeparam name="TMessage">protocol specific message.</typeparam>
    /// <typeparam name="TOptions">protocol specific options.</typeparam>
    public class RedirectToIdentityProviderNotification<TMessage, TOptions> : BaseNotification<TOptions>
    {
        public RedirectToIdentityProviderNotification(HttpContext context, TOptions options) : base(context, options)
        {
        }

        public TMessage ProtocolMessage { get; set; }

        public AuthenticationProperties AuthenticationProperties { get; set; }
    }
}
