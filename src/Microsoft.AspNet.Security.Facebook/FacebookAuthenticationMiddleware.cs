// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Security.DataProtection;
using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.AspNet.Security.OAuth;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security.Facebook
{
    /// <summary>
    /// An ASP.NET middleware for authenticating users using Facebook.
    /// </summary>
    public class FacebookAuthenticationMiddleware : OAuthAuthenticationMiddleware<FacebookAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new <see cref="FacebookAuthenticationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the application pipeline to invoke.</param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="options">Configuration options for the middleware.</param>
        public FacebookAuthenticationMiddleware(
            RequestDelegate next,
            IServiceProvider services,
            IEventBus events,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IOptions<ExternalAuthenticationOptions> externalOptions,
            IOptions<FacebookAuthenticationOptions> options,
            ConfigureOptions<FacebookAuthenticationOptions> configureOptions = null)
            : base(next, services, events, dataProtectionProvider, loggerFactory, externalOptions, options, configureOptions)
        {
            if (string.IsNullOrWhiteSpace(Options.AppId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "AppId"));
            }
            if (string.IsNullOrWhiteSpace(Options.AppSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "AppSecret"));
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="FacebookAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<FacebookAuthenticationOptions> CreateHandler()
        {
            return new FacebookAuthenticationHandler(Backchannel, Logger, Events);
        }
    }
}
