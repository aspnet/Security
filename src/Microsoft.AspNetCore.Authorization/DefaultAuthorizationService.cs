// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Microsoft.AspNetCore.Authorization
{
    public class DefaultAuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly IList<IAuthorizationHandler> _handlers;
        private readonly ILogger _logger;

        public DefaultAuthorizationService(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizationHandler> handlers, ILogger<DefaultAuthorizationService> logger)
        {
            if (policyProvider == null)
            {
                throw new ArgumentNullException(nameof(policyProvider));
            }
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _handlers = handlers.ToArray();
            _policyProvider = policyProvider;
            _logger = logger;
        }

        public async Task<bool> AuthorizeAsync(object authorizationData, object resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            if (requirements == null)
            {
                throw new ArgumentNullException(nameof(requirements));
            }

            var authContext = new AuthorizationContext(requirements, authorizationData, resource);
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(authContext);
            }

            if (authContext.HasSucceeded)
            {
                _logger.AuthorizationSucceeded(authContext);
                return true;
            }
            else
            {
                _logger.AuthorizationFailed(authContext);
                return false;
            }
        }

        public async Task<bool> AuthorizeAsync(object authorizationData, object resource, string policyName)
        {
            if (policyName == null)
            {
                throw new ArgumentNullException(nameof(policyName));
            }

            var policy = await _policyProvider.GetPolicyAsync(policyName);
            if (policy == null)
            {
                throw new InvalidOperationException($"No policy found: {policyName}.");
            }
            return await this.AuthorizeAsync(authorizationData, resource, policy);
        }
    }
}