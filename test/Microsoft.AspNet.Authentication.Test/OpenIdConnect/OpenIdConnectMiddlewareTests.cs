// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    public class OpenIdConnectMiddlewareTests
    {
        static string noncePrefix = "OpenIdConnect." + "Nonce.";
        static string nonceDelimiter = ".";

        [Fact]
        public async Task ChallengeWillSetDefaults()
        {
            var stateDataFormat = new AuthenticationPropertiesFormater();
            var queryValues = ExpectedQueryValues.Defaults("https://login.windows.net/common");
            queryValues.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            queryValues.State = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + stateDataFormat.Protect(new AuthenticationProperties());
            var server = CreateServer(options =>
            {
                options.Authority = "https://login.windows.net/common";
                options.Configuration = queryValues.Configuration;
                options.ClientId = queryValues.ClientId;
                options.StateDataFormat = stateDataFormat;
            });

            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            queryValues.CheckValues(
                transaction.Response.Headers.Location.AbsoluteUri,
                new string[]
                {
                    OpenIdConnectParameterNames.ClientId,
                    OpenIdConnectParameterNames.ResponseMode,
                    OpenIdConnectParameterNames.ResponseType,
                    OpenIdConnectParameterNames.Scope,
                    OpenIdConnectParameterNames.State
                });
        }

        [Fact]
        public async Task ChallengeWillSetNonceCookie()
        {
            var server = CreateServer(options =>
            {
                options.Authority = "https://login.windows.net/common";
                options.ClientId = "Test Id";
                options.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            });
            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.SetCookie.Single().ShouldContain(OpenIdConnectAuthenticationDefaults.CookieNoncePrefix);
        }

        [Fact]
        public async Task ChallengeWillUseOptionsProperties()
        {
            var queryValues = new ExpectedQueryValues("https://login.windows.net/common");
            queryValues.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();

            var server = CreateServer(options =>
            {
                options.Authority = queryValues.Authority;
                options.ClientId = queryValues.ClientId;
                options.Configuration = queryValues.Configuration;
                options.RedirectUri = queryValues.RedirectUri;
                options.Resource = queryValues.Resource;
                options.Scope = queryValues.Scope;
            });

            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            queryValues.CheckValues(
                transaction.Response.Headers.Location.AbsoluteUri,
                new string[]
                {
                    OpenIdConnectParameterNames.ClientId,
                    OpenIdConnectParameterNames.RedirectUri,
                    OpenIdConnectParameterNames.Resource,
                    OpenIdConnectParameterNames.ResponseMode,
                    OpenIdConnectParameterNames.Scope
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChallengeWillSetStateInNotification(bool setStateInNotification)
        {
            var queryValues = new ExpectedQueryValues("https://login.windows.net/common");
            queryValues.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var stateDataFormat = new AuthenticationPropertiesFormater();
            var server = CreateServer(options =>
            {
                options.Authority = queryValues.Authority;
                options.ClientId = queryValues.ClientId;
                options.Configuration = queryValues.Configuration;
                options.RedirectUri = queryValues.RedirectUri;
                options.Resource = queryValues.Resource;
                options.Scope = queryValues.Scope;
                options.StateDataFormat = stateDataFormat;
                options.Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = notification =>
                    {
                        if (setStateInNotification)
                        {
                            notification.ProtocolMessage.State = queryValues.State;
                        }
                        return Task.FromResult<object>(null);
                    }
                };
            });

            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            if (setStateInNotification)
            {
                queryValues.State += ("&" + OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + stateDataFormat.Protect(new AuthenticationProperties()));
            }
            else
            {
                queryValues.State = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + stateDataFormat.Protect(new AuthenticationProperties());
            }

            queryValues.CheckValues(
                transaction.Response.Headers.Location.AbsoluteUri,
                new List<string>
                {
                    OpenIdConnectParameterNames.ClientId,
                    OpenIdConnectParameterNames.RedirectUri,
                    OpenIdConnectParameterNames.Resource,
                    OpenIdConnectParameterNames.ResponseMode,
                    OpenIdConnectParameterNames.Scope,
                    OpenIdConnectParameterNames.State,
                }
            );
        }

        [Fact]
        public async Task ChallengeWillUseNotifications()
        {
            var queryValues = new ExpectedQueryValues("https://login.windows.net/common");
            queryValues.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var queryValuesSetInNotification = new ExpectedQueryValues("https://login.windows.net/common");
            queryValuesSetInNotification.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = queryValues.Authority;
                options.ClientId = queryValues.ClientId;
                options.Configuration = queryValues.Configuration;
                options.RedirectUri = queryValues.RedirectUri;
                options.Resource = queryValues.Resource;
                options.Scope = queryValues.Scope;
                options.Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = notification =>
                    {
                        notification.ProtocolMessage.ClientId = queryValuesSetInNotification.ClientId;
                        notification.ProtocolMessage.RedirectUri = queryValuesSetInNotification.RedirectUri;
                        notification.ProtocolMessage.Resource = queryValuesSetInNotification.Resource;
                        notification.ProtocolMessage.Scope = queryValuesSetInNotification.Scope;
                        return Task.FromResult<object>(null);
                    }
                };
            });

            var transaction = await SendAsync(server,"https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            queryValuesSetInNotification.CheckValues(
                transaction.Response.Headers.Location.AbsoluteUri,
                new string[]
                {
                    OpenIdConnectParameterNames.ClientId,
                    OpenIdConnectParameterNames.RedirectUri,
                    OpenIdConnectParameterNames.Resource,
                    OpenIdConnectParameterNames.ResponseMode,
                    OpenIdConnectParameterNames.Scope
                });
        }

        [Fact]
        public async Task SignOutWithDefaultRedirectUri()
        {
            var configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = "https://login.windows.net/common";
                options.ClientId = "Test Id";
                options.Configuration = configuration;
            });

            var transaction = await SendAsync(server, "https://example.com/signout");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.AbsoluteUri.ShouldBe(configuration.EndSessionEndpoint);
        }

        [Fact]
        public async Task SignOutWithCustomRedirectUri()
        {
            var configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = "https://login.windows.net/common";
                options.ClientId = "Test Id";
                options.Configuration = configuration;
                options.PostLogoutRedirectUri = "https://example.com/logout";
            });

            var transaction = await SendAsync(server, "https://example.com/signout");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.AbsoluteUri.ShouldContain(UrlEncoder.Default.UrlEncode("https://example.com/logout"));
        }

        [Fact]
        public async Task SignOutWith_Specific_RedirectUri_From_Authentication_Properites()
        {
            var configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = "https://login.windows.net/common";
                options.ClientId = "Test Id";
                options.Configuration = configuration;
                options.PostLogoutRedirectUri = "https://example.com/logout";
            });

            var transaction = await SendAsync(server, "https://example.com/signout_with_specific_redirect_uri");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.AbsoluteUri.ShouldContain(UrlEncoder.Default.UrlEncode("http://www.example.com/specific_redirect_uri"));
        }

        [Fact]
        // Test Cases for calculating the expiration time of cookie from cookie name
        public void NonceCookieExpirationTime()
        {
            DateTime utcNow = DateTime.UtcNow;

            GetNonceExpirationTime(noncePrefix + DateTime.MaxValue.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(DateTime.MaxValue);

            GetNonceExpirationTime(noncePrefix + DateTime.MinValue.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue + TimeSpan.FromHours(1));

            GetNonceExpirationTime(noncePrefix + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(utcNow + TimeSpan.FromHours(1));

            GetNonceExpirationTime(noncePrefix, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);

            GetNonceExpirationTime("", TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);

            GetNonceExpirationTime(noncePrefix + noncePrefix, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);

            GetNonceExpirationTime(noncePrefix + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(utcNow + TimeSpan.FromHours(1));

            GetNonceExpirationTime(utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);
        }

        private static TestServer CreateServer(Action<OpenIdConnectAuthenticationOptions> configureOptions, Func<HttpContext, Task> handler = null)
        {
            return TestServer.Create(app =>
            {
                app.UseCookieAuthentication(options =>
                {
                    options.AuthenticationScheme = OpenIdConnectAuthenticationDefaults.AuthenticationScheme;
                });
                app.UseOpenIdConnectAuthentication(configureOptions);
                app.Use(async (context, next) =>
                {
                    var req = context.Request;
                    var res = context.Response;
                    if (req.Path == new PathString("/challenge"))
                    {
                        context.Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
                        res.StatusCode = 401;
                    }
                    else if (req.Path == new PathString("/signin"))
                    {
                        // REVIEW: this used to just be res.SignIn()
                        context.Authentication.SignIn(OpenIdConnectAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal());
                    }
                    else if (req.Path == new PathString("/signout"))
                    {
                        context.Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
                    }
                    else if (req.Path == new PathString("/signout_with_specific_redirect_uri"))
                    {
                        context.Authentication.SignOut(
                            OpenIdConnectAuthenticationDefaults.AuthenticationScheme,
                            new AuthenticationProperties() { RedirectUri = "http://www.example.com/specific_redirect_uri" });
                    }
                    else if (handler != null)
                    {
                        await handler(context);
                    }
                    else
                    {
                        await next();
                    }
                });
            },
            services =>
            {
                services.AddAuthentication();
                services.Configure<ExternalAuthenticationOptions>(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });
            });
        }

        private static async Task<Transaction> SendAsync(TestServer server, string uri, string cookieHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.CreateClient().SendAsync(request),
            };
            if (transaction.Response.Headers.Contains("Set-Cookie"))
            {
                transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").ToList();
            }
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

            public IList<string> SetCookie { get; set; }

            public string ResponseText { get; set; }

            public XElement ResponseElement { get; set; }

            public string AuthenticationCookieValue
            {
                get
                {
                    if (SetCookie != null && SetCookie.Count > 0)
                    {
                        var authCookie = SetCookie.SingleOrDefault(c => c.Contains(".AspNet.Cookie="));
                        if (authCookie != null)
                        {
                            return authCookie.Substring(0, authCookie.IndexOf(';'));
                        }
                    }

                    return null;
                }
            }
        }

        private static DateTime GetNonceExpirationTime(string keyname, TimeSpan nonceLifetime)
        {
            DateTime nonceTime = DateTime.MinValue;
            string timestamp = null;
            int endOfTimestamp;
            if (keyname.StartsWith(noncePrefix, StringComparison.Ordinal))
            {
                timestamp = keyname.Substring(noncePrefix.Length);
                endOfTimestamp = timestamp.IndexOf('.');

                if (endOfTimestamp != -1)
                {
                    timestamp = timestamp.Substring(0, endOfTimestamp);
                    try
                    {
                        nonceTime = DateTime.FromBinary(Convert.ToInt64(timestamp, CultureInfo.InvariantCulture));
                        if ((nonceTime >= DateTime.UtcNow) && ((DateTime.MaxValue - nonceTime) < nonceLifetime))
                            nonceTime = DateTime.MaxValue;
                        else
                            nonceTime += nonceLifetime;
                    }
                    catch
                    {
                    }
                }
            }
            return nonceTime;
        }
    }
}