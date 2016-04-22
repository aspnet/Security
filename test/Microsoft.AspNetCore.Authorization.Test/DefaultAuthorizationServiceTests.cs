// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Authorization.Test
{
    public class DefaultAuthorizationServiceTests
    {
        private IAuthorizationService BuildAuthorizationService(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection();
            services.AddAuthorization();
            services.AddLogging();
            services.AddOptions();
            if (setupServices != null)
            {
                setupServices(services);
            }
            return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
        }

        [Fact]
        public void AuthorizeCombineThrowsOnUnknownPolicy()
        {
            Assert.Throws<InvalidOperationException>(() => AuthorizationPolicy.Combine(new AuthorizationOptions(), new AuthorizeAttribute[] {
                new AuthorizeAttribute { Policy = "Wut" }
            }));
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsPresent()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsPresentWithSpecifiedAuthType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => {
                        policy.AddAuthenticationSchemes("Basic");
                        policy.RequireClaim("Permission", "CanViewPage");
                    });
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage", "CanViewAnything"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim("Permission", "CanViewAnything")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldFailWhenAllRequirementsNotHandled()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage", "CanViewAnything"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage", "CanViewAnything"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage"));
                });
            });

            // Act
            var allowed = await authorizationService.AuthorizeAsync(null, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNotCorrectAuthType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowWithNoAuthType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ThrowsWithUnknownPolicy()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService();

            // Act
            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => authorizationService.AuthorizeAsync(new ClaimsPrincipal(), "whatever", "BogusPolicy"));
            Assert.Equal("No policy found: BogusPolicy.", exception.Message);
        }

        [Fact]
        public async Task Authorize_CustomRolePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequireRole("Administrator")
                .RequireClaim(ClaimTypes.Role, "User");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Role, "Administrator")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_HasAnyClaimOfTypePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequireClaim(ClaimTypes.Role);
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "none"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_PolicyCanAuthenticationSchemeWithNameClaim()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("AuthType").RequireClaim(ClaimTypes.Name);
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Name") }, "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireSingleRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("AuthType").RequireRole("Admin");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Admin") }, "AuthType")
            );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireOneOfManyRoles()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("AuthType").RequireRole("Admin", "Users");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Users") }, "AuthType"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockWrongRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequireClaim("Permission", "CanViewPage");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Nope"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, policy.Build());

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockNoRole()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireRole("Admin", "Users"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public void PolicyThrowsWithNoRequirements()
        {
            Assert.Throws<InvalidOperationException>(() => BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => { });
                });
            }));
        }

        [Fact]
        public async Task RequireUserNameFailsForWrongUserName()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Hao", policy => policy.RequireUserName("Hao"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Tek"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Hao");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task CanRequireUserName()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Hao", policy => policy.RequireUserName("Hao"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Hao"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Hao");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanRequireUserNameWithDiffClaimType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Hao", policy => policy.RequireUserName("Hao"));
                });
            });
            var identity = new ClaimsIdentity("AuthType", "Name", "Role");
            identity.AddClaim(new Claim("Name", "Hao"));
            var user = new ClaimsPrincipal(identity);

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Hao");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanRequireRoleWithDiffClaimType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Hao", policy => policy.RequireRole("Hao"));
                });
            });
            var identity = new ClaimsIdentity("AuthType", "Name", "Role");
            identity.AddClaim(new Claim("Role", "Hao"));
            var user = new ClaimsPrincipal(identity);

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Hao");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanApproveAnyAuthenticatedUser()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Any", policy => policy.RequireAuthenticatedUser());
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            user.AddIdentity(new ClaimsIdentity(
                new Claim[] {
                    new Claim(ClaimTypes.Name, "Name"),
                },
                "AuthType"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Any");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanBlockNonAuthenticatedUser()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Any", policy => policy.RequireAuthenticatedUser());
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Any");

            // Assert
            Assert.False(allowed);
        }

        public class CustomRequirement : IAuthorizationRequirement { }
        public class CustomHandler : AuthorizationHandler<CustomRequirement>
        {
            protected override void Handle(AuthorizationContext context, CustomRequirement requirement)
            {
                context.Succeed(requirement);
            }
        }

        [Fact]
        public async Task CustomReqWithNoHandlerFails()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Custom", policy => policy.Requirements.Add(new CustomRequirement()));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Custom");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task CustomReqWithHandlerSucceeds()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddTransient<IAuthorizationHandler, CustomHandler>();
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Custom", policy => policy.Requirements.Add(new CustomRequirement()));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Custom");

            // Assert
            Assert.True(allowed);
        }

        public class PassThroughRequirement : AuthorizationHandler<PassThroughRequirement>, IAuthorizationRequirement
        {
            public PassThroughRequirement(bool succeed)
            {
                Succeed = succeed;
            }

            public bool Succeed { get; set; }

            protected override void Handle(AuthorizationContext context, PassThroughRequirement requirement)
            {
                if (Succeed) {
                    context.Succeed(requirement);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PassThroughRequirementWillSucceedWithoutCustomHandler(bool shouldSucceed)
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Passthrough", policy => policy.Requirements.Add(new PassThroughRequirement(shouldSucceed)));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Passthrough");

            // Assert
            Assert.Equal(shouldSucceed, allowed);
        }

        [Fact]
        public async Task CanCombinePolicies()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    var basePolicy = new AuthorizationPolicyBuilder().RequireClaim("Base", "Value").Build();
                    options.AddPolicy("Combined", policy => policy.Combine(basePolicy).RequireClaim("Claim", "Exists"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Base", "Value"),
                        new Claim("Claim", "Exists")
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Combined");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CombinePoliciesWillFailIfBasePolicyFails()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    var basePolicy = new AuthorizationPolicyBuilder().RequireClaim("Base", "Value").Build();
                    options.AddPolicy("Combined", policy => policy.Combine(basePolicy).RequireClaim("Claim", "Exists"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Claim", "Exists")
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Combined");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task CombinedPoliciesWillFailIfExtraRequirementFails()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    var basePolicy = new AuthorizationPolicyBuilder().RequireClaim("Base", "Value").Build();
                    options.AddPolicy("Combined", policy => policy.Combine(basePolicy).RequireClaim("Claim", "Exists"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Base", "Value"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Combined");

            // Assert
            Assert.False(allowed);
        }

        public class ExpenseReport { }

        public static class Operations
        {
            public static OperationAuthorizationRequirement Edit = new OperationAuthorizationRequirement { Name = "Edit" };
            public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = "Create" };
            public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = "Delete" };
        }

        public class ExpenseReportAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, ExpenseReport>
        {
            public ExpenseReportAuthorizationHandler(IEnumerable<OperationAuthorizationRequirement> authorized)
            {
                _allowed = authorized;
            }

            private IEnumerable<OperationAuthorizationRequirement> _allowed;

            protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement, ExpenseReport resource)
            {
                if (_allowed.Contains(requirement))
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class SuperUserHandler : AuthorizationHandler<OperationAuthorizationRequirement>
        {
            protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement)
            {
                var user = context.AuthorizationData as ClaimsPrincipal;
                if (user == null || user.HasClaim("SuperUser", "yes"))
                {
                    context.Succeed(requirement);
                }
            }
        }

        [Fact]
        public async Task CanAuthorizeAllSuperuserOperations()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddSingleton<IAuthorizationHandler>(new ExpenseReportAuthorizationHandler(new OperationAuthorizationRequirement[] { Operations.Edit }));
                services.AddTransient<IAuthorizationHandler, SuperUserHandler>();
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SuperUser", "yes"),
                    },
                    "AuthType")
                );

            // Act
            // Assert
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Edit));
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Delete));
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Create));
        }

        public class NotCalledHandler : AuthorizationHandler<OperationAuthorizationRequirement, string>
        {
            protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement, string resource)
            {
                throw new NotImplementedException();
            }
        }

        public class EvenHandler : AuthorizationHandler<OperationAuthorizationRequirement, int>
        {
            protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement, int id)
            {
                if (id % 2 == 0)
                {
                    context.Succeed(requirement);
                }
            }
        }

        [Fact]
        public async Task CanUseValueTypeResource()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddTransient<IAuthorizationHandler, EvenHandler>();
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                    },
                    "AuthType")
                );

            // Act
            // Assert
            Assert.False(await authorizationService.AuthorizeAsync(user, 1, Operations.Edit));
            Assert.True(await authorizationService.AuthorizeAsync(user, 2, Operations.Edit));
        }


        [Fact]
        public async Task DoesNotCallHandlerWithWrongResourceType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddTransient<IAuthorizationHandler, NotCalledHandler>();
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SuperUser", "yes")
                    },
                    "AuthType")
                );

            // Act
            // Assert
            Assert.False(await authorizationService.AuthorizeAsync(user, 1, Operations.Edit));
        }

        [Fact]
        public async Task CanAuthorizeOnlyAllowedOperations()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddSingleton<IAuthorizationHandler>(new ExpenseReportAuthorizationHandler(new OperationAuthorizationRequirement[] { Operations.Edit }));
            });
            var user = new ClaimsPrincipal();

            // Act
            // Assert
            Assert.True(await authorizationService.AuthorizeAsync(user, new ExpenseReport(), Operations.Edit));
            Assert.False(await authorizationService.AuthorizeAsync(user, new ExpenseReport(), Operations.Delete));
            Assert.False(await authorizationService.AuthorizeAsync(user, new ExpenseReport(), Operations.Create));
        }

        [Fact]
        public async Task AuthorizeHandlerNotCalledWithNullResource()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddSingleton<IAuthorizationHandler>(new ExpenseReportAuthorizationHandler(new OperationAuthorizationRequirement[] { Operations.Edit }));
            });
            var user = new ClaimsPrincipal();

            // Act
            // Assert
            Assert.False(await authorizationService.AuthorizeAsync(user, null, Operations.Edit));
        }

        [Fact]
        public async Task CanAuthorizeWithAssertionRequirement()
        {
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireAssertion(context => true));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanAuthorizeWithAsyncAssertionRequirement()
        {
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireAssertion(context => Task.FromResult(true)));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.True(allowed);
        }

        public class StaticPolicyProvider : IAuthorizationPolicyProvider
        {
            public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
            {
                return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
            }
        }

        [Fact]
        public async Task CanReplaceDefaultPolicyProvider()
        {
            var authorizationService = BuildAuthorizationService(services =>
            {
                // This will ignore the policy options
                services.AddSingleton<IAuthorizationPolicyProvider, StaticPolicyProvider>();
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequireAssertion(context => true));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "Basic");

            // Assert
            Assert.False(allowed);
        }

        public class DynamicPolicyProvider : IAuthorizationPolicyProvider
        {
            public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
            {
                return Task.FromResult(new AuthorizationPolicyBuilder().RequireClaim(policyName).Build());
            }
        }

        [Fact]
        public async Task CanUseDynamicPolicyProvider()
        {
            var authorizationService = BuildAuthorizationService(services =>
            {
                // This will ignore the policy options
                services.AddSingleton<IAuthorizationPolicyProvider, DynamicPolicyProvider>();
                services.AddAuthorization(options => { });
            });
            var id = new ClaimsIdentity();
            id.AddClaim(new Claim("1", "1"));
            id.AddClaim(new Claim("2", "2"));
            var user = new ClaimsPrincipal(id);

            // Act
            // Assert
            Assert.False(await authorizationService.AuthorizeAsync(user, "0"));
            Assert.True(await authorizationService.AuthorizeAsync(user, "1"));
            Assert.True(await authorizationService.AuthorizeAsync(user, "2"));
            Assert.False(await authorizationService.AuthorizeAsync(user, "3"));
        }
    }
}