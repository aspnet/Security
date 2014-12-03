// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    public class DefaultAuthorizationService : IAuthorizationService
    {
        private readonly IList<IAuthorizationPolicyHandler> _handlers;

        public DefaultAuthorizationService(IEnumerable<IAuthorizationPolicyHandler> handlers)
        {
            if (_handlers == null)
            {
                _handlers = Enumerable.Empty<IAuthorizationPolicyHandler>().ToArray();
            }
            else
            {
                _handlers = handlers.ToArray(); // REVIEW: order?
            }
        }

        public async Task<bool> AuthorizeAsync(IAuthorizationPolicy policy, ClaimsPrincipal user, params object[] resources)
        {
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



        //public int MaxRetries = 99;


        //public async Task<bool> AuthorizeAsync(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource)
        //{
        //    var context = new AuthorizationPolicyContext(claims, user, resource);

        //    foreach (var policy in _policies)
        //    {
        //        await policy.ApplyingAsync(context);
        //    }

        //    // we only apply the policies for a limited number of times to prevent
        //    // infinite loops

        //    int retries;
        //    for (retries = 0; retries < MaxRetries; retries++)
        //    {
        //        // we don't need to check for owned claims if the permission is already granted
        //        if (!context.Authorized)
        //        {
        //            if (context.User != null)
        //            {
        //                if (ClaimsMatch(context.Claims, context.UserClaims))
        //                {
        //                    context.Authorized = true;
        //                }
        //            }
        //        }

        //        // reset the retry flag
        //        context.Retry = false;

        //        // give a chance for policies to change claims or the grant
        //        foreach (var policy in _policies)
        //        {
        //            await policy.ApplyAsync(context);
        //        }

        //        // if no policies have changed the context, stop checking
        //        if (!context.Retry)
        //        {
        //            break;
        //        }
        //    }

        //    if (retries == MaxRetries)
        //    {
        //        throw new InvalidOperationException("Too many authorization retries.");
        //    }

        //    foreach (var policy in _policies)
        //    {
        //        await policy.AppliedAsync(context);
        //    }

        //    return context.Authorized;
        //}

        //public bool Authorize(IEnumerable<Claim> claims, ClaimsPrincipal user, object resource)
        //{
        //    return AuthorizeAsync(claims, user, resource).GetAwaiter().GetResult();
        //}

        //private static bool ClaimsMatch([NotNull] IEnumerable<Claim> x, [NotNull] IEnumerable<Claim> y)
        //{
        //    return x.Any(claim => 
        //                y.Any(userClaim => 
        //                    string.Equals(claim.Type, userClaim.Type, StringComparison.OrdinalIgnoreCase) &&
        //                    string.Equals(claim.Value, userClaim.Value, StringComparison.Ordinal)
        //                )
        //            );

        //}
    }

    public class DefaultAuthoriziationPolicyHandler : IAuthorizationPolicyHandler
    {
        public Task<bool> AuthorizeAsync(AuthorizationContext context)
        {
            // TODO: optimize this
            var filteredIdentities = context.User.Identities.Where(id => context.Policy.AuthenticationTypes.Contains(id.AuthenticationType));
            foreach (var requires in context.Policy.Requirements)
            {
                bool found = false;
                foreach (var identity in filteredIdentities)
                {
                    found = identity.Claims.Any(c => string.Equals(c.Type, requires.ClaimType, StringComparison.OrdinalIgnoreCase)
                                                     && requires.ClaimValueRequirement.Contains(c.Value, StringComparer.Ordinal));
                    if (found)
                    {
                        break;
                    }
                }
                if (!found)
                {
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }
    }
}