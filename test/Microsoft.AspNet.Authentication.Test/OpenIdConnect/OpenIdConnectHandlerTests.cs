// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Moq;
using Shouldly;
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

        /// <summary>
        /// Sanity check that logging is filtering, hi / low water marks are checked
        /// </summary>
        [Fact]
        public void LoggingLevel()
        {
            var logger = new InMemoryLogger(LogLevel.Debug);
            logger.IsEnabled(LogLevel.Critical).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Debug).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Error).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Information).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Verbose).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Warning).ShouldBe<bool>(true);

            logger = new InMemoryLogger(LogLevel.Critical);
            logger.IsEnabled(LogLevel.Critical).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Debug).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Error).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Information).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Verbose).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Warning).ShouldBe<bool>(false);
        }

        [Theory, MemberData("AuthenticationCoreVariations")]
        public async Task AuthenticateCore(LogLevel logLevel, int[] expectedLogIndexes, Action<OpenIdConnectAuthenticationOptions> action, OpenIdConnectMessage message)
        {
            var errors = new List<Tuple<LogEntry, LogEntry>>();
            var expectedLogs = LoggingUtilities.PopulateLogEntries(expectedLogIndexes);
            var handler = new OpenIdConnectAuthenticationHandlerForTestingAuthenticate(EmptyTask, EmptyTask);
            var loggerFactory = new ReturnsLoggerLoggerFactory(logLevel);
            var server = CreateServer(new ConfigureOptions<OpenIdConnectAuthenticationOptions>(action), UrlEncoder.Default, loggerFactory, handler);

            await server.CreateClient().PostAsync("http://localhost", new FormUrlEncodedContent(message.Parameters));
            LoggingUtilities.CheckLogs(loggerFactory.Logger.Logs, expectedLogs, errors);
            Debug.WriteLine(LoggingUtilities.LoggingErrors(errors));
            Assert.True(errors.Count == 0, LoggingUtilities.LoggingErrors(errors));
        }

        public static TheoryData<LogLevel, int[], Action<OpenIdConnectAuthenticationOptions>, OpenIdConnectMessage> AuthenticationCoreVariations
        {
            get
            {
                var formater = new AuthenticationPropertiesFormaterKeyValue();
                var dataset = new TheoryData<LogLevel, int[], Action< OpenIdConnectAuthenticationOptions >, OpenIdConnectMessage>();
                var properties = new AuthenticationProperties();
                var message = new OpenIdConnectMessage();
                var validState = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + UrlEncoder.Default.UrlEncode(formater.Protect(properties));
                message.State = validState;

                // MessageReceived - Handled / Skipped
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 2 }, MessageReceivedHandledOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 2 }, MessageReceivedHandledOptions, message);
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 3 }, MessageReceivedSkippedOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 3 }, MessageReceivedSkippedOptions, message);

                // State - null, empty string, invalid
                message = new OpenIdConnectMessage();
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 4, 7 }, StateNullOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 4 }, StateNullOptions, message);

                message = new OpenIdConnectMessage();
                message.State = string.Empty;
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 4, 7 }, StateNullOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 4 }, StateNullOptions, message);

                message = new OpenIdConnectMessage();
                message.State = Guid.NewGuid().ToString();
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 5 }, StateInvalidOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 5 }, StateInvalidOptions, message);

                // OpenIdConnectMessage.Error != null
                message = new OpenIdConnectMessage();
                message.Error = "Error";
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 4, 6, 17, 18 }, MessageWithErrorOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 4, 6, 17, 18 }, MessageWithErrorOptions, message);

                // SecurityTokenReceived - Handled / Skipped 
                message = new OpenIdConnectMessage();
                message.IdToken = "invalid";
                message.State = validState;
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20, 8 }, SecurityTokenReceivedHandledOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 8 }, SecurityTokenReceivedHandledOptions, message);

                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20, 9 }, SecurityTokenReceivedSkippedOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 9 }, SecurityTokenReceivedSkippedOptions, message);

                // SecurityTokenValidation - ReturnsNull, Throws, Validates
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20, 11, 17, 18 }, SecurityTokenValidatorCannotReadAnyToken, message);
                dataset.Add(LogLevel.Information, new int[] { 17, 18 }, SecurityTokenValidatorCannotReadAnyToken, message);

                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20, 17, 21, 18 }, SecurityTokenValidatorThrows, message);
                dataset.Add(LogLevel.Information, new int[] { 17, 18 }, SecurityTokenValidatorThrows, message);

                message.Nonce = nonceForJwt;
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20 }, SecurityTokenValidatorValidatesAllTokens, message);
                dataset.Add(LogLevel.Information, new int[] { }, SecurityTokenValidatorValidatesAllTokens, message);

                // SecurityTokenValidation - Handled / Skipped
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20, 12 }, SecurityTokenValidatedHandledOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 12 }, SecurityTokenValidatedHandledOptions, message);

                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 20, 13 }, SecurityTokenValidatedSkippedOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 13 }, SecurityTokenValidatedSkippedOptions, message);

                // AuthenticationCodeReceived - Handled / Skipped 
                message = new OpenIdConnectMessage();
                message.Code = Guid.NewGuid().ToString();
                message.State = validState;
                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 14, 15 }, AuthorizationCodeReceivedHandledOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 15 }, AuthorizationCodeReceivedHandledOptions, message);

                dataset.Add(LogLevel.Debug, new int[] { 0, 1, 7, 14, 16 }, AuthorizationCodeReceivedSkippedOptions, message);
                dataset.Add(LogLevel.Information, new int[] { 16 }, AuthorizationCodeReceivedSkippedOptions, message);

                return dataset;
            }
        }

        private static Task EmptyTask() { return Task.FromResult(0); }

