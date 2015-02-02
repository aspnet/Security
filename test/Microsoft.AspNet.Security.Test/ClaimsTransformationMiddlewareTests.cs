// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Security
{
    public class ClaimsTransformationMiddlewareTests
    {
        [Fact]
        public async Task CanReplaceUser()
        {
            var newUser = new ClaimsPrincipal();
            var server = CreateServer(options => options.TransformAsync = principal => Task.FromResult(newUser),
                context =>
                {
                    Assert.Same(context.User, newUser);
                    return Task.FromResult<object>(null);
                });
            Transaction transaction1 = await SendAsync(server, "http://example.com/base/testpath");
        }

        [Fact]
        public async Task CanAddIdentityToUser()
        {
            var server = CreateServer(options => options.TransformAsync = 
                principal =>
                {
                    principal.AddIdentity(new ClaimsIdentity("newauthtype"));
                    return Task.FromResult(principal);
                },
                context =>
                {
                    Assert.True(context.User.Identities.Any(i => i.AuthenticationType == "newauthtype"));
                    return Task.FromResult<object>(null);
                });
            Transaction transaction1 = await SendAsync(server, "http://example.com/base/testpath");
        }

        [Fact]
        public async Task CanNullOutIdentity()
        {
            var server = CreateServer(options => options.TransformAsync =
                principal =>
                {
                    return Task.FromResult<ClaimsPrincipal>(null);
                },
                context =>
                {
                    Assert.Null(context.User);
                    return Task.FromResult<object>(null);
                });
            Transaction transaction1 = await SendAsync(server, "http://example.com/base/testpath");
        }

        private static TestServer CreateServer(Action<ClaimsTransformationOptions> configureOptions, Func<HttpContext, Task> testpath = null, Uri baseAddress = null)
        {
            var server = TestServer.Create(app =>
            {
                app.UseServices(services => services.AddOptions());
                app.UseClaimsTransformation(configureOptions);
                app.Use(async (context, next) =>
                {
                    var req = context.Request;
                    if (req.Path == new PathString("/testpath") && testpath != null)
                    {
                        await testpath(context);
                    }
                    else
                    {
                        await next();
                    }
                });
            });
            server.BaseAddress = baseAddress;
            return server;
        }


        private static async Task<Transaction> SendAsync(TestServer server, string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.CreateClient().SendAsync(request),
            };
            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }
            return transaction;
        }

        private class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }

            public string ResponseText { get; set; }
            public XElement ResponseElement { get; set; }
        }
    }
}
