// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Infrastructure;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    public class AzureAdB2CTests
    {
        [Fact]
        public void AddCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Instance", "<azure>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Domain", "<domain>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:UseTokenLifetime", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:SaveTokens", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClaimsIssuer", "<issuer>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RemoteAuthenticationTimeout", "0.0:0:30"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:EditProfilePolicyId", "<editProfileId>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:SignInSignUpPolicyId", "<signInId>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ResetPasswordPolicyId", "<resetId>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:SignInScheme", "<signIn>"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddTransient<IConfigureOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureNamedOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddAzureAdB2CAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            // EditProfile options
            var options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.EditProfileAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure>/<domain>/<editProfileId>/v2.0", options.Authority);

            // ResetPassword options
            options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.ResetPasswordAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure>/<domain>/<resetId>/v2.0", options.Authority);

            // SignInSignUp options
            options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.SignInSignUpAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure>/<domain>/<signInId>/v2.0", options.Authority);
        }

        [Fact]
        public void AddBearerCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Instance", "<azure>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Domain", "<domain>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:UseTokenLifetime", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:SaveTokens", "true"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClaimsIssuer", "<issuer>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RemoteAuthenticationTimeout", "0.0:0:30"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:EditProfilePolicyId", "<editProfileId>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:SignInSignUpPolicyId", "<signInId>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ResetPasswordPolicyId", "<resetId>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:SignInScheme", "<signIn>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2CBearer:RequireHttpsMetadata", "false"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddTransient<IConfigureOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureNamedOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureDefaults<JwtBearerOptions>>()
                .AddTransient<IConfigureNamedOptions<JwtBearerOptions>, ConfigureDefaults<JwtBearerOptions>>()
                .AddAzureAdB2CBearerAuthentication()
                .AddSingleton<IConfiguration>(config);

            var sp = services.BuildServiceProvider();

            // JwtBearer options
            var bearer = sp.GetRequiredService<IOptionsSnapshot<JwtBearerOptions>>().Get(AzureAdB2CDefaults.BearerAuthenticationScheme);
            Assert.Equal("<id>", bearer.Audience);
            Assert.Equal("<azure>/<domain>/<signInId>/v2.0", bearer.Authority);

            // EditProfile options
            var options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.EditProfileAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure>/<domain>/<editProfileId>/v2.0", options.Authority);

            // ResetPassword options
            options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.ResetPasswordAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure>/<domain>/<resetId>/v2.0", options.Authority);

            // SignInSignUp options
            options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.SignInSignUpAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<azure>/<domain>/<signInId>/v2.0", options.Authority);
        }

        [Fact]
        public void PolicyIdsRequired()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Instance", "<azure>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Domain", "<domain>"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddTransient<IConfigureOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureNamedOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddAzureAdB2CAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            var e = Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.EditProfileAuthenticationScheme));
            Assert.Contains("EditProfilePolicyId", e.Message);
            e = Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.SignInSignUpAuthenticationScheme));
            Assert.Contains("SignInSignUpPolicyId", e.Message);
            e = Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.ResetPasswordAuthenticationScheme));
            Assert.Contains("ResetPasswordPolicyId", e.Message);
        }

        [Fact]
        public void InstanceRequired()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Domain", "<azure>"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddTransient<IConfigureOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureNamedOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddAzureAdB2CAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            var e = Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.AuthenticationScheme));
            Assert.Contains("Instance", e.Message);
        }

        [Fact]
        public void DomainRequired()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Instance", "<azure>"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddTransient<IConfigureOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureNamedOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddAzureAdB2CAuthentication()
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            var e = Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.AuthenticationScheme));
            Assert.Contains("Domain", e.Message);
        }

        [Fact]
        public void SettingAuthorityOverridesConfig()
        {
            var dic = new Dictionary<string, string>
            {
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientId", "<id>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:ClientSecret", "<secret>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:RequireHttpsMetadata", "false"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Instance", "<azure>"},
                {"Microsoft:AspNetCore:Authentication:Schemes:AzureAdB2C:Domain", "<domain>"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddTransient<IConfigureOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddTransient<IConfigureNamedOptions<AzureAdB2COptions>, ConfigureDefaults<AzureAdB2COptions>>()
                .AddAzureAdB2CAuthentication()
                .Configure<AzureAdB2COptions>(AzureAdB2CDefaults.EditProfileAuthenticationScheme, o => o.Authority = "(edit)")
                .Configure<AzureAdB2COptions>(AzureAdB2CDefaults.ResetPasswordAuthenticationScheme, o => o.Authority = "(reset)")
                .Configure<AzureAdB2COptions>(AzureAdB2CDefaults.SignInSignUpAuthenticationScheme, o => o.Authority = "(sign)")
                .AddSingleton<IConfiguration>(config);
            var sp = services.BuildServiceProvider();

            // EditProfile options
            var options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.EditProfileAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("(edit)", options.Authority);

            // ResetPassword options
            options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.ResetPasswordAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("(reset)", options.Authority);

            // SignInSignUp options
            options = sp.GetRequiredService<IOptionsSnapshot<AzureAdB2COptions>>().Get(AzureAdB2CDefaults.SignInSignUpAuthenticationScheme);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal("<azure>", options.Instance);
            Assert.Equal("<domain>", options.Domain);
            Assert.Equal("(sign)", options.Authority);
        }
    }
}