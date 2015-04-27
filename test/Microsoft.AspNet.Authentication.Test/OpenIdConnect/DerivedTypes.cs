// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// this controls if the logs are written to the console.
// they can be reviewed for general content.
#define _Verbose

using System;
using System.Collections.Generic;
using System.Text;
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
using Xunit;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// Processing a <see cref="OpenIdConnectMessage"/> requires 'unprotecting' the state.
    /// This class side-steps that process.
    /// </summary>
    public class AuthenticationPropertiesFormater : ISecureDataFormat<AuthenticationProperties>
    {
        string _protectedString = Guid.NewGuid().ToString();

        public string Protect(AuthenticationProperties data)
        {
            if (data == null || data.Items.Count == 0)
            {
                return "null";
            }

            var encoder = UrlEncoder.Default;
            var sb = new StringBuilder();
            foreach(var item in data.Items)
            {
                sb.Append(encoder.UrlEncode(item.Key) + " " + encoder.UrlEncode(item.Value) + " ");
            }

            return sb.ToString();
        }

        AuthenticationProperties ISecureDataFormat<AuthenticationProperties>.Unprotect(string protectedText)
        {
            var propeties = new AuthenticationProperties();
            if (protectedText != "null")
            {
                string[] items = protectedText.Split(' ');
                for (int i = 0; i < items.Length; i+=2)
                {
                    propeties.Items.Add(items[i], items[i + 1]);
                }
            }

            return propeties;
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
                var logEntry =
                    new LogEntry
                    {
                        EventId = eventId,
                        Exception = exception,
                        Formatter = formatter,
                        Level = logLevel,
                        State = state,
                    };

                logEntries.Add(logEntry);

#if _Verbose
                Console.WriteLine(logEntry.ToString());
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
        private Func<Task> _applyResponseChallenge;
        private Action<ChallengeContext> _challengeAction;
        private Func<string, bool> _shouldHandleScheme;

        public CustomOpenIdConnectAuthenticationHandler(Func<Task> applyResponseChallenge = null, Action<ChallengeContext> challengeAction = null, Func<string, bool> shouldHandleScheme = null )
                    : base()
        {
            _applyResponseChallenge = applyResponseChallenge;
            _challengeAction = challengeAction;
            _shouldHandleScheme = shouldHandleScheme;
        }
        
        protected override void ApplyResponseChallenge()
        {
            if (_applyResponseChallenge != null)
                _applyResponseChallenge();
            else
                base.ApplyResponseChallenge();
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            if (_applyResponseChallenge != null)
                await _applyResponseChallenge();
            else
                await base.ApplyResponseChallengeAsync();
        }

        public override void Challenge(ChallengeContext context)
        {
            if (_challengeAction != null)
                _challengeAction(context);
            else
                base.Challenge(context);
        }

        protected override Task InitializeCoreAsync()
        {
            base.InitializeCoreAsync();
            return Task.FromResult(0);
        }

        public override bool ShouldHandleScheme(string authenticationScheme)
        {
            if (_shouldHandleScheme != null)
                return _shouldHandleScheme(authenticationScheme);
            else
                return base.ShouldHandleScheme(authenticationScheme);
        }
    }

    /// <summary>
    /// Used to set <see cref="CustomOpenIdConnectAuthenticationHandler"/> as the AuthenticationHandler
    /// which can be configured to handle certain messages.
    /// </summary>
    public class CustomOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        OpenIdConnectAuthenticationHandler _handler;

        public CustomOpenIdConnectAuthenticationMiddleware(
            RequestDelegate next,
            IOptions<OpenIdConnectAuthenticationOptions> options,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            IOptions<ExternalAuthenticationOptions> externalOptions,
            ConfigureOptions<OpenIdConnectAuthenticationOptions> configureOptions = null,
            OpenIdConnectAuthenticationHandler handler = null
            )
        : base(next, options, dataProtectionProvider, loggerFactory, encoder, externalOptions, configureOptions)
        {
            _handler = handler;
            Logger = (loggerFactory as CustomLoggerFactory).Logger;
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return _handler ?? base.CreateHandler();
        }
    }

    public class CustomOpenIdConnectMessage : OpenIdConnectMessage
    {
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
}
