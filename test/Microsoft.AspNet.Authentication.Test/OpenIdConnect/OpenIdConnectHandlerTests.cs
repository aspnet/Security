// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// this controls if the logs are written to the console.
// they can be reviewed for general content.
//#define _Verbose

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Notifications;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// These tests are designed to test OpenIdConnectAuthenticationHandler.
    /// </summary>
    public class OpenIdConnectHandlerTests
    {
        /// <summary>
        /// Sanity check that logging is filtering, hi / low water marks are checked
        /// </summary>
        [Fact]
        public void LoggingLevel()
        {
            var logger = new CustomLogger(LogLevel.Debug);
            logger.IsEnabled(LogLevel.Critical).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Debug).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Error).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Information).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Verbose).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Warning).ShouldBe<bool>(true);

            logger = new CustomLogger(LogLevel.Critical);
            logger.IsEnabled(LogLevel.Critical).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Debug).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Error).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Information).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Verbose).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Warning).ShouldBe<bool>(false);
        }

        /// <summary>
        /// Test <see cref="OpenIdConnectAuthenticationHandler.AuthenticateCoreAsync"/> produces expected logs.
        /// Each call to 'RunVariation' is configured with an <see cref="OpenIdConnectAuthenticationOptions"/> and <see cref="OpenIdConnectMessage"/>.
        /// The list of expected log entries is checked and any errors reported.
        /// <see cref="CustomLoggerFactory"/> captures the logs so they can be prepared.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AuthenticateCore()
        {
            var propertiesFormatter = new AuthenticationPropertiesFormater();
            var protectedProperties = propertiesFormatter.Protect(new AuthenticationProperties());
            var state = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + UrlEncoder.Default.UrlEncode(protectedProperties);
            var code = Guid.NewGuid().ToString();
            var message =
                new OpenIdConnectMessage
                {
                    Code = code,
                    State = state,
                };

            var errors = new Dictionary<string, List<Tuple<LogEntry, LogEntry>>>();

            var logsEntriesExpected = new int[] { 0, 1, 7, 14, 15 };
            await RunVariation(LogLevel.Debug, message, CodeReceivedHandledOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[] { 0, 1, 7, 14, 16 };
            await RunVariation(LogLevel.Debug, message, CodeReceivedSkippedOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[] { 0, 1, 7, 14 };
            await RunVariation(LogLevel.Debug, message, DefaultOptions, errors, logsEntriesExpected);

            // each message below should return before processing the idtoken
            message.IdToken = "invalid_token";

            logsEntriesExpected = new int[] { 0, 1, 2 };
            await RunVariation(LogLevel.Debug, message, MessageReceivedHandledOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[]{ 2 };
            await RunVariation(LogLevel.Information, message, MessageReceivedHandledOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[] { 0, 1, 3 };
            await RunVariation(LogLevel.Debug, message, MessageReceivedSkippedOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[] { 3 };
            await RunVariation(LogLevel.Information, message, MessageReceivedSkippedOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[] {0, 1, 7, 20, 8 };
            await RunVariation(LogLevel.Debug, message, SecurityTokenReceivedHandledOptions, errors, logsEntriesExpected);

            logsEntriesExpected = new int[] {0, 1, 7, 20, 9 };
            await RunVariation(LogLevel.Debug, message, SecurityTokenReceivedSkippedOptions, errors, logsEntriesExpected);

#if _Verbose
            Console.WriteLine("\n ===== \n");
            DisplayErrors(errors);
#endif
            errors.Count.ShouldBe(0);
        }

        /// <summary>
        /// Tests that <see cref="OpenIdConnectAuthenticationHandler"/> processes a messaage as expected.
        /// The test runs two independant paths: Using <see cref="ConfigureOptions{TOptions}"/> and <see cref="IOptions{TOptions}"/>
        /// </summary>
        /// <param name="logLevel"><see cref="LogLevel"/> for this variation</param>
        /// <param name="message">the <see cref="OpenIdConnectMessage"/> that has arrived</param>
        /// <param name="action">the <see cref="OpenIdConnectAuthenticationOptions"/> delegate used for setting the options.</param>
        /// <param name="errors">container for propogation of errors.</param>
        /// <param name="logsEntriesExpected">the expected log entries</param>
        /// <returns>a Task</returns>
        private async Task RunVariation(LogLevel logLevel, OpenIdConnectMessage message, Action<OpenIdConnectAuthenticationOptions> action, Dictionary<string, List<Tuple<LogEntry, LogEntry>>> errors, int[] logsEntriesExpected)
        {
            var expectedLogs = LoggingUtilities.PopulateLogEntries(logsEntriesExpected);
            string variation = action.Method.ToString().Substring(5, action.Method.ToString().IndexOf('(') - 5);
#if _Verbose
            Console.WriteLine(Environment.NewLine + "=====" + Environment.NewLine + "Variation: " + variation + ", LogLevel: " + logLevel.ToString() + Environment.NewLine + Environment.NewLine + "Expected Logs: ");
            DisplayLogs(expectedLogs);
            Console.WriteLine(Environment.NewLine + "Logs using ConfigureOptions:");
#endif
            var form = new FormUrlEncodedContent(message.Parameters);
            var loggerFactory = new CustomLoggerFactory(logLevel);
            var server = CreateServer(new CustomConfigureOptions(action), loggerFactory);
            await server.CreateClient().PostAsync("http://localhost", form);
            LoggingUtilities.CheckLogs(variation + ":ConfigOptions", loggerFactory.Logger.Logs, expectedLogs, errors);

#if _Verbose
            Console.WriteLine(Environment.NewLine + "Logs using IOptions:");
#endif
            form = new FormUrlEncodedContent(message.Parameters);
            loggerFactory = new CustomLoggerFactory(logLevel);
            server = CreateServer(new Options(action), loggerFactory);
            await server.CreateClient().PostAsync("http://localhost", form);
            LoggingUtilities.CheckLogs(variation + ":IOptions", loggerFactory.Logger.Logs, expectedLogs, errors);
        }

        private void DisplayLogs(List<LogEntry> logs)
        {
            foreach (var logentry in logs)
            {
                Console.WriteLine(logentry.ToString());
            }
        }

        private void DisplayErrors(Dictionary<string, List<Tuple<LogEntry, LogEntry>>> errors)
        {
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine("Error in Variation: " + error.Key);
                    foreach (var logError in error.Value)
                    {
                        Console.WriteLine("*Captured*, *Expected* : *" + (logError.Item1?.ToString() ?? "null") + "*, *" + (logError.Item2?.ToString() ?? "null") + "*");
                    }
                    Console.WriteLine(Environment.NewLine);
                }
            }
        }

        #region Configure Options

        private static void CodeReceivedHandledOptions(OpenIdConnectAuthenticationOptions options)
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

        private static void CodeReceivedSkippedOptions(OpenIdConnectAuthenticationOptions options)
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

        private static void DefaultOptions(OpenIdConnectAuthenticationOptions options)
        {
            options.AuthenticationScheme = "OpenIdConnectHandlerTest";
            options.ConfigurationManager = ConfigurationManager.DefaultStaticConfigurationManager;
            options.StateDataFormat = new AuthenticationPropertiesFormater();
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

        private static void SecurityTokenValidatedHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            DefaultOptions(options);
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
            DefaultOptions(options);
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

        #endregion

        private static TestServer CreateServer(IOptions<OpenIdConnectAuthenticationOptions> options, ILoggerFactory loggerFactory)
        {
            return TestServer.Create(
                app =>
                {
                    app.UseCustomOpenIdConnectAuthentication(options, loggerFactory);
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

        private static TestServer CreateServer(CustomConfigureOptions configureOptions, ILoggerFactory loggerFactory)
        {
            return TestServer.Create(
                app =>
                {
                    app.UseCustomOpenIdConnectAuthentication(configureOptions, loggerFactory);
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
