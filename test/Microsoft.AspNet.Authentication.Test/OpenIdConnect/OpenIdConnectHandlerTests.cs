// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Xunit;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// These tests are designed to test OpenIdConnectAuthenticationHandler.
    /// </summary>
    public class OpenIdConnectHandlerTests
    {
        private const string nonceForJwt = "abc";
        private static SecurityToken specCompliantJwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("iat", EpochTime.GetIntDate(DateTime.UtcNow).ToString()), new Claim("nonce", nonceForJwt) }, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1));
        private const string ExpectedStateParameter = "expectedState";

        /// <summary>
        /// Sanity check that logging is filtering, hi / low water marks are checked
        /// </summary>
        [Fact]
        public void LoggingLevel()
        {
            var logger = new InMemoryLogger(LogLevel.Debug);
            Assert.True(logger.IsEnabled(LogLevel.Critical));
            Assert.True(logger.IsEnabled(LogLevel.Debug));
            Assert.True(logger.IsEnabled(LogLevel.Error));
            Assert.True(logger.IsEnabled(LogLevel.Information));
            Assert.True(logger.IsEnabled(LogLevel.Verbose));
            Assert.True(logger.IsEnabled(LogLevel.Warning));

            logger = new InMemoryLogger(LogLevel.Critical);
            Assert.True(logger.IsEnabled(LogLevel.Critical));
            Assert.False(logger.IsEnabled(LogLevel.Debug));
            Assert.False(logger.IsEnabled(LogLevel.Error));
            Assert.False(logger.IsEnabled(LogLevel.Information));
            Assert.False(logger.IsEnabled(LogLevel.Verbose));
            Assert.False(logger.IsEnabled(LogLevel.Warning));
        }

        [Theory, MemberData(nameof(AuthenticateCoreStateDataSet))]
        public async Task AuthenticateCoreState(Action<OpenIdConnectOptions> action, OpenIdConnectMessage message)
        {
            var handler = new OpenIdConnectHandlerForTestingAuthenticate();
            var server = CreateServer(action, UrlEncoder.Default, handler);
            await server.CreateClient().PostAsync("http://localhost", new FormUrlEncodedContent(message.Parameters.Where(pair => pair.Value != null)));
        }

        public static TheoryData<Action<OpenIdConnectOptions>, OpenIdConnectMessage> AuthenticateCoreStateDataSet
        {
            get
            {
                var formater = new AuthenticationPropertiesFormaterKeyValue();
                var properties = new AuthenticationProperties();
                var dataset = new TheoryData<Action<OpenIdConnectOptions>, OpenIdConnectMessage>();

                // expected user state is added to the message.Parameters.Items[ExpectedStateParameter]
                // Userstate == null
                var message = new OpenIdConnectMessage();
                message.State = UrlEncoder.Default.UrlEncode(formater.Protect(properties));
                message.Code = Guid.NewGuid().ToString();
                message.Parameters.Add(ExpectedStateParameter, null);
                dataset.Add(SetStateOptions, message);

                // Userstate != null
                message = new OpenIdConnectMessage();
                properties.Items.Clear();
                var userstate = Guid.NewGuid().ToString();
                message.Code = Guid.NewGuid().ToString();
                properties.Items.Add(OpenIdConnectDefaults.UserstatePropertiesKey, userstate);
                message.State = UrlEncoder.Default.UrlEncode(formater.Protect(properties));
                message.Parameters.Add(ExpectedStateParameter, userstate);
                dataset.Add(SetStateOptions, message);
                return dataset;
            }
        }

        // Setup an event to check for expected state.
        // The state gets set by the runtime after the 'MessageReceivedContext'
        private static void SetStateOptions(OpenIdConnectOptions options)
        {
            options.AuthenticationScheme = "OpenIdConnectHandlerTest";
            options.ConfigurationManager = TestUtilities.DefaultOpenIdConnectConfigurationManager;
            options.ClientId = Guid.NewGuid().ToString();
            options.StateDataFormat = new AuthenticationPropertiesFormaterKeyValue();
            options.Events = new OpenIdConnectEvents()
            {
                OnTokenResponseReceived = context =>
                {
                    context.HandleResponse();
                    if (context.ProtocolMessage.State == null && !context.ProtocolMessage.Parameters.ContainsKey(ExpectedStateParameter))
                        return Task.FromResult<object>(null);

                    if (context.ProtocolMessage.State == null || !context.ProtocolMessage.Parameters.ContainsKey(ExpectedStateParameter))
                        Assert.True(false, "(context.ProtocolMessage.State=!= null || !context.ProtocolMessage.Parameters.ContainsKey(expectedState)");

                    Assert.Equal(context.ProtocolMessage.State, context.ProtocolMessage.Parameters[ExpectedStateParameter]);
                    return Task.FromResult<object>(null);
                }
            };
        }

        private static TestServer CreateServer(Action<OpenIdConnectOptions> configureOptions, IUrlEncoder encoder, OpenIdConnectHandler handler = null)
        {
            return TestServer.Create(
                app =>
                {
                    var options = new OpenIdConnectOptions();
                    configureOptions(options);
                    app.UseMiddleware<OpenIdConnectMiddlewareForTestingAuthenticate>(options, encoder, handler);
                    app.Use(async (context, next) =>
                    {
                        await next();
                    });
                },
                services =>
                {
                    services.AddWebEncoders();
                    services.AddDataProtection();
                }
            );
        }
    }
}
