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
    public class SpotifyAuthenticationMiddleware : OAuthAuthenticationMiddleware<SpotifyAuthenticationOptions, SpotifyAuthenticationNotifications>
    {
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
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "", "ClientId"));
            }
            if (string.IsNullOrWhiteSpace(Options.ClientSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "", "ClientSecret"));
            }

            if (Options.Notifications == null)
            {
                Options.Notifications = new SpotifyAuthenticationNotifications();
            }
        }

        protected override AuthenticationHandler<SpotifyAuthenticationOptions> CreateHandler()
        {
            return new SpotifyAuthenticationHandler(Backchannel, Logger);
        }
    }
}