// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.Options.Tests
{
    public class FakeOptions
    {
        public string Message = "";
    }

    public class OptionsFactoryTest
    {
        [Fact]
        public void CanResolveNamedOptions()
        {
            var services = new ServiceCollection().AddOptionsFactory();

            services.Configure<FakeOptions>("1", options =>
            {
                options.Message = "one";
            });
            services.Configure<FakeOptions>("2", options =>
            {
                options.Message = "two";
            });

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Equal("one", option.Get("1").Message);
            Assert.Equal("two", option.Get("2").Message);
        }
    }
}