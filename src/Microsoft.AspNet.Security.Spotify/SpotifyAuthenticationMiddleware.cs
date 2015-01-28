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
using Microsoft.AspNet.Security.Spotify.Notifications;

namespace Microsoft.AspNet.Security.Spotify
{
    /// <summary>
    /// An ASP.NET middleware for authenticating users using Spotify.
    /// </summary>
    public class SpotifyAuthenticationMiddleware : OAuthAuthenticationMiddleware<SpotifyAuthenticationOptions, SpotifyAuthenticationNotifications>
    {
        /// <summary>
        /// Initializes a new <see cref="SpotifyAuthenticationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the application pipeline to invoke.</param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="options">Configuration options for the middleware.</param>
        public SpotifyAuthenticationMiddleware(RequestDelegate next,
            IServiceProvider services,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IOptions<ExternalAuthenticationOptions> externalOptions,
            IOptions<SpotifyAuthenticationOptions> options,
            ConfigureOptions<SpotifyAuthenticationOptions> configureOptions = null)
            : base(next, services, dataProtectionProvider, loggerFactory, externalOptions, options, configureOptions)
        {
            if (string.IsNullOrWhiteSpace(Options.ClientId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "ClientId"));
            }
            if (string.IsNullOrWhiteSpace(Options.ClientSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "ClientSecret"));
            }

            if (Options.Notifications == null)
            {
                Options.Notifications = new SpotifyAuthenticationNotifications();
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="SpotifyAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<SpotifyAuthenticationOptions> CreateHandler()
        {
            return new SpotifyAuthenticationHandler(Backchannel, Logger);
        }
    }
}