// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using Microsoft.IdentityModel.Protocols;
using Shouldly;
using Xunit;
using System.Globalization;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// These tests are designed to test inbound flows that contain identities.
    /// id_token, id_token + code, code flows are tested.
    /// </summary>
    public class OpenIdConnectIdentityFlowTests
    {
        static List<LogEntry> CompleteLogEntries;
        static Dictionary<string, LogLevel> LogEntries;

        static OpenIdConnectIdentityFlowTests()
        {
            LogEntries =
                new Dictionary<string, LogLevel>()
                {
                    { "OIDCH_0000:", LogLevel.Debug },
                    { "OIDCH_0001:", LogLevel.Debug },
                    { "OIDCH_0002:", LogLevel.Information },
                    { "OIDCH_0003:", LogLevel.Information },
                    { "OIDCH_0004:", LogLevel.Error },
                    { "OIDCH_0005:", LogLevel.Error },
                    { "OIDCH_0006:", LogLevel.Error },
                    { "OIDCH_0007:", LogLevel.Error },
                    { "OIDCH_0008:", LogLevel.Debug },
                    { "OIDCH_0009:", LogLevel.Debug },
                    { "OIDCH_0010:", LogLevel.Error },
                    { "OIDCH_0011:", LogLevel.Error },
                    { "OIDCH_0012:", LogLevel.Debug },
                    { "OIDCH_0013:", LogLevel.Debug },
                    { "OIDCH_0014:", LogLevel.Debug },
                    { "OIDCH_0015:", LogLevel.Debug },
                    { "OIDCH_0016:", LogLevel.Debug },
                    { "OIDCH_0017:", LogLevel.Error },
                    { "OIDCH_0018:", LogLevel.Debug },
                    { "OIDCH_0019:", LogLevel.Debug },
                    { "OIDCH_0026:", LogLevel.Error },
                };

            BuildLogEntryList();
        }

        private static void BuildLogEntryList()
        {
            CompleteLogEntries = new List<LogEntry>();
            foreach(var entry in LogEntries)
            {
                CompleteLogEntries.Add(new LogEntry { State = entry.Key, Level = entry.Value });
            }
        }

        [Fact]
        public void TestLoggingMask()
        {
            var logger = new Logger(LogLevel.Debug);
            logger.IsEnabled(LogLevel.Critical).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Debug).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Error).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Information).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Verbose).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Warning).ShouldBe<bool>(true);

            logger = new Logger(LogLevel.Critical);
            logger.IsEnabled(LogLevel.Critical).ShouldBe<bool>(true);
            logger.IsEnabled(LogLevel.Debug).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Error).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Information).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Verbose).ShouldBe<bool>(false);
            logger.IsEnabled(LogLevel.Warning).ShouldBe<bool>(false);
        }

        [Fact]
        public async Task AuthenticateCore()
        {
            System.Diagnostics.Debugger.Launch();
            OpenIdConnectAuthenticationOptions options = new OpenIdConnectAuthenticationOptions();
            MessageReceivedHandledOptions(options);

            var protectedProperties = options.StateDataFormat.Protect(new AuthenticationProperties());
            var state = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + Uri.EscapeDataString(protectedProperties);
            var code = Guid.NewGuid().ToString();
            var message =
                new OpenIdConnectMessage
                {
                    Code = code,
                    State = state,
                };

            var errors = new List<Tuple<LogEntry, LogEntry>>();

            int[] items = { 0, 1, 2 };
            await RunVariation("LogLevel.ALL - MessageReceivedHandledOptions", LogLevel.Debug, message, MessageReceivedHandledOptions, errors, items);

            items = new int[]{ 2 };
            await RunVariation("LogLevel.Debug - MessageReceivedHandledOptions", LogLevel.Information, message, MessageReceivedHandledOptions, errors, items);

            items = new int[] { 0, 1, 14 };
            await RunVariation("LogLevel.Debug - DefaultOptions", LogLevel.Debug, message, DefaultOptions, errors, items);

            errors.Count.ShouldBe(0);

            Console.WriteLine("\n ===== \n");
        }

        private List<LogEntry> GetLogEntries(int[] items)
        {
            var entries = new List<LogEntry>();
            foreach(var item in items)
            {
                entries.Add(CompleteLogEntries[item]);
            }

            return entries;
        }

        private async Task RunVariation(string name, LogLevel logLevel, OpenIdConnectMessage message, Action<OpenIdConnectAuthenticationOptions> options, List<Tuple<LogEntry, LogEntry>> errors, int[] items)
        {
            Console.WriteLine(Environment.NewLine + "=====" + Environment.NewLine + name);

            var expectedLogs = GetLogEntries(items);
            var form = BuildContent(message);
            var server = CreateServer(options, OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
            var middleware = OpenIdConnectAuthenticationMiddlewarePublic.THIS;
            var logger = new Logger(logLevel, true);
            middleware.Logger = logger;

            await server.CreateClient().PostAsync("http://localhost", form);

            Console.WriteLine("=========================" + Environment.NewLine + "Expected Logs");
            DisplayLogs(expectedLogs);
            CheckLogs(logger.Logs, expectedLogs, errors);
        }

        private void DisplayLogs(List<LogEntry> logs)
        {
            foreach( var logentry in logs)
            {
                Console.WriteLine(logentry.ToString());
            }
        }

        private FormUrlEncodedContent BuildContent(OpenIdConnectMessage message)
        {
            var values = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(message.Code))
            {
                values.Add(new KeyValuePair<string, string>("code", message.Code));
            }

            if (!string.IsNullOrWhiteSpace(message.State))
            {
                values.Add(new KeyValuePair<string, string>("state", message.State));
            }

            return new FormUrlEncodedContent(values);
        }

        private void CheckLogs(List<LogEntry> capturedLogs, List<LogEntry> expectedLogs, List<Tuple<LogEntry, LogEntry>> errors)
        {
            if (capturedLogs.Count >= expectedLogs.Count)
            {
                for (int i = 0; i < capturedLogs.Count; i++)
                {
                    if (i + 1 > expectedLogs.Count)
                    {
                        errors.Add(new Tuple<LogEntry, LogEntry>(capturedLogs[i], null));
                    }
                    else
                    {
                        if (!TestUtilities.AreEqual<LogEntry>(capturedLogs[i], expectedLogs[i]))
                        {
                            errors.Add(new Tuple<LogEntry, LogEntry>(capturedLogs[i], expectedLogs[i]));
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < expectedLogs.Count; i++)
                {
                    if (i + 1 > capturedLogs.Count)
                    {
                        errors.Add(new Tuple<LogEntry, LogEntry>(expectedLogs[i], null));
                    }
                    else
                    {
                        if (!TestUtilities.AreEqual<LogEntry>(expectedLogs[i], capturedLogs[i]))
                        {
                            errors.Add(new Tuple<LogEntry, LogEntry>(expectedLogs[i], capturedLogs[i]));
                        }
                    }
                }

            }
        }

        private static void MessageReceivedHandledOptions(OpenIdConnectAuthenticationOptions options)
        {
            options.StateDataFormat = new AuthenticationPropertiesFormater();
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

        private static void DefaultOptions(OpenIdConnectAuthenticationOptions options)
        {
            options.StateDataFormat = new AuthenticationPropertiesFormater();
        }

        public Task OnMessageReceived(MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>  notification)
        {
            notification.HandleResponse();
            return Task.FromResult<object>(null);
        }

        private static TestServer CreateServer(Action<OpenIdConnectAuthenticationOptions> configureOptions, string scheme)
        {
            return TestServer.Create(app =>
            {
                app.UseOpenIdConnectAuthenticationPublic(configureOptions);
                app.Use(async (context, next) =>
                {
                    await next();
                });
            },
            services =>
            {
                services.AddDataProtection();
            });
        }
    }

    /// <summary>
    /// Extension specifies <see cref="OpenIdConnectAuthenticationMiddlewarePublic"/> as the middleware.
    /// </summary>
    public static class OpenIdConnectAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="options">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseOpenIdConnectAuthenticationPublic(this IApplicationBuilder app, Action<OpenIdConnectAuthenticationOptions> configureOptions)
        {
            return app.UseMiddleware<OpenIdConnectAuthenticationMiddlewarePublic>(new ConfigureOptions<OpenIdConnectAuthenticationOptions>(configureOptions));
        }
    }

    public class OpenIdConnectAuthenticationContext : IAuthenticateContext
    {
        public OpenIdConnectAuthenticationContext(string scheme = null)
        {
            AuthenticationScheme = scheme ?? OpenIdConnectAuthenticationDefaults.AuthenticationScheme;
        }

        public string AuthenticationScheme
        {
            get;
            set;
        }

        public void Authenticated(ClaimsPrincipal principal, IDictionary<string, string> properties, IDictionary<string, object> description)
        {
        }

        public void NotAuthenticated()
        {
        }
    }

    public class OpenIdConnectAuthenticationHandlerPublic : OpenIdConnectAuthenticationHandler
    {
        public OpenIdConnectAuthenticationHandlerPublic(ILogger logger)
            : base(logger)
        {
        }

        public async Task BaseInitializeAsyncPublic(AuthenticationOptions options, HttpContext context)
        {
            await base.BaseInitializeAsync(options, context);
        }

        public override bool ShouldHandleScheme(string authenticationScheme)
        {
            return true;
        }

        public override void Challenge(IChallengeContext context)
        {
            return;
        }

        protected override void ApplyResponseChallenge()
        {
            return;
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            var redirectToIdentityProviderNotification = new RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
            {
            };

            await Options.Notifications.RedirectToIdentityProvider(redirectToIdentityProviderNotification);
        }
    }

    public class OpenIdConnectAuthenticationMiddlewarePublic : OpenIdConnectAuthenticationMiddleware
    {
        public static IDataProtectionProvider DataProtectionProvider;
        public static OpenIdConnectAuthenticationMiddlewarePublic THIS;


        public OpenIdConnectAuthenticationMiddlewarePublic(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IOptions<OpenIdConnectAuthenticationOptions> options,
            ConfigureOptions<OpenIdConnectAuthenticationOptions> configureOptions
            )
        : base(next, dataProtectionProvider, loggerFactory, options, configureOptions)
        {
            DataProtectionProvider = dataProtectionProvider;
            OptionsPublic = options.Options;
            SecureDataFormat = Options.StateDataFormat;
            THIS = this;
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            Handler = new OpenIdConnectAuthenticationHandlerPublic(Logger);
            return Handler;
        }

        public AuthenticationHandler<OpenIdConnectAuthenticationOptions> Handler
        {
            get;
            set;
        }

        public Logger Logger
        {
            get;
            set;
        }

        public OpenIdConnectAuthenticationOptions OptionsPublic
        {
            get;
            set;
        }

        public ISecureDataFormat<AuthenticationProperties> SecureDataFormat
        {
            get;
            set;
        }
    }

    public enum LogMaskMask
    {
        None = 0,
        Debug = 1,
        Verbose = 2,
        Information = 4,
        Warning = 8,
        Error = 16,
        Critical = 32
    }

    public class LogEntry
    {
        public LogEntry() { }

        public int EventId { get; set; }

        public Exception Exception { get; set; }

        public Func<object, Exception, string> Formatter { get; set; }

        public LogLevel Level { get; set; }

        public object State { get; set; }

        public override string ToString()
        {
            if (Formatter != null)
            {
                return Formatter(this, this.Exception);
            }
            else
            {
                string message = (State == null ? "null" : State.ToString());
                message += ", LogLevel: " + Level.ToString();
                message += ", EventId: " + EventId.ToString();
                message += ", Exception: " + (Exception == null ? "null" : Exception.Message);
                return message;
            }
        }
    }
    
    public class Logger : ILogger, IDisposable
    {
        bool _echo;
        LogLevel _logLevel = 0;


        public Logger(LogLevel logLevel = LogLevel.Debug, bool echo = false)
        {
            _echo = echo;
            _logLevel = logLevel;
        }

        List<LogEntry> logEntries = new List<LogEntry>();

        public IDisposable BeginScope(object state)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (logLevel >= _logLevel);
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                logEntries.Add(
                    new LogEntry
                    {
                        EventId = eventId,
                        Exception = exception,
                        Level = logLevel,
                        State = state,
                    });

                if (_echo)
                {
                    Console.WriteLine(state.ToString());
                }
            }
        }

        public List<LogEntry> Logs { get { return logEntries; } }
    }

    public class LoggerFactory : ILoggerFactory
    {
        LogLevel _logLevel = LogLevel.Debug;
        public LogLevel MinimumLevel
        {
            get
            {
                return _logLevel;
            }

            set
            {
                _logLevel = value;
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger();
        }
    }

    public class OptionsSupplier : IOptions<OpenIdConnectAuthenticationOptions>
    {
        OpenIdConnectAuthenticationOptions _options;

        public OptionsSupplier(OpenIdConnectAuthenticationOptions options)
        {
            _options = options;
        }

        public OpenIdConnectAuthenticationOptions Options
        {
            get
            {
                return _options;
            }
        }

        public OpenIdConnectAuthenticationOptions GetNamedOptions(string name)
        {
            return _options;
        }
    }

    public class AuthenticationPropertiesFormater : ISecureDataFormat<AuthenticationProperties>
    {
        public string Protect(AuthenticationProperties data)
        {
            return "protectedData";
        }

        AuthenticationProperties ISecureDataFormat<AuthenticationProperties>.Unprotect(string protectedText)
        { 
            return new AuthenticationProperties();
        }
    }
}
