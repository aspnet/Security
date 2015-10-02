// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Authentication
{
    public class RemoteAuthenticationEvents : IRemoteAuthenticationEvents
    {

        public Func<ErrorContext, Task> OnError { get; set; } = context =>
        {
            var errorUri = context.ErrorHandlerUri;
            // Noop if no error handler path configured
            if (!string.IsNullOrEmpty(errorUri))
            {
                var redirectUri = errorUri + QueryString.Create("ErrorMessage", context.ErrorMessage);
                context.HttpContext.Response.Redirect(redirectUri);
            }
            else
            {
                context.HttpContext.Response.StatusCode = 500;
            }
            context.CompleteRequest();
            return Task.FromResult(0);
        };

        public Func<SigningInContext, Task> OnSigningIn { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Invoked when the authentication process completes.
        /// </summary>
        public virtual Task Error(ErrorContext context) => OnError(context);

        /// <summary>
        /// Invoked before sign in.
        /// </summary>
        public virtual Task SigningIn(SigningInContext context) => OnSigningIn(context);
    }
}