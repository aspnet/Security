// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthorizationEndpointConventionBuilderExtensions
    {
        // REVIEW: What other overloads should exist?
        // e.g. params string[] policies, or params string[] roles
        public static IEndpointConventionBuilder RequireAuthorization(this IEndpointConventionBuilder builder, params IAuthorizeData[] authorizeData)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (authorizeData == null)
            {
                throw new ArgumentNullException(nameof(authorizeData));
            }

            builder.Apply(endpointBuilder =>
            {
                foreach (var data in authorizeData)
                {
                    // REVIEW: Are metadata being added in the correct order
                    endpointBuilder.Metadata.Add(data);
                }
            });
            return builder;
        }
    }
}