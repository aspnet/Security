// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TwitterExtensions
    {
        /// <summary>
        /// Adds Twitter authentication with options bound against the "Twitter" section 
        /// from the IConfiguration in the service container.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTwitterAuthentication(this IServiceCollection services)
        {
            services.AddSingleton<IConfigureOptions<TwitterOptions>, TwitterConfigureOptions>();
            return services.AddTwitterAuthentication(TwitterDefaults.AuthenticationScheme, _ => { });
        }

        public static IServiceCollection AddTwitterAuthentication(this IServiceCollection services, Action<TwitterOptions> configureOptions)
            => services.AddTwitterAuthentication(TwitterDefaults.AuthenticationScheme, configureOptions);

        public static IServiceCollection AddTwitterAuthentication(this IServiceCollection services, string authenticationScheme, Action<TwitterOptions> configureOptions)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeOptions<TwitterOptions>, EnsureDataProtection>());
            services.Initialize<TwitterOptions>(authenticationScheme, options =>
            {
                if (options.StateDataFormat == null)
                {
                    if (options.DataProtectionProvider == null)
                    {
                        // This shouldn't happen normally due to the EnsureDataProtection initialize options.
                        throw new InvalidOperationException("DataProtectionProvider must be provided.");
                    }

                    var dataProtector = options.DataProtectionProvider.CreateProtector(
                        typeof(TwitterHandler).FullName, authenticationScheme, "v1");
                    options.StateDataFormat = new SecureDataFormat<RequestToken>(
                        new RequestTokenSerializer(),
                        dataProtector);
                }

                if (options.Backchannel == null)
                {
                    options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
                    options.Backchannel.Timeout = options.BackchannelTimeout;
                    options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
                    options.Backchannel.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                    options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core Twitter handler");
                    options.Backchannel.DefaultRequestHeaders.ExpectContinue = false;
                }
            });
            return services.AddRemoteScheme<TwitterOptions, TwitterHandler>(authenticationScheme, authenticationScheme, configureOptions);
        }

        // Used to ensure that there's always a default data protection provider
        private class EnsureDataProtection : IInitializeOptions<TwitterOptions>
        {
            private readonly IDataProtectionProvider _dp;

            public EnsureDataProtection(IDataProtectionProvider dataProtection)
            {
                _dp = dataProtection;
            }

            public void Initialize(string name, TwitterOptions options)
            {
                options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;
            }
        }
    }
}
