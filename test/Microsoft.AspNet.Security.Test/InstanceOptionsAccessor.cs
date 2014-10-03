// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security
{
    public class InstanceOptionsAccessor<TOptions> : IOptionsAccessor<TOptions> where TOptions : class, new()
    {
        public InstanceOptionsAccessor(TOptions options)
        {
            Options = options;
        }

        public TOptions Options { get; private set; }

        public TOptions GetNamedOptions(string name)
        {
            return Options;
        }
    }
}
