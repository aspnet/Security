﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Text;
using Microsoft.AspNet.Authentication.DataHandler;
using Microsoft.AspNet.Authentication.DataHandler.Encoder;
using Microsoft.AspNet.Authentication.DataHandler.Serializer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Authentication.OpenIdConnect
{
    /// <summary>
    /// ASP.NET middleware for obtaining identities using OpenIdConnect protocol.
    /// </summary>
    public class OpenIdConnectAuthenticationMiddleware : AuthenticationMiddleware<OpenIdConnectAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a <see cref="OpenIdConnectAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the ASP.NET pipeline to invoke.</param>
        /// <param name="dataProtectionProvider"> provider for creating a data protector.</param>
        /// <param name="loggerFactory">factory for creating a <see cref="ILogger"/>.</param>
        /// <param name="options">a <see cref="IOptions{OpenIdConnectAuthenticationOptions}"/> instance that will supply <see cref="OpenIdConnectAuthenticationOptions"/> 
        /// if configureOptions is null.</param>
        /// <param name="configureOptions">a <see cref="ConfigureOptions{OpenIdConnectAuthenticationOptions}"/> instance that will be passed to an instance of <see cref="OpenIdConnectAuthenticationOptions"/>
        /// that is retrieved by calling <see cref="IOptions{OpenIdConnectAuthenticationOptions}.GetNamedOptions(string)"/> where string == <see cref="ConfigureOptions{OpenIdConnectAuthenticationOptions}.Name"/> provides runtime configuration.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        public OpenIdConnectAuthenticationMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IDataProtectionProvider dataProtectionProvider,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IUrlEncoder encoder,
            [NotNull] IOptions<ExternalAuthenticationOptions> externalOptions,
            [NotNull] IOptions<OpenIdConnectAuthenticationOptions> options,
            ConfigureOptions<OpenIdConnectAuthenticationOptions> configureOptions = null)
            : base(next, options, loggerFactory, encoder, configureOptions)
        {
<<<<<<< HEAD
            if (string.IsNullOrEmpty(Options.SignInScheme) && !string.IsNullOrEmpty(externalOptions.Options.SignInScheme))
=======
            _logger = loggerFactory.CreateLogger<OpenIdConnectAuthenticationMiddleware>();

            if (Options.IdentityModelEventSourceListener != null)
            {
                Options.IdentityModelEventSourceListener.EnableEvents(IdentityModelEventSource.Logger, EventLevel.Informational);
            }
            else
            {
                DefaultLoggingListener eventListener = new DefaultLoggingListener(_logger);
                IdentityModelEventSource.LogLevel = EventLevel.Informational;
                eventListener.EnableEvents(IdentityModelEventSource.Logger, EventLevel.Informational);
            }

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.AuthenticationType))
>>>>>>> Adding logging draft.
            {
                Options.SignInScheme = externalOptions.Options.SignInScheme;
            }

            if (Options.StateDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(OpenIdConnectAuthenticationMiddleware).FullName, 
					typeof(string).FullName,
                    Options.AuthenticationScheme,
					"v1");

                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            if (Options.StringDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(OpenIdConnectAuthenticationMiddleware).FullName,
                    typeof(string).FullName,
                    Options.AuthenticationScheme,
                    "v1");

                Options.StringDataFormat = new SecureDataFormat<string>(new StringSerializer(), dataProtector, TextEncodings.Base64Url);
            }

            if (Options.SecurityTokenValidators == null)
            {
                Options.SecurityTokenValidators = new Collection<ISecurityTokenValidator> { new JwtSecurityTokenHandler() };
            }
            
            // if the user has not set the AuthorizeCallback, set it from the redirect_uri
            if (!Options.CallbackPath.HasValue)
            {
                Uri redirectUri;
                if (!string.IsNullOrEmpty(Options.RedirectUri) && Uri.TryCreate(Options.RedirectUri, UriKind.Absolute, out redirectUri))
                {
                    // Redirect_Uri must be a very specific, case sensitive value, so we can't generate it. Instead we generate AuthorizeCallback from it.
                    Options.CallbackPath = PathString.FromUriComponent(redirectUri);
                }
            }

            if (Options.Notifications == null)
            {
                Options.Notifications = new OpenIdConnectAuthenticationNotifications();
            }

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.ValidAudience) && !string.IsNullOrWhiteSpace(Options.ClientId))
            {
                Options.TokenValidationParameters.ValidAudience = Options.ClientId;
            }

            if (Options.ConfigurationManager == null)
            {
                if (Options.Configuration != null)
                {
                    Options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(Options.Configuration);
                }
                else if (!(string.IsNullOrWhiteSpace(Options.MetadataAddress) && string.IsNullOrWhiteSpace(Options.Authority)))
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

                    var httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
                    httpClient.Timeout = Options.BackchannelTimeout;
                    httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
                    Options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(Options.MetadataAddress, httpClient);
                }
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="OpenIdConnectAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new OpenIdConnectAuthenticationHandler();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(OpenIdConnectAuthenticationOptions options)
        {
            var handler = options.BackchannelHttpHandler ??
#if DNX451
                new WebRequestHandler();
            // If they provided a validator, apply it or fail.
            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Resources.OIDCH_0102_ExceptionValidatorHandlerMismatch);
                }
                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }
#else
                new WinHttpHandler();
#endif
            return handler;
        }

        private class StringSerializer : IDataSerializer<string>
        {
            public string Deserialize(byte[] data)
            {
                return Encoding.UTF8.GetString(data);
            }

            public byte[] Serialize(string model)
            {
                return Encoding.UTF8.GetBytes(model);
            }
        }
    }
}
