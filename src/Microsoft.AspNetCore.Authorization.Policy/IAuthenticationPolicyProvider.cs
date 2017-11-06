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

        public string DefaultPolicy { get; set; }

        public Func<HttpContext, string> DefaultPolicySelector { get; set; }

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
        private readonly IDefaultAuthenticationPolicySelector _defaultSelector;
        public AuthenticationPolicyProvider(IOptionsMonitor<AuthenticationPolicyOptions> options, IDefaultAuthenticationPolicySelector defaultPolicy)
        {
            _options = options;
            _defaultSelector = defaultPolicy;
        }

        public async Task<AuthenticationPolicy> GetAsync(HttpContext context, string name)
        {
            // Use the default policy for null name
            name = name ?? await _defaultSelector.GetDefaultPolicyAsync(context);
            if (name == null) // If resolution fails, treat it as an unknown policy
            {
                return null;
            }

            var map = _options.CurrentValue.PolicyMap;
            return (name != null && map.ContainsKey(name)) ? map[name] : null;
        }
    }
}