#region Configure Options

        private static void DefaultOptions(OpenIdConnectAuthenticationOptions options)
        {
            options.AuthenticationScheme = "OpenIdConnectHandlerTest";
            options.ConfigurationManager = ConfigurationManager.DefaultStaticConfigurationManager();
            options.ClientId = Guid.NewGuid().ToString();
            options.StateDataFormat = new AuthenticationPropertiesFormaterKeyValue();
        }

        private static void AuthorizationCodeReceivedHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = (notification) =>
                    {
                        notification.HandleResponse();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void AuthorizationCodeReceivedSkippedOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = (notification) =>
                    {
                        notification.SkipToNextMiddleware();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void AuthenticationErrorHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = (notification) =>
                    {
                        notification.HandleResponse();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void AuthenticationErrorSkippedOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = (notification) =>
                    {
                        notification.SkipToNextMiddleware();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void MessageReceivedHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    MessageReceived = (notification) =>
                    {
                        notification.HandleResponse();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void MessageReceivedSkippedOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    MessageReceived = (notification) =>
                    {
                        notification.SkipToNextMiddleware();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void MessageWithErrorOptions(OpenIdConnectAuthenticationOptions options)
        {
            AuthenticationErrorHandledOptions(options);
        }

        private static void SecurityTokenReceivedHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenReceived = (notification) =>
                    {
                        notification.HandleResponse();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void SecurityTokenReceivedSkippedOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenReceived = (notification) =>
                    {
                        notification.SkipToNextMiddleware();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void SecurityTokenValidatorCannotReadAnyToken(OpenIdConnectAuthenticationOptions options)
        {
            AuthenticationErrorHandledOptions(options);
            Mock<ISecurityTokenValidator> mockValidator = new Mock<ISecurityTokenValidator>();
            SecurityToken jwt = null;
            mockValidator.Setup(v => v.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out jwt)).Returns(new ClaimsPrincipal());
            mockValidator.Setup(v => v.CanReadToken(It.IsAny<string>())).Returns(false);
            options.SecurityTokenValidators = new Collection<ISecurityTokenValidator> { mockValidator.Object };
        }

        private static void SecurityTokenValidatorThrows(OpenIdConnectAuthenticationOptions options)
        {
            AuthenticationErrorHandledOptions(options);
            Mock<ISecurityTokenValidator> mockValidator = new Mock<ISecurityTokenValidator>();
            SecurityToken jwt = new JwtSecurityToken();
            mockValidator.Setup(v => v.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out jwt)).Throws<SecurityTokenSignatureKeyNotFoundException>();
            mockValidator.Setup(v => v.CanReadToken(It.IsAny<string>())).Returns(true);
            options.SecurityTokenValidators = new Collection<ISecurityTokenValidator> { mockValidator.Object };
        }

        private static void SecurityTokenValidatorValidatesAllTokens(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
            Mock<ISecurityTokenValidator> mockValidator = new Mock<ISecurityTokenValidator>();
            mockValidator.Setup(v => v.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out specCompliantJwt)).Returns(new ClaimsPrincipal());
            mockValidator.Setup(v => v.CanReadToken(It.IsAny<string>())).Returns(true);
            Mock<INonceCache> mockNonceCache = new Mock<INonceCache>();
            mockNonceCache.Setup(n => n.TryRemoveNonce(It.IsAny<string>())).Returns(true);
            options.SecurityTokenValidators = new Collection<ISecurityTokenValidator> { mockValidator.Object };
            options.NonceCache = mockNonceCache.Object;
            options.ProtocolValidator.RequireTimeStampInNonce = false;
        }

        private static void SecurityTokenValidatedHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            SecurityTokenValidatorValidatesAllTokens(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = (notification) =>
                    {
                        notification.HandleResponse();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void SecurityTokenValidatedSkippedOptions(OpenIdConnectAuthenticationOptions options)
        {
            SecurityTokenValidatorValidatesAllTokens(options);
            options.Notifications =
                new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = (notification) =>
                    {
                        notification.SkipToNextMiddleware();
                        return Task.FromResult<object>(null);
                    }
                };
        }

        private static void StateNullOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
        }

        private static void StateEmptyOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
        }

        private static void StateInvalidOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
        }

#endregion

        private static TestServer CreateServer(IOptions<OpenIdConnectAuthenticationOptions> options, IUrlEncoder encoder, ILoggerFactory loggerFactory, OpenIdConnectAuthenticationHandler handler = null)
        {
            return TestServer.Create(
                app =>
                {
                    app.UseMiddleware<OpenIdConnectAuthenticationMiddlewareForTestingAuthenticate>(options, encoder, loggerFactory, handler);
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

        private static TestServer CreateServer(ConfigureOptions<OpenIdConnectAuthenticationOptions> configureOptions, IUrlEncoder encoder, ILoggerFactory loggerFactory, OpenIdConnectAuthenticationHandler handler = null)
        {
            return TestServer.Create(
                app =>
                {
                    app.UseMiddleware<OpenIdConnectAuthenticationMiddlewareForTestingAuthenticate>(configureOptions, encoder, loggerFactory, handler);
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
