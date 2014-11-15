// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.AspNet.Security.Notifications;
using Microsoft.Framework.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security.Cookies
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static void AddAuthenticationEventHandler<TEvent, TOptions>(
            this EventBusOptions options, 
            Func<TEvent, Task> action, 
            string authenticationType = null,
            bool handled = true)
            where TOptions : AuthenticationOptions
            where TEvent : BaseContext<TOptions>
        {
            options.Handlers.Add(new AuthenticationEventHandler<TEvent, TOptions>(authenticationType, async ev =>
            {
                await action(ev);
                return handled;
            }));
        }
    }

    // TODO: rename BaseContext -> BaseEvent
    public class AuthenticationEventHandler<TEvent, TOptions> : IEventHandler<TEvent> 
        where TOptions : AuthenticationOptions
        where TEvent : BaseContext<TOptions> 
    {
        private readonly Func<TEvent, Task<bool>> _handler;
        private readonly string _authenticationType;

        public AuthenticationEventHandler(string authenticationType, Func<TEvent, Task<bool>> handler)
        {
            _handler = handler;
            _authenticationType = authenticationType;
        }

        public Task<bool> HandleAsync(TEvent ev)
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
