// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OAuthExtensions
    {
        public static IServiceCollection AddOAuthAuthentication(this IServiceCollection services, string authenticationScheme, Action<OAuthOptions> configureOptions)
        {
            return services.AddScheme<OAuthOptions, OAuthHandler<OAuthOptions>>(authenticationScheme, authenticationScheme, configureOptions);
        }

        public static IServiceCollection AddOAuthAuthentication<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : OAuthOptions, new()
            where THandler : OAuthHandler<TOptions>
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeOptions<TOptions>, EnsureDataProtection<TOptions>>());
            services.Initialize<TOptions>(authenticationScheme, options =>
            {
                if (options.Backchannel == null)
                {
                    options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
                    options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth handler");
                    options.Backchannel.Timeout = options.BackchannelTimeout;
                    options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
                }

                if (options.StateDataFormat == null)
                {
                    if (options.DataProtectionProvider == null)
                    {
                        // This shouldn't happen normally due to the EnsureDataProtection initialize options.
                        throw new InvalidOperationException("DataProtectionProvider must be provided.");
                    }

                    var dataProtector = options.DataProtectionProvider.CreateProtector(
                        typeof(THandler).FullName, authenticationScheme, "v1");
                    options.StateDataFormat = new PropertiesDataFormat(dataProtector);
                }
            });
            return services.AddRemoteScheme<TOptions, THandler>(authenticationScheme, authenticationScheme, configureOptions);
        }

        // Used to ensure that there's always a default data protection provider
        private class EnsureDataProtection<TOptions> : IInitializeOptions<TOptions> where TOptions : OAuthOptions
        {
            private readonly IDataProtectionProvider _dp;

            public EnsureDataProtection(IDataProtectionProvider dataProtection)
            {
                _dp = dataProtection;
            }

            public void Initialize(string name, TOptions options)
            {
                options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;
            }
        }

    }
}
