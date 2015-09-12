// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Caching.Distributed;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
        /// <param name="encoder"></param>
        /// <param name="services"></param>
        /// <param name="sharedOptions"></param>
        /// <param name="options">a <see cref="IOptions{OpenIdConnectAuthenticationOptions}"/> instance that will supply <see cref="OpenIdConnectAuthenticationOptions"/> 
        /// if configureOptions is null.</param>
        /// <param name="configureOptions">a <see cref="ConfigureOptions{OpenIdConnectAuthenticationOptions}"/> instance that will be passed to an instance of <see cref="OpenIdConnectAuthenticationOptions"/>
        /// that is retrieved by calling <see cref="IOptions{OpenIdConnectAuthenticationOptions}.GetNamedOptions(string)"/> where string == <see cref="ConfigureOptions{OpenIdConnectAuthenticationOptions}.Name"/> provides runtime configuration.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        public OpenIdConnectAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            IServiceProvider services,
            IOptions<SharedAuthenticationOptions> sharedOptions,
            IOptions<OpenIdConnectAuthenticationOptions> options,
            ConfigureOptions<OpenIdConnectAuthenticationOptions> configureOptions = null)
            : base(next, options, loggerFactory, encoder, configureOptions)
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

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (sharedOptions == null)
            {
                throw new ArgumentNullException(nameof(sharedOptions));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(Options.SignInScheme) && !string.IsNullOrEmpty(sharedOptions.Value.SignInScheme))
            {
                Options.SignInScheme = sharedOptions.Value.SignInScheme;
            }

            if (Options.HtmlEncoder == null)
            {
                Options.HtmlEncoder = services.GetHtmlEncoder();
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

            if (Options.Events == null)
            {
                Options.Events = new OpenIdConnectAuthenticationEvents();
            }

            if (string.IsNullOrEmpty(Options.TokenValidationParameters.ValidAudience) && !string.IsNullOrEmpty(Options.ClientId))
            {
                Options.TokenValidationParameters.ValidAudience = Options.ClientId;
            }

            Backchannel = new HttpClient(Options.BackchannelHttpHandler ?? new HttpClientHandler());
            Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET OpenIdConnect middleware");
            Backchannel.Timeout = Options.BackchannelTimeout;
            Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB

            if (Options.ConfigurationManager == null)
            {
                if (Options.Configuration != null)
                {
                    Options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(Options.Configuration);
                }
                else if (!(string.IsNullOrEmpty(Options.MetadataAddress) && string.IsNullOrEmpty(Options.Authority)))
                {
                    if (string.IsNullOrEmpty(Options.MetadataAddress) && !string.IsNullOrEmpty(Options.Authority))
                    {
                        Options.MetadataAddress = Options.Authority;
                        if (!Options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            Options.MetadataAddress += "/";
                        }

                        Options.MetadataAddress += ".well-known/openid-configuration";
                    }

                    Options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(Options.MetadataAddress, new OpenIdConnectConfigurationRetriever(), Backchannel);
                }
            }

            if (Options.CacheNonces && Options.NonceCache == null)
            {
                // Use the global distributed cache if the user has not provided his own instance.
                // Note: GetRequiredService will throw an exception if caching services have not been registered.
                Options.NonceCache = services.GetRequiredService<IDistributedCache>();
            }
        }

        protected HttpClient Backchannel { get; private set; }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="OpenIdConnectAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new OpenIdConnectAuthenticationHandler(Backchannel);
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
