// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;

namespace Microsoft.AspNet.Authentication.Infrastructure
{
    /// <summary>
    /// Provides access to the normal system clock.
    /// </summary>
    public class SystemClock : ISystemClock
    {
        /// <summary>
        /// Retrieves the current system time in UTC.
        /// </summary>
        public DateTimeOffset UtcNow
        {
            get
            {
                // the clock measures whole seconds only, to have integral expires_in results, and
                // because milliseconds do not round-trip serialization formats
                DateTimeOffset utcNow = DateTimeOffset.UtcNow;
                return utcNow.AddMilliseconds(-utcNow.Millisecond);
            }
        }
    }
}
