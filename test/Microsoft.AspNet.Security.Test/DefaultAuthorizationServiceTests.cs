// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
        public async Task Authorize_ShouldAllowIfClaimIsPresent()
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
        public async Task Authorize_ShouldAllowIfClaimIsAmongValues()
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
        public async Task Authorize_ShouldNotAllowIfClaimTypeIsNotPresent()
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
        public async Task Authorize_ShouldNotAllowIfClaimValueIsNotPresent()
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
        public async Task Authorize_ShouldNotAllowIfNoClaims()
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
        public async Task Authorize_ShouldNotAllowIfUserIsNull()
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
        public async Task Authorize_ShouldNotAllowIfUserIsNotAuthenticated()
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
        public async Task Authorize_ShouldAllowWithNoAuthType()
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
        public async Task Authorize_ShouldNotAllowIfUnknownPolicy()
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
        public async Task Authorize_CustomRolePolicy()
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
        public async Task Authorize_HasAnyClaimOfTypePolicy()
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
        public async Task Authorize_PolicyRequiresAuthenticationTypeWithNameClaim()
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

        [Fact]
        public async Task RolePolicyCanRequireSingleRole()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Admin"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireOneOfManyRoles()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Users"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockWrongRole()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Nope"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockNoRole()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        private class GrumpyPolicyHandler : IAuthorizationPolicyHandler
        {
            public Task<bool> AuthorizeAsync(AuthorizationContext context)
            {
                return Task.FromResult(false);
            }
        }

        [Fact]
        public async Task CustomPolicyCanBlock()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .RequiresAny(ClaimTypes.Name)
                .AddHandler(new GrumpyPolicyHandler());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object);
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
            Assert.False(allowed);
        }

        [Fact]
        public async Task PolicyHandlerCanApproveWithNoDefaultHandler()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .RequiresAny(ClaimTypes.Name)
                .AddHandler(new DefaultAuthoriziationPolicyHandler());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, new List<IAuthorizationPolicyHandler>());
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

        [Fact]
        public async Task AlwaysApproveWithNoPolicyHandlers()
        {
            // Arrange
            var policy = new AuthorizationPolicy("TotallyBogus")
                .RequiresAny(ClaimTypes.Name);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, new List<IAuthorizationPolicyHandler>());
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }
    }
}