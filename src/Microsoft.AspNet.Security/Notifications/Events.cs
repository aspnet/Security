// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    public interface IEventBus
    {
        // Returns true if anyone handled the event
        Task<bool> RaiseAsync(object ev);
    }

    // use DI to find subscribers of event
    public interface IEventHandler
    {
    }

    public interface IEventHandler<T> : IEventHandler
    {
        // If true, stop calling other handlers
        Task<bool> HandleAsync(T ev);
    }

    // Need new name
    public class EventHandler<T> : IEventHandler<T>
    {
        private readonly Func<T, Task<bool>> _handler;
        public EventHandler(Func<T, Task<bool>> handler)
        {
            _handler = handler;
        }

        public async Task<bool> HandleAsync(T ev)
        {
            return await _handler(ev);
        }

    }

    public class EventBus : IEventBus
    {
        private IEnumerable<IEventHandler> _handlers;

        public EventBus(IEnumerable<IEventHandler> handlers)
        {
            _handlers = handlers;
        }

        private static async Task<bool> InvokeHandle(IEventHandler handler, object ev)
        {
            var handleMethod = handler.GetType().GetTypeInfo().GetDeclaredMethod("HandleAsync");
            return await (Task<bool>)handleMethod.Invoke(handler, new[] { ev });
        }

        public async Task<bool> RaiseAsync(object ev)
        {
            // TODO: slice handlers down to handlers that care about event
            // TODO: cache

            var handlerType = typeof(IEventHandler<>).MakeGenericType(ev.GetType());
            var query = _handlers.Where(h => handlerType.GetTypeInfo().IsAssignableFrom(h.GetType().GetTypeInfo()));
            var handlers = query.ToArray().Cast<IEventHandler>();

            // DI will enumerate in reverse order of registration, 
            // so later handlers can stop calling parent ones to replace default behavior
            foreach (var handler in handlers)
            {
                if (await InvokeHandle(handler, ev))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
