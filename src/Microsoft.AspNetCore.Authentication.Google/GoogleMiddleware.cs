// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Google
{
    /// <summary>
    /// An ASP.NET middleware for authenticating users using Google OAuth 2.0.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Middleware are not disposable.")]
    public class GoogleMiddleware : OAuthMiddleware<GoogleOptions>
    {
        /// <summary>
        /// Initializes a new <see cref="GoogleMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the HTTP pipeline to invoke.</param>
        /// <param name="serviceProvider"></param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="encoder"></param>
        /// <param name="sharedOptions"></param>
        /// <param name="options">Configuration options for the middleware.</param>
        public GoogleMiddleware(
            RequestDelegate next,
            IServiceProvider serviceProvider,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            IOptions<SharedAuthenticationOptions> sharedOptions,
            IOptions<GoogleOptions> options)
            : base(next, serviceProvider, dataProtectionProvider, loggerFactory, encoder, sharedOptions, options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (sharedOptions == null)
            {
                throw new ArgumentNullException(nameof(sharedOptions));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="GoogleOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<GoogleOptions> CreateHandler()
        {
            return new GoogleHandler(Backchannel);
        }
    }
}