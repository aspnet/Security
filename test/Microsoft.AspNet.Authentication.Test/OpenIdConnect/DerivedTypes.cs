// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// This formatter creates an easy to read string of the format: "'key1' 'value1' ..."
    /// </summary>
    public class AuthenticationPropertiesFormaterKeyValue : ISecureDataFormat<AuthenticationProperties>
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
                for (int i = 0; i < items.Length - 1; i+=2)
                {
                    propeties.Items.Add(items[i], items[i + 1]);
                }
            }

            return propeties;
        }
    }

    /// <summary>
    /// This formatter always throws. message and exception type can be set.
    /// Defaults:
    /// message: "AuthenticationPropertiesFormaterThrows"
    /// type:  InvalidOperationException
    /// </summary>
    public class AuthenticationPropertiesFormaterThrows : ISecureDataFormat<AuthenticationProperties>
    {
        AuthenticationPropertiesFormaterThrows(string message = "AuthenticationPropertiesFormaterThrows", Type exceptionType = null)
        {
            Message = message;
            ExceptionType = exceptionType ?? typeof(InvalidOperationException);
        }

        public Type ExceptionType { get; set; }

        public string Message { get; set; }

        public string Protect(AuthenticationProperties data)
        {
            throw (Exception)Activator.CreateInstance(ExceptionType, Message);
        }

        AuthenticationProperties ISecureDataFormat<AuthenticationProperties>.Unprotect(string protectedText)
        {
            throw (Exception)Activator.CreateInstance(ExceptionType, Message);
        }
    }

    /// <summary>
    /// This formatter returns values passed in the constructor
    /// Defaults:
    /// Protect: null
    /// UnProtect: null
    /// </summary>
    public class AuthenticationPropertiesFormaterSetReturn : ISecureDataFormat<AuthenticationProperties>
    {
        string _protect;
        AuthenticationProperties _unprotect;

        AuthenticationPropertiesFormaterSetReturn(string protect = null, AuthenticationProperties unprotect = null)
        {
            _protect = protect;
            _unprotect = unprotect;
        }

        public string Protect(AuthenticationProperties data)
        {
            return _protect;
        }

        AuthenticationProperties ISecureDataFormat<AuthenticationProperties>.Unprotect(string protectedText)
        {
            return _unprotect;
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
                Debug.WriteLine(logEntry.ToString());
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

        public OpenIdConnectAuthenticationOptions OptionsPublic { get; set; }

    }

    /// <summary>
    /// pass a <see cref="OpenIdConnectAuthenticationHandler"/> as the AuthenticationHandler
    /// configured to handle certain messages.
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
            var customHandler = _handler as CustomOpenIdConnectAuthenticationHandler;
            if (customHandler != null)
            {
                customHandler.OptionsPublic = Options;
            }
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
