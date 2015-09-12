// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Authentication.Facebook
{
    /// <summary>
    /// An ASP.NET middleware for authenticating users using Facebook.
    /// </summary>
    public class FacebookAuthenticationMiddleware : OAuthAuthenticationMiddleware<FacebookAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new <see cref="FacebookAuthenticationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the HTTP pipeline to invoke.</param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="encoder"></param>
        /// <param name="sharedOptions"></param>
        /// <param name="options">Configuration options for the middleware.</param>
        /// <param name="configureOptions"></param>
        public FacebookAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            IOptions<SharedAuthenticationOptions> sharedOptions,
            IOptions<FacebookAuthenticationOptions> options,
            ConfigureOptions<FacebookAuthenticationOptions> configureOptions = null)
            : base(next, dataProtectionProvider, loggerFactory, encoder, sharedOptions, options, configureOptions)
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

            if (string.IsNullOrEmpty(Options.AppId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(Options.AppId)));
            }
            if (string.IsNullOrEmpty(Options.AppSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(Options.AppSecret)));
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="FacebookAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<FacebookAuthenticationOptions> CreateHandler()
        {
            return new FacebookAuthenticationHandler(Backchannel);
        }
    }
}