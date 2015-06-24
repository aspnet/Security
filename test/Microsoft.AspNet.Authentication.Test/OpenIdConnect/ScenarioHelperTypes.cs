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
            if (string.IsNullOrWhiteSpace(protectedText))
            {
                return null;
            }

            if (protectedText == "null")
            {
                return new AuthenticationProperties();
            }

            string[] items = protectedText.Split(' ');
            if (items.Length % 2 != 0)
            {
                return null;
            }

            var propeties = new AuthenticationProperties();
            for (int i = 0; i < items.Length - 1; i+=2)
            {
                propeties.Items.Add(items[i], items[i + 1]);
            }

            return propeties;
        }
    }

    public class InMemoryLogger : ILogger, IDisposable
    {
        LogLevel _logLevel = 0;

        public InMemoryLogger(LogLevel logLevel = LogLevel.Debug)
        {
            _logLevel = logLevel;
        }

        List<LogEntry> _logEntries = new List<LogEntry>();

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

                _logEntries.Add(logEntry);
                Debug.WriteLine(logEntry.ToString());
            }
        }

        public List<LogEntry> Logs { get { return _logEntries; } }
    }

    public class ReturnsLoggerLoggerFactory : ILoggerFactory
    {
        InMemoryLogger _logger;
        LogLevel _logLevel = LogLevel.Debug;

        public ReturnsLoggerLoggerFactory(LogLevel logLevel)
        {
            _logLevel = logLevel;
            _logger = new InMemoryLogger(_logLevel);
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

        public InMemoryLogger Logger { get { return _logger; } }
    }

    /// <summary>
    ///  Allows for custom processing of ApplyResponseChallenge, ApplyResponseGrant and AuthenticateCore
    /// </summary>
    public class OpenIdConnectAuthenticationHandlerForTestingAuthenticate : OpenIdConnectAuthenticationHandler
    {
        private Func<Task> _applyResponseChallenge;
        private Func<Task> _applyResponseGrant;
        private Func<Task<AuthenticationTicket>> _authenticationCore;

        public OpenIdConnectAuthenticationHandlerForTestingAuthenticate(Func<Task> applyResponseChallenge = null, Func<Task> applyResponseGrant = null, Func<Task<AuthenticationTicket>> authenticationCore = null )
                    : base()
        {
            _applyResponseChallenge = applyResponseChallenge;
            _applyResponseGrant = applyResponseGrant;
            _authenticationCore = authenticationCore;
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            if (_applyResponseChallenge != null)
                await _applyResponseChallenge();
            else
                await base.ApplyResponseChallengeAsync();
        }

        protected override async Task ApplyResponseGrantAsync()
        {
            if (_applyResponseGrant != null)
                await _applyResponseGrant();
            else
                await base.ApplyResponseGrantAsync();
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            if (_authenticationCore != null)
                return await _authenticationCore();
            else
                return await base.AuthenticateCoreAsync();
        }

        public override bool ShouldHandleScheme(string authenticationScheme)
        {
            return true;
        }
    }

    /// <summary>
    /// pass a <see cref="OpenIdConnectAuthenticationHandler"/> as the AuthenticationHandler
    /// configured to handle certain messages.
    /// </summary>
    public class OpenIdConnectAuthenticationMiddlewareForTestingAuthenticate : OpenIdConnectAuthenticationMiddleware
    {
        OpenIdConnectAuthenticationHandler _handler;

        public OpenIdConnectAuthenticationMiddlewareForTestingAuthenticate(
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
            var customFactory = loggerFactory as ReturnsLoggerLoggerFactory;
            if (customFactory != null)
                Logger = customFactory.Logger;
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return _handler ?? base.CreateHandler();
        }
    }
}
