// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2.Twitter;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TwitterExtensions
    {
        public static IServiceCollection AddTwitterAuthentication(this IServiceCollection services, Action<TwitterOptions> configureOptions)
        {
            var options = new TwitterOptions();
            configureOptions?.Invoke(options);
            services.AddAuthentication(o =>
            {
                o.AddScheme(options.AuthenticationScheme, b =>
                {
                    b.HandlerType = typeof(TwitterHandler);
                    b.Settings["Options"] = options;
                });
            });
            services.AddTransient<TwitterHandler>();
            return services;
        }
    }
}
