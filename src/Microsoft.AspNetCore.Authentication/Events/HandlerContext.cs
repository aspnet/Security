// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Base class used by other context classes.
    /// </summary>
    public abstract class HandlerContext<THandler> where THandler : IAuthenticationHandler
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler">The authentication handler.</param>
        /// <param name="context">The context.</param>
        protected HandlerContext(THandler handler, HttpContext context)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Handler = handler;
            HttpContext = context;
        }

        /// <summary>
        /// The authentication handler.
        /// </summary>
        public THandler Handler { get; set; }

        /// <summary>
        /// The context.
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// The request.
        /// </summary>
        public HttpRequest Request => HttpContext.Request;

        /// <summary>
        /// The response.
        /// </summary>
        public HttpResponse Response => HttpContext.Response;
    }
}
