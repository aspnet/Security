// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication2
{
    public interface IAuthenticationSchemeProvider
    {
        Task<AuthenticationScheme> GetSchemeAsync(string name);
        Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync();
        Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync();

        /// <summary>
        /// Returns the schemes in priority order for request handling.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AuthenticationScheme>> GetPriorityOrderedSchemes();
    }

    public class DefaultAuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        public DefaultAuthenticationSchemeProvider(IOptions<AuthenticationOptions2> options)
        {
            _options = options.Value;
        }

        private readonly AuthenticationOptions2 _options;

        public Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
        {
            if (_options.DefaultAuthenticationScheme != null)
            {
                return GetSchemeAsync(_options.DefaultAuthenticationScheme);
            }
            if (_options.SchemeMap.Count == 1)
            {
                return Task.FromResult(_options.SchemeMap.First().Value);
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
        {
            if (_options.DefaultAuthenticationScheme != null)
            {
                return GetSchemeAsync(_options.DefaultChallengeScheme);
            }
            if (_options.SchemeMap.Count == 1)
            {
                return Task.FromResult(_options.SchemeMap.First().Value);
            }
            return Task.FromResult<AuthenticationScheme>(null);
        }

        public Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            return Task.FromResult(_options.SchemeMap[name]);
        }

        public Task<IEnumerable<AuthenticationScheme>> GetPriorityOrderedSchemes()
        {
            return Task.FromResult(_options.Schemes);
        }
    }
}