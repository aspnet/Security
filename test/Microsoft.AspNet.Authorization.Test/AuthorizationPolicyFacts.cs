// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Authroization.Test
{
    public class AuthorizationPolicyFacts
    {
        [Fact]
        public void RequireRoleThrowsIfEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthorizationPolicyBuilder().RequireRole().Build().BuildRequirements(new ServiceCollection().BuildServiceProvider()));
        }

        [Fact]
        public void CanCombineAuthorizeAttributes()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute(),
                new AuthorizeAttribute("1") { ActiveAuthenticationSchemes = "dupe" },
                new AuthorizeAttribute("2") { ActiveAuthenticationSchemes = "dupe" },
                new AuthorizeAttribute { Roles = "r1,r2", ActiveAuthenticationSchemes = "roles" },
            };
            var options = new AuthorizationOptions();
            options.AddPolicy("1", policy => policy.RequireClaim("1"));
            options.AddPolicy("2", policy => policy.RequireClaim("2"));

            // Act
            var combined = AuthorizationPolicy.Combine(options, attributes);

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.AuthenticationSchemes.Contains("roles"));
            var reqs = combined.BuildRequirements(new ServiceCollection().BuildServiceProvider());
            Assert.Equal(4, reqs.Count());
            Assert.True(reqs.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(2, reqs.OfType<ClaimsAuthorizationRequirement>().Count());
            Assert.Equal(1, reqs.OfType<RolesAuthorizationRequirement>().Count());
        }

        [Fact]
        public void CanReplaceDefaultPolicy()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute(),
                new AuthorizeAttribute("2") { ActiveAuthenticationSchemes = "dupe" }
            };
            var options = new AuthorizationOptions();
            options.DefaultPolicy = new AuthorizationPolicyBuilder("default").RequireClaim("default").Build();
            options.AddPolicy("2", policy => policy.RequireClaim("2"));

            // Act
            var combined = AuthorizationPolicy.Combine(options, attributes);

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.AuthenticationSchemes.Contains("default"));
            var reqs = combined.BuildRequirements(new ServiceCollection().BuildServiceProvider());
            Assert.Equal(2, reqs.Count());
            Assert.False(reqs.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(2, reqs.OfType<ClaimsAuthorizationRequirement>().Count());
        }
    }
}