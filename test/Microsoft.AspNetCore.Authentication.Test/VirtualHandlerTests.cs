// Copyright (c) .NET Foundation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.Tests
{
    public class VirtualHandlerTests
    {
        [Fact]
        public async Task CanDispatch()
        {
            var server = CreateServer(services =>
            {
                services.AddAuthentication(o =>
                {
                    o.AddScheme<TestHandler>("auth1", "auth1");
                    o.AddScheme<TestHandler>("auth2", "auth2");
                    o.AddScheme<TestHandler>("auth3", "auth3");
                })
                .AddScheme("policy1", "policy1", p =>
                {
                    p.ForwardDefault = "auth1";
                })
                .AddScheme("policy2", "policy2", p =>
                {
                    p.ForwardAuthenticate = "auth2";
                });
            });

            var transaction = await server.SendAsync("http://example.com/auth/policy1");
            Assert.Equal("auth1", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth1"));

            transaction = await server.SendAsync("http://example.com/auth/auth1");
            Assert.Equal("auth1", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth1"));

            transaction = await server.SendAsync("http://example.com/auth/auth2");
            Assert.Equal("auth2", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth2"));

            transaction = await server.SendAsync("http://example.com/auth/auth3");
            Assert.Equal("auth3", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth3"));

            transaction = await server.SendAsync("http://example.com/auth/policy2");
            Assert.Equal("auth2", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth2"));
        }

        [Fact]
        public async Task DefaultTargetSelectorWinsOverDefaultTarget()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
                o.AddScheme<TestHandler2>("auth2", "auth2");
            })
            .AddScheme("forward", "forward", p => {
                p.ForwardDefault = "auth2";
                p.ForwardDefaultSelector = ctx => "auth1";
            });

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);
            var handler2 = new TestHandler2();
            services.AddSingleton(handler2);

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);
            Assert.Equal(0, handler2.AuthenticateCount);
            Assert.Equal(0, handler2.ForbidCount);
            Assert.Equal(0, handler2.ChallengeCount);
            Assert.Equal(0, handler2.SignInCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.AuthenticateAsync("forward");
            Assert.Equal(1, handler1.AuthenticateCount);
            Assert.Equal(0, handler2.AuthenticateCount);

            await context.ForbidAsync("forward");
            Assert.Equal(1, handler1.ForbidCount);
            Assert.Equal(0, handler2.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(1, handler1.ChallengeCount);
            Assert.Equal(0, handler2.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(1, handler1.SignOutCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(1, handler1.SignInCount);
            Assert.Equal(0, handler2.SignInCount);
        }

        [Fact]
        public async Task NullDefaultTargetSelectorFallsBacktoDefaultTarget()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
                o.AddScheme<TestHandler2>("auth2", "auth2");
            })
            .AddScheme("forward", "forward", p => {
                p.ForwardDefault = "auth1";
                p.ForwardDefaultSelector = ctx => null;
            });

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);
            var handler2 = new TestHandler2();
            services.AddSingleton(handler2);

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);
            Assert.Equal(0, handler2.AuthenticateCount);
            Assert.Equal(0, handler2.ForbidCount);
            Assert.Equal(0, handler2.ChallengeCount);
            Assert.Equal(0, handler2.SignInCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.AuthenticateAsync("forward");
            Assert.Equal(1, handler1.AuthenticateCount);
            Assert.Equal(0, handler2.AuthenticateCount);

            await context.ForbidAsync("forward");
            Assert.Equal(1, handler1.ForbidCount);
            Assert.Equal(0, handler2.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(1, handler1.ChallengeCount);
            Assert.Equal(0, handler2.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(1, handler1.SignOutCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(1, handler1.SignInCount);
            Assert.Equal(0, handler2.SignInCount);
        }

        [Fact]
        public async Task SpecificTargetAlwaysWinsOverDefaultTarget()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
                o.AddScheme<TestHandler2>("auth2", "auth2");
            })
            .AddScheme("forward", "forward", p => {
                p.ForwardDefault = "auth2";
                p.ForwardDefaultSelector = ctx => "auth2";
                p.ForwardAuthenticate = "auth1";
                p.ForwardSignIn = "auth1";
                p.ForwardSignOut = "auth1";
                p.ForwardForbid = "auth1";
                p.ForwardChallenge = "auth1";
            });

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);
            var handler2 = new TestHandler2();
            services.AddSingleton(handler2);

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);
            Assert.Equal(0, handler2.AuthenticateCount);
            Assert.Equal(0, handler2.ForbidCount);
            Assert.Equal(0, handler2.ChallengeCount);
            Assert.Equal(0, handler2.SignInCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.AuthenticateAsync("forward");
            Assert.Equal(1, handler1.AuthenticateCount);
            Assert.Equal(0, handler2.AuthenticateCount);

            await context.ForbidAsync("forward");
            Assert.Equal(1, handler1.ForbidCount);
            Assert.Equal(0, handler2.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(1, handler1.ChallengeCount);
            Assert.Equal(0, handler2.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(1, handler1.SignOutCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(1, handler1.SignInCount);
            Assert.Equal(0, handler2.SignInCount);
        }

        [Fact]
        public async Task VirtualSchemeTargetsForwardWithDefaultTarget()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
                o.AddScheme<TestHandler2>("auth2", "auth2");
            })
            .AddScheme("forward", "forward", p => p.ForwardDefault = "auth1");

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);
            var handler2 = new TestHandler2();
            services.AddSingleton(handler2);

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);
            Assert.Equal(0, handler2.AuthenticateCount);
            Assert.Equal(0, handler2.ForbidCount);
            Assert.Equal(0, handler2.ChallengeCount);
            Assert.Equal(0, handler2.SignInCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.AuthenticateAsync("forward");
            Assert.Equal(1, handler1.AuthenticateCount);
            Assert.Equal(0, handler2.AuthenticateCount);

            await context.ForbidAsync("forward");
            Assert.Equal(1, handler1.ForbidCount);
            Assert.Equal(0, handler2.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(1, handler1.ChallengeCount);
            Assert.Equal(0, handler2.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(1, handler1.SignOutCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(1, handler1.SignInCount);
            Assert.Equal(0, handler2.SignInCount);
        }

        [Fact]
        public async Task TargetsSelfResultsInFallbackToDefault()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
                o.DefaultScheme = "auth1";
            })
            .AddScheme("forward", "forward", p =>
            {
                p.ForwardDefault = "forward";
            });

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);

            await context.AuthenticateAsync("forward");
            Assert.Equal(1, handler1.AuthenticateCount);

            await context.ForbidAsync("forward");
            Assert.Equal(1, handler1.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(1, handler1.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(1, handler1.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(1, handler1.SignInCount);
        }

        [Fact]
        public async Task VirtualSchemeTargetsOverrideDefaultTarget()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
                o.AddScheme<TestHandler2>("auth2", "auth2");
            })
            .AddScheme("forward", "forward", p =>
            {
                p.ForwardDefault = "auth1";
                p.ForwardChallenge = "auth2";
                p.ForwardSignIn = "auth2";
            });

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);
            var handler2 = new TestHandler2();
            services.AddSingleton(handler2);

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);
            Assert.Equal(0, handler2.AuthenticateCount);
            Assert.Equal(0, handler2.ForbidCount);
            Assert.Equal(0, handler2.ChallengeCount);
            Assert.Equal(0, handler2.SignInCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.AuthenticateAsync("forward");
            Assert.Equal(1, handler1.AuthenticateCount);
            Assert.Equal(0, handler2.AuthenticateCount);

            await context.ForbidAsync("forward");
            Assert.Equal(1, handler1.ForbidCount);
            Assert.Equal(0, handler2.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(1, handler2.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(1, handler1.SignOutCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(1, handler2.SignInCount);
        }

        [Fact]
        public async Task AuthenticationHandlerCanTargetSelfOverridingForwardDefault()
        {
            var services = new ServiceCollection().AddOptions().AddLogging();

            services.AddAuthentication(o =>
            {
                o.AddScheme<TestHandler>("auth1", "auth1");
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("forward", "forward", p =>
            {
                p.ForwardDefault = "auth1";
                p.ForwardAuthenticate = "forward";
                p.ForwardChallenge = "forward";
                p.ForwardForbid = "forward";
                p.ForwardSignOut = "forward";
                p.ForwardSignIn = "forward";
            });

            var handler1 = new TestHandler();
            services.AddSingleton(handler1);
            services.AddSingleton<TestAuthHandler>();

            var sp = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = sp;

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(0, handler1.SignOutCount);

            await context.AuthenticateAsync("forward");
            var handler2 = sp.GetRequiredService<TestAuthHandler>();

            Assert.Equal(0, handler1.AuthenticateCount);
            Assert.Equal(1, handler2.AuthenticateCount);
            Assert.Equal(0, handler2.ForbidCount);
            Assert.Equal(0, handler2.ChallengeCount);
            Assert.Equal(0, handler2.SignInCount);
            Assert.Equal(0, handler2.SignOutCount);

            await context.ForbidAsync("forward");
            Assert.Equal(0, handler1.ForbidCount);
            Assert.Equal(1, handler2.ForbidCount);

            await context.ChallengeAsync("forward");
            Assert.Equal(0, handler1.ChallengeCount);
            Assert.Equal(1, handler2.ChallengeCount);

            await context.SignOutAsync("forward");
            Assert.Equal(0, handler1.SignOutCount);
            Assert.Equal(1, handler2.SignOutCount);

            await context.SignInAsync("forward", new ClaimsPrincipal());
            Assert.Equal(0, handler1.SignInCount);
            Assert.Equal(1, handler2.SignInCount);
        }

        [Fact]
        public async Task CanDynamicTargetBasedOnQueryString()
        {
            var server = CreateServer(services =>
            {
                services.AddAuthentication(o =>
                {
                    o.AddScheme<TestHandler>("auth1", "auth1");
                    o.AddScheme<TestHandler>("auth2", "auth2");
                    o.AddScheme<TestHandler>("auth3", "auth3");
                })
                .AddScheme("dynamic", "dynamic", p =>
                {
                    p.ForwardDefaultSelector = c => c.Request.QueryString.Value.Substring(1);
                });
            });
 
            var transaction = await server.SendAsync("http://example.com/auth/dynamic?auth1");
            Assert.Equal("auth1", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth1"));
            transaction = await server.SendAsync("http://example.com/auth/dynamic?auth2");
            Assert.Equal("auth2", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth2"));
            transaction = await server.SendAsync("http://example.com/auth/dynamic?auth3");
            Assert.Equal("auth3", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth3"));
        }

        [Fact]
        public async Task TargetsDefaultSchemeByDefault()
        {
            var server = CreateServer(services =>
            {
                services.AddAuthentication(o =>
                {
                    o.DefaultScheme = "default";
                    o.AddScheme<TestHandler>("default", "default");
                })
                .AddScheme("virtual", "virtual", p => { });
            });

            var transaction = await server.SendAsync("http://example.com/auth/virtual");
            Assert.Equal("default", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "default"));
        }

        [Fact]
        public async Task TargetsDefaultSchemeThrowsWithNoDefault()
        {
            var server = CreateServer(services =>
            {
                services.AddAuthentication(o =>
                {
                    o.AddScheme<TestHandler>("default", "default");
                })
                .AddScheme("virtual", "virtual", p => { });
            });

            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => server.SendAsync("http://example.com/auth/virtual"));
            Assert.Contains("No authenticationScheme was specified", error.Message);
        }

        private class TestAuthHandler : SignInAuthenticationHandler<AuthenticationSchemeOptions>
        {
            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
            { }

            public int SignInCount { get; set; }
            public int SignOutCount { get; set; }
            public int ForbidCount { get; set; }
            public int ChallengeCount { get; set; }
            public int AuthenticateCount { get; set; }

            protected override Task HandleChallengeAsync(AuthenticationProperties properties)
            {
                ChallengeCount++;
                return Task.CompletedTask;
            }

            protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
            {
                ForbidCount++;
                return Task.CompletedTask;
            }

            protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            {
                SignInCount++;
                return Task.CompletedTask;
            }

            protected override Task HandleSignOutAsync(AuthenticationProperties properties)
            {
                SignOutCount++;
                return Task.CompletedTask;
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                AuthenticateCount++;
                var principal = new ClaimsPrincipal();
                var id = new ClaimsIdentity();
                id.AddClaim(new Claim(ClaimTypes.NameIdentifier, Scheme.Name, ClaimValueTypes.String, Scheme.Name));
                principal.AddIdentity(id);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name)));
            }
        }

        private class TestHandler : IAuthenticationSignInHandler
        {
            public AuthenticationScheme Scheme { get; set; }
            public int SignInCount { get; set; }
            public int SignOutCount { get; set; }
            public int ForbidCount { get; set; }
            public int ChallengeCount { get; set; }
            public int AuthenticateCount { get; set; }

            public Task<AuthenticateResult> AuthenticateAsync()
            {
                AuthenticateCount++;
                var principal = new ClaimsPrincipal();
                var id = new ClaimsIdentity();
                id.AddClaim(new Claim(ClaimTypes.NameIdentifier, Scheme.Name, ClaimValueTypes.String, Scheme.Name));
                principal.AddIdentity(id);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name)));
            }

            public Task ChallengeAsync(AuthenticationProperties properties)
            {
                ChallengeCount++;
                return Task.CompletedTask;
            }

            public Task ForbidAsync(AuthenticationProperties properties)
            {
                ForbidCount++;
                return Task.CompletedTask;
            }

            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                Scheme = scheme;
                return Task.CompletedTask;
            }

            public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            {
                SignInCount++;
                return Task.CompletedTask;
            }

            public Task SignOutAsync(AuthenticationProperties properties)
            {
                SignOutCount++;
                return Task.CompletedTask;
            }
        }

        private class TestHandler2 : TestHandler { }

        private static TestServer CreateServer(Action<IServiceCollection> configure = null, string defaultScheme = null)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseAuthentication();
                    app.Use(async (context, next) =>
                    {
                        var req = context.Request;
                        var res = context.Response;
                        if (req.Path.StartsWithSegments(new PathString("/auth"), out var remainder))
                        {
                            var name = (remainder.Value.Length > 0) ? remainder.Value.Substring(1) : null;
                            var result = await context.AuthenticateAsync(name);
                            res.Describe(result?.Ticket?.Principal);
                        }
                        else
                        {
                            await next();
                        }
                    });
                })
                .ConfigureServices(services =>
                {
                    configure?.Invoke(services);
                });
            return new TestServer(builder);
        }
    }
}
