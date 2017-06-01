// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Infrastructure;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    public class AzureAdTests
    {
        [Fact]
        public void AddCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:Instance", "<azure>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:TenantId", "<tenant>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:UseTokenLifetime", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:SaveTokens", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClaimsIssuer", "<issuer>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RemoteAuthenticationTimeout", "0.0:0:30"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:SignInScheme", "<signIn>"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfigureOptions<AzureAdOptions>, ConfigureDefaults<AzureAdOptions>>()
                .AddAzureAdAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsSnapshot<AzureAdOptions>>().Get(AzureAdDefaults.AuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<tenant>", options.TenantId);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure><tenant>", options.Authority);
        }

        [Fact]
        public void AddBearerCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:Instance", "<azure>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:TenantId", "<tenant>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:UseTokenLifetime", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:SaveTokens", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClaimsIssuer", "<issuer>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RemoteAuthenticationTimeout", "0.0:0:30"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:SignInScheme", "<signIn>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdBearer:RequireHttpsMetadata", "false"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfigureOptions<AzureAdOptions>, ConfigureDefaults<AzureAdOptions>>()
                .AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureDefaults<JwtBearerOptions>>()
                .AddTransient<IConfigureNamedOptions<JwtBearerOptions>, ConfigureDefaults<JwtBearerOptions>>()
                .AddAzureAdBearerAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            // JwtBearer options
            var bearer = sp.GetRequiredService<IOptionsSnapshot<JwtBearerOptions>>().Get(AzureAdDefaults.BearerAuthenticationScheme);
            Assert.Equal("<id>", bearer.Audience);
            Assert.Equal("<azure><tenant>", bearer.Authority);

            var options = sp.GetRequiredService<IOptionsSnapshot<AzureAdOptions>>().Get(AzureAdDefaults.AuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<tenant>", options.TenantId);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure><tenant>", options.Authority);
        }

        [Fact]
        public void InstanceRequired()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:TenantId", "<tenant>"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfigureOptions<AzureAdOptions>, ConfigureDefaults<AzureAdOptions>>()
                .AddAzureAdAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdOptions>>().Get(AzureAdDefaults.AuthenticationScheme));
        }

        [Fact]
        public void TenantIdRequired()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:Instance", "<azure>"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfigureOptions<AzureAdOptions>, ConfigureDefaults<AzureAdOptions>>()
                .AddAzureAdAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdOptions>>().Get(AzureAdDefaults.AuthenticationScheme));
        }

        [Fact]
        public void SettingAuthorityOverrides()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAd:RequireHttpsMetadata", "false"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfigureOptions<AzureAdOptions>, ConfigureDefaults<AzureAdOptions>>()
                .AddAzureAdAuthentication(o => o.Authority = "(authority)")
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsSnapshot<AzureAdOptions>>().Get(AzureAdDefaults.AuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("(authority)", options.Authority);
        }
    }
}