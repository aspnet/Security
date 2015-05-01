// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// this controls if the logs are written to the console.
// they can be reviewed for general content.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    public class LoggingUtilities
    {
        static List<LogEntry> CompleteLogEntries;
        static Dictionary<string, LogLevel> LogEntries;

        static LoggingUtilities()
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
                    { "OIDCH_0020:", LogLevel.Debug },
                    { "OIDCH_0026:", LogLevel.Error },
            };

            BuildLogEntryList();

        }

        /// <summary>
        /// Builds the complete list of OpenIdConnect log entries that are available in the runtime.
        /// </summary>
        private static void BuildLogEntryList()
        {
            CompleteLogEntries = new List<LogEntry>();
            foreach (var entry in LogEntries)
            {
                CompleteLogEntries.Add(new LogEntry { State = entry.Key, Level = entry.Value });
            }
        }

        /// <summary>
        /// Adds to errors if a variation if any are found.
        /// </summary>
        /// <param name="variation">if this has been seen before, errors will be appended, test results are easier to understand if this is unique.</param>
        /// <param name="capturedLogs">these are the logs the runtime generated</param>
        /// <param name="expectedLogs">these are the errors that were expected</param>
        /// <param name="errors">the dictionary to record any errors</param>
        public static void CheckLogs(string variation, List<LogEntry> capturedLogs, List<LogEntry> expectedLogs, Dictionary<string, List<Tuple<LogEntry, LogEntry>>> errors)
        {
            var localErrors = new List<Tuple<LogEntry, LogEntry>>();

            if (capturedLogs.Count >= expectedLogs.Count)
            {
                for (int i = 0; i < capturedLogs.Count; i++)
                {
                    if (i + 1 > expectedLogs.Count)
                    {
                        localErrors.Add(new Tuple<LogEntry, LogEntry>(capturedLogs[i], null));
                    }
                    else
                    {
                        if (!TestUtilities.AreEqual<LogEntry>(capturedLogs[i], expectedLogs[i]))
                        {
                            localErrors.Add(new Tuple<LogEntry, LogEntry>(capturedLogs[i], expectedLogs[i]));
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
                        localErrors.Add(new Tuple<LogEntry, LogEntry>(null, expectedLogs[i]));
                    }
                    else
                    {
                        if (!TestUtilities.AreEqual<LogEntry>(expectedLogs[i], capturedLogs[i]))
                        {
                            localErrors.Add(new Tuple<LogEntry, LogEntry>(capturedLogs[i], expectedLogs[i]));
                        }
                    }
                }
            }

            if (localErrors.Count != 0)
            {
                if (errors.ContainsKey(variation))
                {
                    foreach (var error in localErrors)
                    {
                        errors[variation].Add(error);
                    }
                }
                else
                {
                    errors[variation] = localErrors;
                }
            }
        }

        public static void DebugWriteLineLogs(List<LogEntry> logs, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Debug.WriteLine(message);

            foreach (var logentry in logs)
                Debug.WriteLine(logentry.ToString());
        }

        public static void DebugWriteLineLoggingErrors(Dictionary<string, List<Tuple<LogEntry, LogEntry>>> errors)
        {
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.WriteLine("Error in Variation: " + error.Key);
                    foreach (var logError in error.Value)
                    {
                        Debug.WriteLine("*Captured*, *Expected* : *" + (logError.Item1?.ToString() ?? "null") + "*, *" + (logError.Item2?.ToString() ?? "null") + "*");
                    }

                    Debug.WriteLine(Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Populates a list of expected log entries for a test variation.
        /// </summary>
        /// <param name="items">the index for the <see cref="LogEntry"/> in CompleteLogEntries of interest.</param>
        /// <returns>a <see cref="List{LogEntry}"/> that represents the expected entries for a test variation.</returns>
        public static List<LogEntry> PopulateLogEntries(int[] items)
        {
            var entries = new List<LogEntry>();
            foreach (var item in items)
            {
                entries.Add(CompleteLogEntries[item]);
            }

            return entries;
        }
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
                return Formatter(this.State, this.Exception);
            }
            else
            {
                string message = (Formatter != null ? Formatter(State, Exception) : (State?.ToString() ?? "null"));
                message += ", LogLevel: " + Level.ToString();
                message += ", EventId: " + EventId.ToString();
                message += ", Exception: " + (Exception == null ? "null" : Exception.Message);
                return message;
            }
        }
    }    
}
