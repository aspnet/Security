// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
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

    public class EventBusOptions
    {
        public IList<IEventHandler> Handlers { get; private set; } = new List<IEventHandler>();
    }

    public static class EventServiceCollectionExtensions
    {
        public static IServiceCollection AddEventHandler<TEvent>(this IServiceCollection services, Func<TEvent, Task<bool>> action)
        {
            services.AddInstance(new EventHandler<TEvent>(ev => action(ev)));
            return services;
        }

        public static IServiceCollection ConfigureEventBus(this IServiceCollection services, Action<EventBusOptions> configure)
        {
            return services.Configure(configure);
        }
    }

    // use DI to find subscribers of event
    public interface IEventHandler
    {
    }

    public interface IEventHandler<T> : IEventHandler
    {
        // If true, stop calling other handlers
        // consider taking previous handled so everyone gets a chance?
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
        public EventBus(IOptions<EventBusOptions> options)
        {
            Options = options.Options;
        }

        public EventBusOptions Options { get; private set; }

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
            var query = Options.Handlers.Where(h => handlerType.GetTypeInfo().IsAssignableFrom(h.GetType().GetTypeInfo()));
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