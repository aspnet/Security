// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.Policy.Test
{
    public class AuthenticationPolicyTests
    {
        [Fact]
        public async Task CanSelectPolicyBasedOnContext()
        {
            // Arrange
            var provider = BuildProvider(services => 
            {
                services.Configure<AuthenticationPolicyOptions>(o => 
                {   
                    o.DefaultPolicySelector = ctx => (string)ctx.Items["policy"];
                    o.AddPolicy("auth", p => p.DefaultScheme = "default");
                });
            });
            var context = new DefaultHttpContext();
            context.Items["policy"] = "auth";

            // Act
            var result = await provider.GetAsync(context, name: null);

            // Assert
            Assert.Equal("default", result.AuthenticateSchemes.First());
        }

        private IAuthenticationPolicyProvider BuildProvider(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection()
                .AddAuthorizationRequestStuff()
                .AddLogging()
                .AddOptions();
            setupServices?.Invoke(services);
            return services.BuildServiceProvider().GetRequiredService<IAuthenticationPolicyProvider>();
        }
    }
}