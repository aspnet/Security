// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.AspNet.Security.Notifications;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security.Cookies
{
    // TODO: rename BaseContext -> BaseEvent
    public class CookieEventHandler<TCookieEvent> : IEventHandler<TCookieEvent> where TCookieEvent : BaseContext<CookieAuthenticationOptions>
    {
        private readonly Func<TCookieEvent, Task<bool>> _handler;
        private readonly string _authenticationType;

        public CookieEventHandler(string authenticationType, Func<TCookieEvent, Task<bool>> handler)
        {
            _handler = handler;
            _authenticationType = authenticationType;
        }

        public Task<bool> HandleAsync(TCookieEvent ev)
        {
            // REVIEW: should we allow unspecified auth type to hook all cookies?
            if (string.IsNullOrWhiteSpace(_authenticationType) ||
                string.Equals(_authenticationType, ev.Options.AuthenticationType, StringComparison.OrdinalIgnoreCase))
            {
                return _handler(ev);
            }
            return Task.FromResult(false);
        }

    }
}
