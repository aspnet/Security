// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security
{
    public class InstanceOptionsAccessor<TOptions>(TOptions options) : IOptionsAccessor<TOptions> where TOptions : class, new()
    {
        public TOptions Options { get; } = options;

        public TOptions GetNamedOptions(string name)
        {
            return Options;
        }
    }
}
