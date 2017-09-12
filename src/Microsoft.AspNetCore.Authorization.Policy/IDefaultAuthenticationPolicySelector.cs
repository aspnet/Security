// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Used to select the default authentication policy
    /// </summary>
    public interface IDefaultAuthenticationPolicySelector
    {
        /// <summary>
        /// Get the default policy name for a given request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>The default policy name.</returns>
        Task<string> GetDefaultPolicyAsync(HttpContext context);
    }

    public class DefaultAuthenticationPolicySelector : IDefaultAuthenticationPolicySelector
    {
        private readonly IOptionsMonitor<AuthenticationPolicyOptions> _options;
        public DefaultAuthenticationPolicySelector(IOptionsMonitor<AuthenticationPolicyOptions> options)
        {
            _options = options;
        }

        public Task<string> GetDefaultPolicyAsync(HttpContext context)
            => Task.FromResult(_options.CurrentValue.DefaultPolicySelector?.Invoke(context) ?? _options.CurrentValue.DefaultPolicy);
    }

}
