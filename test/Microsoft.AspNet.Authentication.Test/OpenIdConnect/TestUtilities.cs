// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Xunit;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// These utilities are designed to test openidconnect related flows
    /// </summary>
    public class TestUtilities
    {
        public static bool AreEqual<T>(object obj1, object obj2, Func<object, object, bool> comparer = null) where T : class
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }

            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            if (obj1.GetType() != obj2.GetType())
            {
                return false;
            }

            if (obj1.GetType() != typeof(T))
            {
                return false;
            }

            if (comparer != null)
            {
                return comparer(obj1, obj2);
            }

            if (typeof(T) == typeof(LogEntry))
            {
                return AreEqual(obj1 as LogEntry, obj2 as LogEntry);
            }
            else if (typeof(T) == typeof(Exception))
            {
                return AreEqual(obj1 as Exception, obj2 as Exception);
            }

            throw new ArithmeticException("Unknown type, no comparer. Type: " + typeof(T).ToString());

        }

        /// <summary>
        /// Never call this method directly, call AreObjectsEqual, as it deals with nulls and types"/>
        /// </summary>
        /// <param name="logEntry1"></param>
        /// <param name="logEntry2"></param>
        /// <returns></returns>
        private static bool AreEqual(LogEntry logEntry1, LogEntry logEntry2)
        {
            if (logEntry1.EventId != logEntry2.EventId)
            {
                return false;
            }

            if (!AreEqual<Exception>(logEntry1.Exception, logEntry2.Exception))
            {
                return false;
            }

            if (logEntry1.State == null && logEntry2.State == null)
            {
                return true;
            }

            if (logEntry1.State == null)
            {
                return false;
            }

            if (logEntry2.State == null)
            {
                return false;
            }

            string logValue1 = logEntry1.Formatter == null ? logEntry1.State.ToString() : logEntry1.Formatter(logEntry1.State, logEntry1.Exception);
            string logValue2 = logEntry2.Formatter == null ? logEntry2.State.ToString() : logEntry2.Formatter(logEntry2.State, logEntry2.Exception);

            return (logValue1.StartsWith(logValue2) || (logValue2.StartsWith(logValue1)));
        }

        /// <summary>
        /// Never call this method directly, call AreObjectsEqual, as it deals with nulls and types"/>
        /// </summary>
        /// <param name="exception1"></param>
        /// <param name="exception2"></param>
        /// <returns></returns>
        private static bool AreEqual(Exception exception1, Exception exception2)
        {
            if (!string.Equals(exception1.Message, exception2.Message))
            {
                return false;
            }

            return AreEqual<Exception>(exception1.InnerException, exception2.InnerException);
        }
    }

    public class Default
    {
        public static void Options(OpenIdConnectAuthenticationOptions options)
        {
            options.AuthenticationScheme = "OpenIdConnectHandlerTest";
            options.ConfigurationManager = ConfigurationManager.DefaultStaticConfigurationManager();
            options.ClientId = Guid.NewGuid().ToString();
            options.StateDataFormat = new AuthenticationPropertiesFormater();
        }
    }

    /// <summary>
    /// Used to set up different configurations of metadata for different tests
    /// </summary>
    public class ConfigurationManager
    {
        /// <summary>
        /// Default settings for <see cref="OpenIdConnectConfiguration"/>
        /// </summary>
        static public IConfigurationManager<OpenIdConnectConfiguration> DefaultStaticConfigurationManager()
        {
            return new StaticConfigurationManager<OpenIdConnectConfiguration>(DefaultOpenIdConnectConfiguration());
        }

        /// <summary>
        /// Default settings for <see cref="OpenIdConnectConfiguration"/>
        /// </summary>
        /// <returns></returns>
        static public OpenIdConnectConfiguration DefaultOpenIdConnectConfiguration()
        {
            return new OpenIdConnectConfiguration()
            {
                AuthorizationEndpoint = @"https://login.windows.net/common/oauth2/authorize",
                EndSessionEndpoint = @"https://login.windows.net/common/oauth2/endsessionendpoint",
                TokenEndpoint = @"https://login.windows.net/common/oauth2/token",
            };
        }
    }

    /// <summary>
    /// This helper class is used to check that query string parameters are as expected.
    /// </summary>
    public class ExpectedQueryValues
    {
        public ExpectedQueryValues(string authority, OpenIdConnectConfiguration configuration = null)
        {
            Authority = authority;
            Configuration = configuration ?? ConfigurationManager.DefaultOpenIdConnectConfiguration();
        }

        public static ExpectedQueryValues Defaults(string authority)
        {
            var result = new ExpectedQueryValues(authority);
            result.Scope = OpenIdConnectScopes.OpenIdProfile;
            result.ResponseType = OpenIdConnectResponseTypes.CodeIdToken;
            return result;
        }

        public void CheckValues(string query, IEnumerable<string> parameters)
        {
            var errors = new List<string>();
            if (!query.StartsWith(ExpectedAuthority))
            {
                errors.Add("ExpectedAuthority: " + ExpectedAuthority);
            }

            foreach(var str in parameters)
            {
                if (str == OpenIdConnectParameterNames.ClientId)
                {
                    if (!query.Contains(ExpectedClientId))
                        errors.Add("ExpectedClientId: " + ExpectedClientId);

                    continue;
                }

                if (str == OpenIdConnectParameterNames.RedirectUri)
                {
                     if(!query.Contains(ExpectedRedirectUri))
                        errors.Add("ExpectedRedirectUri: " + ExpectedRedirectUri);

                    continue;
                }

                if (str == OpenIdConnectParameterNames.Resource)
                {
                    if(!query.Contains(ExpectedResource))
                        errors.Add("ExpectedResource: " + ExpectedResource);

                    continue;
                }

                if (str == OpenIdConnectParameterNames.ResponseMode)
                {
                    if(!query.Contains(ExpectedResponseMode))
                        errors.Add("ExpectedResponseMode: " + ExpectedResponseMode);

                    continue;
                }

                if (str == OpenIdConnectParameterNames.Scope)
                {
                    if (!query.Contains(ExpectedScope))
                        errors.Add("ExpectedScope: " + ExpectedScope);

                    continue;
                }

                if (str == OpenIdConnectParameterNames.State)
                {
                    if (!query.Contains(ExpectedState))
                        errors.Add("ExpectedState: " + ExpectedState);

                    continue;
                }
            }

            if (errors.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("query string not as expected: " + Environment.NewLine + query + Environment.NewLine);
                foreach (var str in errors)
                {
                    sb.AppendLine(str);
                }

                Console.WriteLine(sb.ToString());
                Assert.True(false, sb.ToString());
            }
        }

        public string Authority { get; set; }

        public string ClientId { get; set; } = Guid.NewGuid().ToString();

        public string RedirectUri { get; set; } = Guid.NewGuid().ToString();

        public OpenIdConnectRequestType RequestType { get; set; } = OpenIdConnectRequestType.AuthenticationRequest;

        public string Resource { get; set; } = Guid.NewGuid().ToString();

        public string ResponseMode { get; set; } = OpenIdConnectResponseModes.FormPost;

        public string ResponseType { get; set; } = Guid.NewGuid().ToString();

        public string Scope { get; set; } = Guid.NewGuid().ToString();

        public string State { get; set; } = Guid.NewGuid().ToString();

        public string ExpectedAuthority
        {
            get
            {
                if (RequestType == OpenIdConnectRequestType.TokenRequest)
                {
                    return Configuration?.EndSessionEndpoint ?? Authority + @"/oauth2/token";
                }
                else if (RequestType == OpenIdConnectRequestType.LogoutRequest)
                {
                    return Configuration?.TokenEndpoint ?? Authority + @"/oauth2/logout";
                }

                return Configuration?.AuthorizationEndpoint ?? Authority + (@"/oauth2/authorize");
            }
        }

        public OpenIdConnectConfiguration Configuration { get; set; }

        public string ExpectedClientId
        {
            get { return OpenIdConnectParameterNames.ClientId + "=" + Encoder.UrlEncode(ClientId); }
        }

        public string ExpectedRedirectUri
        {
            get { return OpenIdConnectParameterNames.RedirectUri + "=" + Encoder.UrlEncode(RedirectUri); }
        }

        public string ExpectedResource
        {
            get { return OpenIdConnectParameterNames.Resource + "=" + Encoder.UrlEncode(Resource); }
        }

        public string ExpectedResponseMode
        {
            get { return OpenIdConnectParameterNames.ResponseMode + "=" + Encoder.UrlEncode(ResponseMode); }
        }

        public string ExpectedScope
        {
            get { return OpenIdConnectParameterNames.Scope + "=" + Encoder.UrlEncode(Scope); }
        }
        public string ExpectedState
        {
            get { return OpenIdConnectParameterNames.State + "=" + Encoder.UrlEncode(State); }
        }

        public UrlEncoder Encoder { get; set; } = UrlEncoder.Default;
    }
}
