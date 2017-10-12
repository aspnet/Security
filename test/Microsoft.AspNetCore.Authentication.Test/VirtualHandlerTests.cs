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

namespace Microsoft.AspNetCore.Authentication
{
    public class VirtualHandlerTests
    {
        [Fact]
        public async Task CanDispatch()
        {
            var server = CreateServer(auth =>
            {
                auth.AddVirtualScheme("policy1", "policy1", p =>
                    {
                        p.DefaultTarget = "auth1";
                    })
                    .AddVirtualScheme("policy2", "policy2", p =>
                    {
                        p.AuthenticateTarget = "auth2";
                    })
                    .AddScheme<TestOptions, TestHandler>("auth1", o => { })
                    .AddScheme<TestOptions, TestHandler>("auth2", o => { })
                    .AddScheme<TestOptions, TestHandler>("auth3", o => { });
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
        public async Task CanDynamicTargetBasedOnQueryString()
        {
            var server = CreateServer(auth =>
            {
                auth.AddVirtualScheme("dynamic", "dynamic", p =>
                {
                    p.DefaultTargetSelector = c => c.Request.QueryString.Value.Substring(1);
                })
                    .AddScheme<TestOptions, TestHandler>("auth1", o => { })
                    .AddScheme<TestOptions, TestHandler>("auth2", o => { })
                    .AddScheme<TestOptions, TestHandler>("auth3", o => { });
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
            var server = CreateServer(auth =>
            {
                auth.AddVirtualScheme("virtual", "virtual", p => { })
                    .AddScheme<TestOptions, TestHandler>("default", o => { });
            }, "default");

            var transaction = await server.SendAsync("http://example.com/auth/virtual");
            Assert.Equal("default", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "default"));
        }

        [Fact]
        public async Task TargetsDefaultSchemeThrowsWithNoDefault()
        {
            var server = CreateServer(auth =>
            {
                auth.AddVirtualScheme("virtual", "virtual", p => { })
                    .AddScheme<TestOptions, TestHandler>("default", o => { });
            });

            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => server.SendAsync("http://example.com/auth/virtual"));
            Assert.Contains("No authenticationScheme was specified", error.Message);
        }

        // TODO: test other verbs

        public class TestOptions : AuthenticationSchemeOptions
        {
        }

        private class TestHandler : AuthenticationHandler<TestOptions>
        {
            public TestHandler(IOptionsMonitor<TestOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var principal = new ClaimsPrincipal();
                var id = new ClaimsIdentity();
                id.AddClaim(new Claim(ClaimTypes.NameIdentifier, Scheme.Name, ClaimValueTypes.String, Scheme.Name));
                principal.AddIdentity(id);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name)));
            }
        }

        private static TestServer CreateServer(Action<AuthenticationBuilder> configureAuth = null, string defaultScheme = null)
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
                    var auth = services.AddAuthentication(defaultScheme);
                    configureAuth?.Invoke(auth);
                });
            return new TestServer(builder);
        }
    }
}
