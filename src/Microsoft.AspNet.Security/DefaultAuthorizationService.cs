// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security
{
    public class DefaultAuthorizationService : IAuthorizationService
    {
        private readonly IList<IAuthorizationPolicyHandler> _handlers;
        private readonly AuthorizationOptions _options;

        public DefaultAuthorizationService(IOptions<AuthorizationOptions> options, IEnumerable<IAuthorizationPolicyHandler> handlers)
        {
            if (_handlers == null)
            {
                _handlers = new List<IAuthorizationPolicyHandler>();
                _handlers.Add(new DefaultAuthoriziationPolicyHandler());
            }
            else
            {
                _handlers = handlers.ToArray(); // REVIEW: order?
            }
            _options = options.Options;
        }

        public Task<bool> AuthorizeAsync([NotNull] string policyName, ClaimsPrincipal user, params object[] resources)
        {
            if (!_options.Policies.ContainsKey(policyName))
            {
                return Task.FromResult(false);
            }
            return AuthorizeAsync(_options.Policies[policyName], user, resources);
        }

        public async Task<bool> AuthorizeAsync([NotNull] IAuthorizationPolicy policy, ClaimsPrincipal user, params object[] resources)
        {
            if (user == null)
            {
                return false;
            }

            var context = new AuthorizationContext(policy, user, resources);
            foreach (var handler in _handlers)
            {
                var authorized = await handler.AuthorizeAsync(context);
                if (!authorized)
                {
                    return false;
                }
            }
            // TODO: we don't really use the Authorized right now
            context.Authorized = true;
            return true;
        }
    }
}