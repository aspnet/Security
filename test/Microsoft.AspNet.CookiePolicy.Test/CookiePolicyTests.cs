// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.CookiePolicy.Test
{
    public class CookiePolicyTests
    {
        [Fact]
        public async Task CookiePolicySecureAlwaysSetsSecure()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.Secure = SecurePolicy.Always);
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/; secure", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/; secure", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/; secure", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; secure", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CanMapTwoCookiePolicies()
        {
            var server = TestServer.Create(app =>
            {
                app.Map("/always", map =>
                {
                    map.UseCookiePolicy(options => options.Secure = SecurePolicy.Always);
                    map.Run(context =>
                    {
                        context.Response.Cookies.Append("A", "A");
                        context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                        context.Response.Cookies.Append("C", "C", new CookieOptions());
                        context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                        return Task.FromResult(0);
                    });
                });
                app.Map("/none", map =>
                {
                    map.UseCookiePolicy(options => options.Secure = SecurePolicy.None);
                    map.Run(context =>
                    {
                        context.Response.Cookies.Append("A", "A");
                        context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                        context.Response.Cookies.Append("C", "C", new CookieOptions());
                        context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                        return Task.FromResult(0);
                    });
                });

            });

            var transaction = await server.SendAsync("http://example.com/Always");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/; secure", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/; secure", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/; secure", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; secure", transaction.SetCookie[3]);

            transaction = await server.SendAsync("http://example.com/None");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; secure", transaction.SetCookie[3]);

        }


        [Fact]
        public async Task CookiePolicySecureNoneLeavesSecureAlone()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.Secure = SecurePolicy.None);
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; secure", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CookiePolicySecureSameAsRequestIsFalseOnHttp()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.Secure = SecurePolicy.SameAsRequest);
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CookiePolicySecureSameAsRequestIsTrueOnHttps()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.Secure = SecurePolicy.SameAsRequest);
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("https://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/; secure", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/; secure", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/; secure", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; secure", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CookiePolicyHttpOnlyAlwaysSetsHttpOnly()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.HttpOnly = HttpOnlyPolicy.Always);
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { HttpOnly = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { HttpOnly = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/; httponly", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/; httponly", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/; httponly", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; httponly", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CookiePolicyHttpOnlyNoneLeavesItAlone()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.HttpOnly = HttpOnlyPolicy.None);
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { HttpOnly = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { HttpOnly = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("A=A; path=/", transaction.SetCookie[0]);
            Assert.Equal("B=B; path=/", transaction.SetCookie[1]);
            Assert.Equal("C=C; path=/", transaction.SetCookie[2]);
            Assert.Equal("D=D; path=/; httponly", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CookiePolicyCanHijackAppend()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.OnAppendCookie = ctx => ctx.CookieName = ctx.CookieValue = "Hao");
                app.Run(context =>
                {
                    context.Response.Cookies.Append("A", "A");
                    context.Response.Cookies.Append("B", "B", new CookieOptions { Secure = false });
                    context.Response.Cookies.Append("C", "C", new CookieOptions());
                    context.Response.Cookies.Append("D", "D", new CookieOptions { Secure = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal("Hao=Hao; path=/", transaction.SetCookie[0]);
            Assert.Equal("Hao=Hao; path=/", transaction.SetCookie[1]);
            Assert.Equal("Hao=Hao; path=/", transaction.SetCookie[2]);
            Assert.Equal("Hao=Hao; path=/; secure", transaction.SetCookie[3]);
        }

        [Fact]
        public async Task CookiePolicyCanHijackDelete()
        {
            var server = TestServer.Create(app =>
            {
                app.UseCookiePolicy(options => options.OnDeleteCookie = ctx => ctx.CookieName = "A");
                app.Run(context =>
                {
                    context.Response.Cookies.Delete("A");
                    context.Response.Cookies.Delete("B", new CookieOptions { Secure = false });
                    context.Response.Cookies.Delete("C", new CookieOptions());
                    context.Response.Cookies.Delete("D", new CookieOptions { Secure = true });
                    return Task.FromResult(0);
                });
            });

            var transaction = await server.SendAsync("http://example.com/login");

            Assert.NotNull(transaction.SetCookie);
            Assert.Equal(2, transaction.SetCookie.Count);
            Assert.Equal("A=; expires=Thu, 01-Jan-1970 00:00:00 GMT", transaction.SetCookie[0]);
            Assert.Equal("A=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/", transaction.SetCookie[1]);
        }

    }
}