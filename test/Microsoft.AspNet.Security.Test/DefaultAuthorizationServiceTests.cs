// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Security.Test
{
    public class DefaultAuthorizationServiceTests
    {
        [Fact]
        public async Task Check_ShouldAllowIfClaimIsPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage", "CanViewAnything");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim("Permission", "CanViewAnything")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage", "CanViewAnything");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            ClaimsPrincipal user = null;

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfUserIsNotAuthenticated()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldAllowWithNoAuthType()
        {
            // Arrange
            var policy = new AuthorizationPolicy().Requires("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfUnknownPolicy()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_CustomRolePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicy()
                .Requires(ClaimTypes.Role, "Administrator")
                .Requires(ClaimTypes.Role, "User");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Role, "Administrator")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_HasAnyClaimOfTypePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicy()
                .RequiresAny(ClaimTypes.Role);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, ""),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_PolicyRequiresAuthenticationTypeWithNameClaim()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .RequiresAny(ClaimTypes.Name);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }
    }
}