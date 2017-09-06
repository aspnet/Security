// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Responsible for mapping names to actual AuthenticationPolicy instances.
    /// </summary>
    public interface IAuthenticationPolicyProvider
    {
        /// <summary>
        /// Returns the <see cref="AuthenticationPolicy"/> for the name and request.
        /// A null name is used to represent the default policy.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="name">The name of the authenticationPolicy.</param>
        /// <returns>The policy or null if not found.</returns>
        Task<AuthenticationPolicy> GetAsync(HttpContext context, string name);
    }

    // TODO: move to a real home
    public class AuthenticationPolicyOptions
    {
        public IDictionary<string, AuthenticationPolicy> PolicyMap { get; } = new Dictionary<string, AuthenticationPolicy>();

        public void AddPolicy(string name, Action<AuthenticationPolicyBuilder> build)
        {
            var builder = new AuthenticationPolicyBuilder(name);
            build?.Invoke(builder);
            PolicyMap[builder.Name] = builder.Build();
        }
    }

    public class AuthenticationPolicyProvider : IAuthenticationPolicyProvider
    {
        private readonly IOptionsMonitor<AuthenticationPolicyOptions> _options;
        public AuthenticationPolicyProvider(IOptionsMonitor<AuthenticationPolicyOptions> options)
        {
            _options = options;
        }

        public Task<AuthenticationPolicy> GetAsync(HttpContext context, string name)
        {
            var map = _options.CurrentValue.PolicyMap;
            return Task.FromResult(name != null && map.ContainsKey(name) ? map[name] : null);
        }
    }
}