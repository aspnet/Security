// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Protocols;
using System;

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
                Console.WriteLine("obj1.GetType(): " + obj1.GetType().ToString() + ", obj2.GetType(): " + obj2.GetType().ToString());
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

            if (!AreEqual<string>(logEntry1.State.ToString(), logEntry2.State.ToString(), IsLogStateEqual))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Never call this method directly, call AreObjectsEqual, as it deals with nulls and types"/>
        /// </summary>
        /// <param name="exception1"></param>
        /// <param name="exception2"></param>
        /// <returns></returns>
        private static bool IsLogStateEqual(object string1, object string2)
        {
            return (string2 as string).StartsWith((string1 as string)) || (string1 as string).StartsWith((string2 as string));
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
}
