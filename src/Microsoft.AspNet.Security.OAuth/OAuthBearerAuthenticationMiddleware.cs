﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Security.DataProtection;
using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;
using System.Net.Http;

namespace Microsoft.AspNet.Security.OAuth
{
	/// <summary>
	/// Bearer authentication middleware component which is added to an HTTP pipeline. This class is not
	/// created by application code directly, instead it is added by calling the the IAppBuilder UseOAuthBearerAuthentication
	/// extension method.
	/// </summary>
	public class OAuthBearerAuthenticationMiddleware : AuthenticationMiddleware<OAuthBearerAuthenticationOptions>
    {
        private readonly ILogger _logger;

        private readonly string _challenge;

        /// <summary>
        /// Bearer authentication component which is added to an HTTP pipeline. This constructor is not
        /// called by application code directly, instead it is added by calling the the IAppBuilder UseOAuthBearerAuthentication 
        /// extension method.
        /// </summary>
        public OAuthBearerAuthenticationMiddleware(
            RequestDelegate next,
            IServiceProvider services,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IOptions<OAuthBearerAuthenticationOptions> options,
            ConfigureOptions<OAuthBearerAuthenticationOptions> configureOptions)
            : base(next, services, options, configureOptions)
        {
            _logger = loggerFactory.Create<OAuthBearerAuthenticationMiddleware>();

            if (!string.IsNullOrWhiteSpace(Options.Challenge))
            {
                _challenge = Options.Challenge;
            }
            else if (string.IsNullOrWhiteSpace(Options.Realm))
            {
                _challenge = "Bearer";
            }
            else
            {
                _challenge = "Bearer realm=\"" + Options.Realm + "\"";
            }

            if (Options.Notifications == null)
            {
                Options.Notifications = new OAuthBearerAuthenticationNotifications();
            }

            if (Options.SecurityTokenValidators == null)
            {
                Options.SecurityTokenValidators = new Collection<ISecurityTokenValidator> { new JwtSecurityTokenHandler() };
            }

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.ValidAudience) && !string.IsNullOrWhiteSpace(Options.Audience))
            {
                Options.TokenValidationParameters.ValidAudience = Options.Audience;
            }

            if (Options.ConfigurationManager == null)
            {
                if (Options.Configuration != null)
                {
                    Options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(Options.Configuration);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Options.MetadataAddress) && !string.IsNullOrWhiteSpace(Options.Authority))
                    {
                        Options.MetadataAddress = Options.Authority;
                        if (!Options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            Options.MetadataAddress += "/";
                        }

                        Options.MetadataAddress += ".well-known/openid-configuration";
                    }

                    HttpClient httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
                    httpClient.Timeout = Options.BackchannelTimeout;
                    httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
                    Options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(Options.MetadataAddress, httpClient);
                }
            }
        }

        /// <summary>
        /// Called by the AuthenticationMiddleware base class to create a per-request handler. 
        /// </summary>
        /// <returns>A new instance of the request handler</returns>
        protected override AuthenticationHandler<OAuthBearerAuthenticationOptions> CreateHandler()
        {
            return new OAuthBearerAuthenticationHandler(_logger, _challenge);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(OAuthBearerAuthenticationOptions options)
        {
            HttpMessageHandler handler = options.BackchannelHttpHandler ??
#if ASPNET50
            new WebRequestHandler();
            // If they provided a validator, apply it or fail.
            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
                }
                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }
#else
            new WinHttpHandler();
#endif
            return handler;
        }
    }
}
