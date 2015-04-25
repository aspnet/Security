// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// this controls if the logs are written to the console.
// they can be reviewed for general content.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Notifications;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// Extension specifies <see cref="CustomOpenIdConnectAuthenticationMiddleware"/> as the middleware.
    /// </summary>
    public static class OpenIdConnectAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="customConfigureOption">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <param name="loggerFactory">custom loggerFactory</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseCustomOpenIdConnectAuthentication(this IApplicationBuilder app, CustomConfigureOptions customConfigureOption, ILoggerFactory loggerFactory)
        {
            return app.UseMiddleware<CustomOpenIdConnectAuthenticationMiddleware>(customConfigureOption, loggerFactory);
        }

        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the ASP.NET runtime.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="options">Options which control the processing of the OpenIdConnect protocol and token validation.</param>
        /// <param name="loggerFactory">custom loggerFactory</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseCustomOpenIdConnectAuthentication(this IApplicationBuilder app, IOptions<OpenIdConnectAuthenticationOptions> options, ILoggerFactory loggerFactory)
        {
            return app.UseMiddleware<CustomOpenIdConnectAuthenticationMiddleware>(options, loggerFactory);
        }
    }

    /// <summary>
    /// Provides a Facade over IOptions
    /// </summary>
    public class Options : IOptions<OpenIdConnectAuthenticationOptions>
    {
        OpenIdConnectAuthenticationOptions _options;

        public Options(Action<OpenIdConnectAuthenticationOptions> action)
        {
            _options = new OpenIdConnectAuthenticationOptions();
            action(_options);
        }

        OpenIdConnectAuthenticationOptions IOptions<OpenIdConnectAuthenticationOptions>.Options
        {
            get
            {
                return _options;
            }
        }

        /// <summary>
        /// For now returns _options
        /// </summary>
        /// <param name="name">configuration to return</param>
        /// <returns></returns>
        public OpenIdConnectAuthenticationOptions GetNamedOptions(string name)
        {
            return _options;
        }
    }

    public class CustomConfigureOptions : ConfigureOptions<OpenIdConnectAuthenticationOptions>
    {
        public CustomConfigureOptions(Action<OpenIdConnectAuthenticationOptions> action)
            : base(action)
        {
        }

        public override void Configure(OpenIdConnectAuthenticationOptions options, string name = "")
        {
            base.Configure(options, name);
            return;
        }
    }

    public class CustomLogger : ILogger, IDisposable
    {
        LogLevel _logLevel = 0;

        public CustomLogger(LogLevel logLevel = LogLevel.Debug)
        {
            _logLevel = logLevel;
        }

        List<LogEntry> logEntries = new List<LogEntry>();

        public IDisposable BeginScopeImpl(object state)
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
                        Formatter = formatter,
                        Level = logLevel,
                        State = state,
                    });

#if _Verbose
                Console.WriteLine(state?.ToString() ?? "state null");
#endif
            }
        }

        public List<LogEntry> Logs { get { return logEntries; } }
    }

    public class CustomLoggerFactory : ILoggerFactory
    {
        CustomLogger _logger;
        LogLevel _logLevel = LogLevel.Debug;

        public CustomLoggerFactory(LogLevel logLevel)
        {
            _logLevel = logLevel;
            _logger = new CustomLogger(_logLevel);
        }

        public LogLevel MinimumLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public CustomLogger Logger { get { return _logger; } }
    }

    /// <summary>
    /// Used to control which methods are handled
    /// </summary>
    public class CustomOpenIdConnectAuthenticationHandler : OpenIdConnectAuthenticationHandler
    {
        public async Task BaseInitializeAsyncPublic(AuthenticationOptions options, HttpContext context, ILogger logger, IUrlEncoder encoder)
        {
            await base.BaseInitializeAsync(options, context, logger, encoder);
        }

        public override bool ShouldHandleScheme(string authenticationScheme)
        {
            return true;
        }

        public override void Challenge(ChallengeContext context)
        {
        }

        protected override void ApplyResponseChallenge()
        {
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            var redirectToIdentityProviderNotification = new RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
            {
            };

            await Options.Notifications.RedirectToIdentityProvider(redirectToIdentityProviderNotification);
        }
    }

    /// <summary>
    /// Used to set <see cref="CustomOpenIdConnectAuthenticationHandler"/> as the AuthenticationHandler
    /// which can be configured to handle certain messages.
    /// </summary>
    public class CustomOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        public CustomOpenIdConnectAuthenticationMiddleware(
            RequestDelegate next,
            IOptions<OpenIdConnectAuthenticationOptions> options,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            IOptions<ExternalAuthenticationOptions> externalOptions,
            ConfigureOptions<OpenIdConnectAuthenticationOptions> configureOptions = null
            )
        : base(next, options, dataProtectionProvider, loggerFactory, encoder, externalOptions, configureOptions)
        {
            Logger = (loggerFactory as CustomLoggerFactory).Logger;
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new CustomOpenIdConnectAuthenticationHandler();
        }
    }
}
