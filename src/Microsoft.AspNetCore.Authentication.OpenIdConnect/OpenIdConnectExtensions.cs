// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdConnectExtensions
    {
        /// <summary>
        /// Adds OpenIdConnect authentication with options bound against the "OpenIdConnect" section 
        /// from the IConfiguration in the service container.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services)
        {
            services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectConfigureOptions>();
            return services.AddOpenIdConnectAuthentication(OpenIdConnectDefaults.AuthenticationScheme, _ => { });
        }

        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, Action<OpenIdConnectOptions> configureOptions) 
            => services.AddOpenIdConnectAuthentication(OpenIdConnectDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, string authenticationScheme, Action<OpenIdConnectOptions> configureOptions)
        {
            // Makes sure that DataProtectionProvider is always set.
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeOptions<OpenIdConnectOptions>, EnsureDataProtection>());
            services.Initialize<OpenIdConnectOptions>(authenticationScheme, options =>
            {
                if (string.IsNullOrEmpty(options.SignOutScheme))
                {
                    options.SignOutScheme = options.SignInScheme;
                }

                if (options.StateDataFormat == null)
                {
                    if (options.DataProtectionProvider == null)
                    {
                        // This shouldn't happen normally due to the EnsureDataProtection initialize options.
                        throw new InvalidOperationException("DataProtectionProvider must be provided.");
                    }

                    var dataProtector = options.DataProtectionProvider.CreateProtector(
                        typeof(OpenIdConnectHandler).FullName, authenticationScheme, "v1");
                    options.StateDataFormat = new PropertiesDataFormat(dataProtector);
                }

                if (options.StringDataFormat == null)
                {
                    if (options.DataProtectionProvider == null)
                    {
                        // This shouldn't happen normally due to the EnsureDataProtection initialize options.
                        throw new InvalidOperationException("DataProtectionProvider must be provided.");
                    }

                    var dataProtector = options.DataProtectionProvider.CreateProtector(
                        typeof(OpenIdConnectHandler).FullName,
                        typeof(string).FullName,
                        authenticationScheme,
                        "v1");

                    options.StringDataFormat = new SecureDataFormat<string>(new StringSerializer(), dataProtector);
                }

                if (string.IsNullOrEmpty(options.TokenValidationParameters.ValidAudience) && !string.IsNullOrEmpty(options.ClientId))
                {
                    options.TokenValidationParameters.ValidAudience = options.ClientId;
                }

                if (options.Backchannel == null)
                {
                    options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
                    options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OpenIdConnect handler");
                    options.Backchannel.Timeout = options.BackchannelTimeout;
                    options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
                }

                if (options.ConfigurationManager == null)
                {
                    if (options.Configuration != null)
                    {
                        options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(options.Configuration);
                    }
                    else if (!(string.IsNullOrEmpty(options.MetadataAddress) && string.IsNullOrEmpty(options.Authority)))
                    {
                        if (string.IsNullOrEmpty(options.MetadataAddress) && !string.IsNullOrEmpty(options.Authority))
                        {
                            options.MetadataAddress = options.Authority;
                            if (!options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                            {
                                options.MetadataAddress += "/";
                            }

                            options.MetadataAddress += ".well-known/openid-configuration";
                        }

                        if (options.RequireHttpsMetadata && !options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
                        }

                        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(options.MetadataAddress, new OpenIdConnectConfigurationRetriever(),
                            new HttpDocumentRetriever(options.Backchannel) { RequireHttps = options.RequireHttpsMetadata });
                    }
                }
            });
            return services.AddRemoteScheme<OpenIdConnectOptions, OpenIdConnectHandler>(authenticationScheme, authenticationScheme, configureOptions);
        }

        // Used to ensure that there's always a default data protection provider
        private class EnsureDataProtection : IInitializeOptions<OpenIdConnectOptions>
        {
            private readonly IDataProtectionProvider _dp;

            public EnsureDataProtection(IDataProtectionProvider dataProtection)
            {
                _dp = dataProtection;
            }

            public void Initialize(string name, OpenIdConnectOptions options)
            {
                options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;
            }
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
