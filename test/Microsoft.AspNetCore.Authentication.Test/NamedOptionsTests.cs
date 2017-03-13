// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        [Fact]
        public void FactoryValidatesOptions()
        {
            var services = new ServiceCollection().AddOptionsFactory();
            services.Validate<FakeOptions>("1", options =>
            {
                if (string.IsNullOrEmpty(options.Message))
                {
                    throw new ArgumentNullException();
                }
            });

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Throws<ArgumentNullException>(() => option.Get("1"));
        }

        [Fact]
        public void FactoryCanConfigureAllOptions()
        {
            var services = new ServiceCollection().AddOptionsFactory();
            services.ConfigureAll<FakeOptions>(o => o.Message = "Default");

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Equal("Default", option.Get("1").Message);
            Assert.Equal("Default", option.Get("2").Message);
        }

        [Fact]
        public void FactoryCanValidateAllOptions()
        {
            var services = new ServiceCollection().AddOptionsFactory();
            services.ValidateAll<FakeOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.Message))
                {
                    throw new ArgumentNullException();
                }
            });

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Throws<ArgumentNullException>(() => option.Get("1"));
            Assert.Throws<ArgumentNullException>(() => option.Get("2"));
        }

        [Fact]
        public void FactoryConfigureAndValidateAllPlayWellTogether()
        {
            var services = new ServiceCollection().AddOptionsFactory();
            services.ConfigureAll<FakeOptions>(o => o.Message = "Default");
            services.Configure<FakeOptions>("NotDefault", o => o.Message = "NotDefault");
            services.Configure<FakeOptions>("Throws", o => o.Message = null);
            services.Validate<FakeOptions>("NotDefault", options =>
            {
                if (options.Message == "Default")
                {
                    throw new Exception();
                }
            });

            services.ValidateAll<FakeOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.Message))
                {
                    throw new ArgumentNullException();
                }
            });

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Equal("NotDefault", option.Get("NotDefault").Message);
            Assert.Equal("Default", option.Get("Default").Message);
            Assert.Throws<ArgumentNullException>(() => option.Get("Throws"));
        }

        [Fact]
        public void FactoryConfiguresInRegistrationOrder()
        {
            var services = new ServiceCollection().AddOptionsFactory();
            services.Configure<FakeOptions>("-", o => o.Message += "-");
            services.ConfigureAll<FakeOptions>(o => o.Message += "A");
            services.Configure<FakeOptions>("+", o => o.Message += "+");
            services.ConfigureAll<FakeOptions>(o => o.Message += "B");
            services.ConfigureAll<FakeOptions>(o => o.Message += "C");
            services.Configure<FakeOptions>("+", o => o.Message += "+");
            services.Configure<FakeOptions>("-", o => o.Message += "-");

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Equal("ABC", option.Get("1").Message);
            Assert.Equal("A+BC+", option.Get("+").Message);
            Assert.Equal("-ABC-", option.Get("-").Message);
        }

        [Fact]
        public void FactoryValidatesInRegistrationOrder()
        {
            var services = new ServiceCollection().AddOptionsFactory();
            services.Validate<FakeOptions>("-", o => o.Message += "-");
            services.ValidateAll<FakeOptions>(o => o.Message += "A");
            services.Validate<FakeOptions>("+", o => o.Message += "+");
            services.ValidateAll<FakeOptions>(o => o.Message += "B");
            services.ValidateAll<FakeOptions>(o => o.Message += "C");
            services.Validate<FakeOptions>("+", o => o.Message += "+");
            services.Validate<FakeOptions>("-", o => o.Message += "-");

            var sp = services.BuildServiceProvider();
            var option = sp.GetRequiredService<IOptionsFactory<FakeOptions>>();
            Assert.Equal("ABC", option.Get("1").Message);
            Assert.Equal("A+BC+", option.Get("+").Message);
            Assert.Equal("-ABC-", option.Get("-").Message);
        }

    }
}