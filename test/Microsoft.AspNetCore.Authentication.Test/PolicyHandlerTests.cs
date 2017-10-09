// Copyright (c) .NET Foundation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
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
    public class PolicyHandlerTests
    {
        [Fact]
        public async Task CanDispatchViaPolicy()
        {
            var server = CreateServer(auth =>
            {
                auth.AddPolicyScheme("policy1", "policy1", p =>
                {
                    p.DefaultScheme = "auth1";
                });
                auth.AddPolicyScheme("policy2", "policy2", p =>
                {
                    p.AuthenticateScheme = "auth2";
                });
                auth.AddPolicyScheme("dynamic", "dynamic", p =>
                {
                    p.DefaultSchemeSelector = c => c.Request.QueryString.Value.Substring(1);
                });

                auth.AddScheme<TestOptions, TestHandler>("auth1", o => { });
                auth.AddScheme<TestOptions, TestHandler>("auth2", o => { });
                auth.AddScheme<TestOptions, TestHandler>("auth3", o => { });
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() => server.SendAsync("http://example.com/auth/One"));

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

            transaction = await server.SendAsync("http://example.com/auth/dynamic?auth1");
            Assert.Equal("auth1", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth1"));
            transaction = await server.SendAsync("http://example.com/auth/dynamic?auth2");
            Assert.Equal("auth2", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth2"));
            transaction = await server.SendAsync("http://example.com/auth/dynamic?auth3");
            Assert.Equal("auth3", transaction.FindClaimValue(ClaimTypes.NameIdentifier, "auth3"));

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

        private static TestServer CreateServer(Action<AuthenticationBuilder> configureAuth = null)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseAuthentication();
                    app.Use(async (context, next) =>
                    {
                        var req = context.Request;
                        var res = context.Response;
                        if (req.Path.StartsWithSegments(new PathString("/add"), out var remainder))
                        {
                            var name = remainder.Value.Substring(1);
                            var auth = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                            var scheme = new AuthenticationScheme(name, name, typeof(TestHandler));
                            auth.AddScheme(scheme);
                        }
                        else if (req.Path.StartsWithSegments(new PathString("/auth"), out remainder))
                        {
                            var name = (remainder.Value.Length > 0) ? remainder.Value.Substring(1) : null;
                            var result = await context.AuthenticateAsync(name);
                            res.Describe(result?.Ticket?.Principal);
                        }
                        else if (req.Path.StartsWithSegments(new PathString("/remove"), out remainder))
                        {
                            var name = remainder.Value.Substring(1);
                            var auth = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                            auth.RemoveScheme(name);
                        }
                        else
                        {
                            await next();
                        }
                    });
                })
                .ConfigureServices(services =>
                {
                    var auth = services.AddAuthentication();
                    configureAuth?.Invoke(auth);
                });
            return new TestServer(builder);
        }
    }
}
