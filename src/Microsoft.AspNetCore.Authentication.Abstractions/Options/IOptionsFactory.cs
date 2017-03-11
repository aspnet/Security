// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Options
{
    public static class Extensions
    {
        /// <summary>
        /// Adds services required for using options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddOptionsFactory(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(ServiceDescriptor.Singleton(typeof(IOptionsFactory<>), typeof(OptionsFactory<>)));
            return services;
        }

        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, Action<TOptions> configureOptions)
            where TOptions : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<IConfigureNamedOptions<TOptions>>(new ConfigureNamedOptions<TOptions>(name, configureOptions));
            return services;
        }

        public static IServiceCollection ConfigureAllNamed<TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TOptions : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<IConfigureNamedOptions<TOptions>>(new ConfigureNamedOptions<TOptions>(name: null, action: configureOptions));
            return services;
        }

        /// <summary>
        /// Registers an action used to validate options with a specific name.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="validateOptions">The action used to validate the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection Validate<TOptions>(this IServiceCollection services, string name, Action<TOptions> validateOptions)
            where TOptions : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (validateOptions == null)
            {
                throw new ArgumentNullException(nameof(validateOptions));
            }

            services.AddSingleton<IValidateNamedOptions<TOptions>>(new ValidateNamedOptions<TOptions>(name, validateOptions));
            return services;
        }

        public static IServiceCollection ValidateAllNamed<TOptions>(this IServiceCollection services, Action<TOptions> validateOptions)
            where TOptions : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (validateOptions == null)
            {
                throw new ArgumentNullException(nameof(validateOptions));
            }

            services.AddSingleton<IValidateNamedOptions<TOptions>>(new ValidateNamedOptions<TOptions>(name: null, action: validateOptions));
            return services;
        }
    }

/// <summary>
/// Used to retreive configured and validated TOptions instances.
/// </summary>
/// <typeparam name="TOptions">The type of options being requested.</typeparam>
public interface IOptionsFactory<out TOptions> where TOptions : class, new()
    {
        /// <summary>
        /// The configured TOptions instance with the given name.
        /// </summary>
        TOptions Get(string name);
    }

    /// <summary>
    /// Represents something that configures the TOptions type.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IConfigureNamedOptions<in TOptions> where TOptions : class
    {
        /// <summary>
        /// The name of the instance to configure.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Invoked to configure a TOptions instance.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configure.</param>
        void Configure(string name, TOptions options);
    }

    /// <summary>
    /// Implementation of IConfigureNamedOptions.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class ConfigureNamedOptions<TOptions> : IConfigureNamedOptions<TOptions> where TOptions : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="action">The action to register.</param>
        public ConfigureNamedOptions(string name, Action<TOptions> action)
        {
            Name = name;
            Action = action;
        }

        /// <summary>
        /// The options name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The configuration action.
        /// </summary>
        public Action<TOptions> Action { get; }

        /// <summary>
        /// Invokes the registered configure Action if the name matches.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public virtual void Configure(string name, TOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Null name is used to configure all named options.
            if (name == null || name == Name)
            {
                Action?.Invoke(options);
            }
        }
    }

    /// <summary>
    /// Implementation of IOptions.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsFactory<TOptions> : IOptionsFactory<TOptions> where TOptions : class, new()
    {
        private readonly Dictionary<string, TOptions> _cache = new Dictionary<string, TOptions>();
        private readonly IEnumerable<IConfigureNamedOptions<TOptions>> _setups;
        private readonly IEnumerable<IValidateNamedOptions<TOptions>> _checks;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="setups">The configuration actions to run.</param>
        /// <param name="checks">The validation actions to run.</param>
        public OptionsFactory(IEnumerable<IConfigureNamedOptions<TOptions>> setups, IEnumerable<IValidateNamedOptions<TOptions>> checks)
        {
            _setups = setups;
            _checks = checks;
        }

        public TOptions Get(string name)
        {
            if (_cache.ContainsKey(name))
            {
                return _cache[name];
            }
            else
            {
                lock (_lock)
                {
                    if (_cache.ContainsKey(name))
                    {
                        return _cache[name];
                    }

                    var value = new TOptions();
                    foreach (var setup in _setups)
                    {
                        setup.Configure(name, value);
                    }
                    foreach (var check in _checks)
                    {
                        check.Validate(name, value);
                    }
                    _cache[name] = value;
                    return value;
                }
            }
        }
    }

    /// <summary>
    /// Represents something that validate the TOptions type.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IValidateNamedOptions<in TOptions> where TOptions : class
    {
        /// <summary>
        /// The name of the options instance to validate.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Invoked to validate a TOptions instance.
        /// </summary>
        /// <param name="name">The name of the options instance being validated.</param>
        /// <param name="options">The options instance to validate.</param>
        void Validate(string name, TOptions options);
    }

    /// <summary>
    /// Implementation of IValidateNamedOptions.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class ValidateNamedOptions<TOptions> : IValidateNamedOptions<TOptions> where TOptions : class
    {
        public ValidateNamedOptions() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="action">The action to register.</param>
        public ValidateNamedOptions(string name, Action<TOptions> action)
        {
            Name = name;
            Action = action;
        }

        /// <summary>
        /// The options name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The configuration action.
        /// </summary>
        public Action<TOptions> Action { get; set; }

        /// <summary>
        /// Invokes the registered validate Action if the name matches.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public virtual void Validate(string name, TOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Null name is used to configure all named options.
            if (name == null || name == Name)
            {
                Action?.Invoke(options);
            }
        }
    }

    /// <summary>
    /// Represents something that configures the TOptions type.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IValidateOptions<in TOptions> where TOptions : class
    {
        /// <summary>
        /// Invoked to validate a TOptions instance.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        void Validate(TOptions options);
    }

}