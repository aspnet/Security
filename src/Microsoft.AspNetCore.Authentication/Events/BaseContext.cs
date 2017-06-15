// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Base class used by other context classes.
    /// </summary>
    public abstract class BaseContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        private AuthenticationProperties _properties;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scheme">The authentication scheme.</param>
        /// <param name="options">The authentication options associated with the scheme.</param>
        protected BaseContext(HttpContext context, AuthenticationScheme scheme, TOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (scheme == null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            HttpContext = context;
            Scheme = scheme;
            Options = options;
        }

        /// <summary>
        /// The authentication scheme.
        /// </summary>
        public AuthenticationScheme Scheme { get; }

        /// <summary>
        /// Gets the authentication options associated with the scheme.
        /// </summary>
        public TOptions Options { get; }

        /// <summary>
        /// The context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// The request.
        /// </summary>
        public HttpRequest Request => HttpContext.Request;

        /// <summary>
        /// The response.
        /// </summary>
        public HttpResponse Response => HttpContext.Response;

        /// <summary>
        /// Gets or sets the <see cref="AuthenticationProperties"/>.
        /// </summary>
        public virtual AuthenticationProperties Properties
        {
            get => _properties ?? (_properties = new AuthenticationProperties());
            set => _properties = value;
        }

        public EventResultState State { get; protected set; }
    }
}
