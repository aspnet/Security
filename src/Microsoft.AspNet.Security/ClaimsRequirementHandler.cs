// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    public class ClaimsRequirementHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationContext context)
        {
            if (context.User == null)
            {
                return Task.FromResult(false);
            }

            foreach (var req in context.Policy.Requirements)
            {
                var claimsReq = req as ClaimRequirement;
                if (claimsReq != null)
                {
                    // TODO: optimize this
                    bool found = false;
                    if (claimsReq.AllowedValues == null || !claimsReq.AllowedValues.Any())
                    {
                        found = context.User.Claims.Any(c => string.Equals(c.Type, claimsReq.ClaimType, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        found = context.User.Claims.Any(c => string.Equals(c.Type, claimsReq.ClaimType, StringComparison.OrdinalIgnoreCase)
                                                         && claimsReq.AllowedValues.Contains(c.Value, StringComparer.Ordinal));
                    }
                    if (found)
                    {
                        context.RequirementSucceeded(req);
                    }
                    else
                    {
                        context.Deny();
                        return Task.FromResult(0);
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
