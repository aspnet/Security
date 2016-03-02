// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public class ClaimsTransformationOptions
    {
        public ClaimsTransformationOptions(IEnumerable<IConfigureOptions<ClaimsTransformationOptions>> configureOptions = null)
        {
            if (configureOptions != null)
            {
                foreach (var configure in configureOptions)
                {
                    configure.Configure(this);
                }
            }
        }

        public IClaimsTransformer Transformer { get; set; }
    }
}
