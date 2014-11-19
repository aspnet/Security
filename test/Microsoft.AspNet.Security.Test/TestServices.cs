// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Security.DataProtection;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Security
{
    public static class TestServices
    {
        public static IServiceProvider CreateTestServices()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<IApplicationEnvironment, TestApplicationEnvironment>();
            collection.Add(DataProtectionServices.GetDefaultServices());
            collection.AddSingleton<IServiceManifest, TestManifest>();
            return collection.BuildServiceProvider();
        }

        private class TestManifest : IServiceManifest
        {
            public IEnumerable<Type> Services
            {
                get
                {
                    return new Type[] { typeof(IApplicationEnvironment), typeof(IDataProtectionProvider) };
                }
            }
        }

    }
}