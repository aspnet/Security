// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Base class for all authentication events.
    /// </summary>
    public class AuthenticationEvents
    {
        /// <summary>
        /// If set, this will be used to query the service container for the EventType to use instead of this instance.
        /// </summary>
        public Type EventsType { get; set; }

        public AuthenticationEvents ResolveEvents(IServiceProvider services)
        {
            if (EventsType != null)
            {
                var events = services.GetRequiredService(EventsType) as AuthenticationEvents;
                return events ?? this;
            }
            return this;
        }
    }
}