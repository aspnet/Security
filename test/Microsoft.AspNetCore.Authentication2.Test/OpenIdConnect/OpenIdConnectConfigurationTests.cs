// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication2.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Authentication2.Test.OpenIdConnect
{
    public class OpenIdConnectConfigurationTests
    {
        [Fact]
        public void MetadataAddressIsGeneratedFromAuthorityWhenMissing()
        {
            // Fix this
            Assert.False(true);
            //BuildTestServer(o =>
            //{
            //    o.Authority = TestServerBuilder.DefaultAuthority;
            //    o.ClientId = Guid.NewGuid().ToString();
            //    o.SignInScheme = Guid.NewGuid().ToString()
            //});

            //Assert.Equal($"{TestServerBuilder.DefaultAuthority}/.well-known/openid-configuration", options.MetadataAddress);
        }

        public void ThrowsWhenSignInSchemeIsMissing()
        {
            TestConfigurationException<ArgumentException>(
                 o =>
                 {
                     o.Authority = TestServerBuilder.DefaultAuthority;
                     o.ClientId = Guid.NewGuid().ToString();
                 },
                 ex => Assert.Equal("SignInScheme", ex.ParamName));
        }

        [Fact]
        public void ThrowsWhenClientIdIsMissing()
        {
            TestConfigurationException<ArgumentException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.Authority = TestServerBuilder.DefaultAuthority;
                },
                ex => Assert.Equal("ClientId", ex.ParamName));
        }

        [Fact]
        public void ThrowsWhenAuthorityIsMissing()
        {
            TestConfigurationException<InvalidOperationException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.ClientId = "Test Id";
                },
                ex => Assert.Equal("Provide Authority, MetadataAddress, Configuration, or ConfigurationManager to OpenIdConnectOptions", ex.Message)
            );
        }

        [Fact]
        public void ThrowsWhenAuthorityIsNotHttps()
        {
            TestConfigurationException<InvalidOperationException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.ClientId = "Test Id";
                    o.MetadataAddress = "http://example.com";
                },
                ex => Assert.Equal("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.", ex.Message)
            );
        }

        [Fact]
        public void ThrowsWhenMetadataAddressIsNotHttps()
        {
            TestConfigurationException<InvalidOperationException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.ClientId = "Test Id";
                    o.MetadataAddress = "http://example.com";
                },
                ex => Assert.Equal("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.", ex.Message)
            );
        }

        private TestServer BuildTestServer(Action<OpenIdConnectOptions> options)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => services.AddOpenIdConnectAuthentication(options))
                .Configure(app => app.UseAuthentication());

            return new TestServer(builder);
        }

        private void TestConfigurationException<T>(
            Action<OpenIdConnectOptions> options,
            Action<T> verifyException)
            where T : Exception
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => services.AddOpenIdConnectAuthentication(options))
                .Configure(app => app.UseAuthentication());

            var exception = Assert.Throws<T>(() =>
            {
                new TestServer(builder);
            });

            verifyException(exception);
        }
    }
}
