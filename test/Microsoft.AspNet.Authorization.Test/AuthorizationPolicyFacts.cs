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
            Assert.Throws<InvalidOperationException>(() => new AuthorizationPolicyBuilder().RequireRole().Build(new ServiceCollection().BuildServiceProvider()));
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
            var combined = AuthorizationPolicy.Combine(options, attributes).Build(new ServiceCollection().BuildServiceProvider());

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.AuthenticationSchemes.Contains("roles"));
            Assert.Equal(4, combined.Requirements.Count());
            Assert.True(combined.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(2, combined.Requirements.OfType<ClaimsAuthorizationRequirement>().Count());
            Assert.Equal(1, combined.Requirements.OfType<RolesAuthorizationRequirement>().Count());
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
            options.DefaultPolicy = new AuthorizationPolicyBuilder("default").RequireClaim("default");
            options.AddPolicy("2", policy => policy.RequireClaim("2"));

            // Act
            var combined = AuthorizationPolicy.Combine(options, attributes).Build(new ServiceCollection().BuildServiceProvider());

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.AuthenticationSchemes.Contains("default"));
            Assert.Equal(2, combined.Requirements.Count());
            Assert.False(combined.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(2, combined.Requirements.OfType<ClaimsAuthorizationRequirement>().Count());
        }
    }
}