// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Builds the actual AuthenticationScheme instances from the AuthenticationOptions.
    /// </summary>
    public class AuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        public AuthenticationSchemeProvider(IOptions<AuthenticationOptions> options)
        {
            _options = options.Value;

            foreach (var builder in _options.Schemes)
            {
                var scheme = builder.Build();
                AddScheme(scheme);
            }
        }

        private readonly AuthenticationOptions _options;
        private readonly object _lock = new object();

        private IDictionary<string, AuthenticationScheme> _map = new Dictionary<string, AuthenticationScheme>(StringComparer.Ordinal); // case sensitive?

        private List<AuthenticationScheme> _requestHandlers = new List<AuthenticationScheme>();

        public Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
        {
            if (_options.DefaultAuthenticationScheme != null)
            {
                return GetSchemeAsync(_options.DefaultAuthenticationScheme);
            }
            if (_map.Count == 1)
            {
                return Task.FromResult(_map.Values.First());
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
        {
            if (_options.DefaultChallengeScheme != null)
            {
                return GetSchemeAsync(_options.DefaultChallengeScheme);
            }
            if (_map.Count == 1)
            {
                return Task.FromResult(_map.Values.First());
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<AuthenticationScheme> GetDefaultSignInSchemeAsync()
        {
            if (_options.DefaultSignInScheme != null)
            {
                return GetSchemeAsync(_options.DefaultSignInScheme);
            }
            if (_map.Count == 1)
            {
                return Task.FromResult(_map.Values.First());
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            if (_map.ContainsKey(name))
            {
                return Task.FromResult(_map[name]);
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync(PathString requestPath)
        {
            return Task.FromResult<IEnumerable<AuthenticationScheme>>(_requestHandlers);
        }

        public void AddScheme(AuthenticationScheme scheme)
        {
            if (_map.ContainsKey(scheme.Name))
            {
                throw new InvalidOperationException("Scheme already exists: " + scheme.Name);
            }
            lock (_lock)
            {
                if (_map.ContainsKey(scheme.Name))
                {
                    throw new InvalidOperationException("Scheme already exists: " + scheme.Name);
                }
                if (typeof(IAuthenticationRequestHandler).IsAssignableFrom(scheme.HandlerType))
                {
                    _requestHandlers.Add(scheme);
                }
                _map[scheme.Name] = scheme;
            }
        }

        public void RemoveScheme(string name)
        {
            if (!_map.ContainsKey(name))
            {
                return;
            }
            lock (_lock)
            {
                if (_map.ContainsKey(name))
                {
                    var scheme = _map[name];
                    _requestHandlers.Remove(_requestHandlers.Where(s => s.Name == name).FirstOrDefault());
                    _map.Remove(name);
                }
            }
        }

        public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            return Task.FromResult<IEnumerable<AuthenticationScheme>>(_map.Values);
        }
    }
}