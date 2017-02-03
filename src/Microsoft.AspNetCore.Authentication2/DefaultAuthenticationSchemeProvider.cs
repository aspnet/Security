// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication2
{
    /// <summary>
    /// Builds the actual AuthenticationScheme instances from the AuthenticationOptions2.
    /// </summary>
    public class DefaultAuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        public DefaultAuthenticationSchemeProvider(IOptions<AuthenticationOptions2> options)
        {
            _options = options.Value;

            foreach (var builder in _options.Schemes)
            {
                var scheme = builder.Build(_options);
                AddScheme(scheme);
            }
        }

        private readonly AuthenticationOptions2 _options;
        private readonly object _lock = new object();

        private readonly List<AuthenticationScheme> _schemes = new List<AuthenticationScheme>();

        public IDictionary<string, AuthenticationScheme> _map = new Dictionary<string, AuthenticationScheme>(); // case sensitive?

        public Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
        {
            if (_options.DefaultAuthenticationScheme != null)
            {
                return GetSchemeAsync(_options.DefaultAuthenticationScheme);
            }
            if (_schemes.Count == 1)
            {
                return Task.FromResult(_schemes[0]);
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
        {
            if (_options.DefaultChallengeScheme != null)
            {
                return GetSchemeAsync(_options.DefaultChallengeScheme);
            }
            if (_schemes.Count == 1)
            {
                return Task.FromResult(_schemes[0]);
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

        public Task<IEnumerable<AuthenticationScheme>> GetPriorityOrderedSchemesAsync()
        {
            return Task.FromResult<IEnumerable<AuthenticationScheme>>(_schemes);
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
                _schemes.Add(scheme);
                _map[scheme.Name] = scheme;
            }
        }
    }
}