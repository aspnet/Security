// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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

    /// <summary>
    /// This helper class is used to check that query string parameters are as expected.
    /// </summary>
    public class ExpectedQueryValues
    {
        public ExpectedQueryValues(string authority)
        {
            Authority = authority;
        }

        public void CheckValues(string query, string[] parameters)
        {
            var errors = new List<string>();
            if (!query.StartsWith(Authority))
            {
                errors.Add("authority: " + Authority);
            }

            foreach(var str in parameters)
            {
                if (str == OpenIdConnectParameterNames.ClientId && !query.Contains(ExpectedClientId))
                {
                    errors.Add(ExpectedClientId);
                }
                else if (str == OpenIdConnectParameterNames.RedirectUri && !query.Contains(ExpectedRedirectUri))
                {
                    errors.Add(ExpectedRedirectUri);
                }
                else if (str == OpenIdConnectParameterNames.Resource && !query.Contains(ExpectedResource))
                {
                    errors.Add(ExpectedResource);
                }
                else if (str == OpenIdConnectParameterNames.ResponseMode && !query.Contains(ExpectedResponseMode))
                {
                    errors.Add(ExpectedResponseMode);
                }
                else if (str == OpenIdConnectParameterNames.Scope && !query.Contains(ExpectedScope))
                {
                    errors.Add(ExpectedScope);
                }
            }

            if (errors.Count > 0)
            {
                Console.WriteLine("query string not as expected: " + Environment.NewLine + query + Environment.NewLine);
                foreach (var str in errors)
                {
                    Console.WriteLine(str);
                }

                Console.WriteLine(Environment.NewLine);
                Assert.True(false);
            }
        }

        public string Authority { get; set; }
        public string ClientId { get; set; } = Guid.NewGuid().ToString();
        public string RedirectUri { get; set; } = Guid.NewGuid().ToString();
        public string Resource { get; set; } = Guid.NewGuid().ToString();
        public string ResponseMode { get; set; } = OpenIdConnectResponseModes.FormPost;
        public string ResponseType { get; set; } = Guid.NewGuid().ToString();
        public string Scope { get; set; } = Guid.NewGuid().ToString();

        public string ExpectedClientId
        {
            get { return OpenIdConnectParameterNames.ClientId + "=" + ClientId; }
        }

        public string ExpectedRedirectUri
        {
            get { return OpenIdConnectParameterNames.RedirectUri + "=" + RedirectUri; }
        }

        public string ExpectedResource
        {
            get { return OpenIdConnectParameterNames.Resource + "=" + Resource; }
        }

        public string ExpectedResponseMode
        {
            get { return OpenIdConnectParameterNames.ResponseMode + "=" + ResponseMode; }
        }

        public string ExpectedScope
        {
            get { return OpenIdConnectParameterNames.Scope + "=" + Scope; }
        }
    }
}
