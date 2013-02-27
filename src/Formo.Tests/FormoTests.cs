﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using NUnit.Framework;

namespace Formo.Tests
{
    public class When_using_namespaced_values_that_exist : ConfigurationTestBase
    {
        [Test]
        public void Can_retreive_namespaced_settings_as_properties()
        {
            var key = configuration.ThirdPartyApi.Key;
            var secret = configuration.ThirdPartyApi.Secret;

            Assert.That(key, Is.EqualTo("something"));
            Assert.That(secret, Is.EqualTo("blah"));
        }

        [Test]
        public void Can_retreive_namespaced_settings_as_methods()
        {
            var thing = configuration.Namespace.Thing<decimal>();
            var words = configuration.Namespace.Words<string>();

            Assert.That(thing, Is.EqualTo(123.45m));
            Assert.That(words, Is.EqualTo("hello world!"));
        }

        [Test]
        public void Can_retreive_namespaced_settings_as_methods_with_defaults()
        {
            var thing = configuration.Namespace().MissingThing(99.99m);
            var words = configuration.Namespace.MissingWords("hi mom!");

            Assert.That(thing, Is.EqualTo(99.99m));
            Assert.That(words, Is.EqualTo("hi mom!"));
        }
    }

    public class When_using_namespaced_values_that_dont_exist : ConfigurationTestBase
    {
        [Test]
        public void Expect_exception_if_deep_namespace_fails_early_on_property()
        {
            Assert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() =>
            {
                var _ = configuration.Null.Key;
            });
        }

        [Test]
        public void Expect_helpful_exception_if_deep_namespace_fails_on_property()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                var _ = configuration.ThirdPartyApi.Null;
            });
            Assert.That(ex.Message, Contains.Substring("ThirdPartyApi.Null"));
        }

        [Test]
        public void Expect_helpful_exception_if_deep_namespace_fails_on_method()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                var _ = configuration.Namespace.MissingSetting();
            });
            Assert.That(ex.Message, Contains.Substring("Namespace.MissingSetting"));
        }
    }

    public class When_forced_to_use_a_string_key : ConfigurationTestBase
    {
        public When_forced_to_use_a_string_key()
        {
            
        }
        public When_forced_to_use_a_string_key(string sectionName)
            : base(sectionName)
        {
        }

        [Test]
        public void Get_method_should_return_value()
        {
            var actual = configuration.Get("weird:key");

            Assert.That(actual, Is.EqualTo("some value"));
        }

        [Test]
        public void Get_method_should_work_with_type_parameters()
        {
            var actual = configuration.Get<int>("NumberOfRetries");

            Assert.That(actual, Is.TypeOf<int>());
        }
    }

    public class When_using_typed_configuration_values : ConfigurationTestBase
    {
        private dynamic germanConfiguration;
        private string _sectionName;
        public When_using_typed_configuration_values()
        {
            
        }
        public When_using_typed_configuration_values(string sectionName)
            : base(sectionName)
        {
            _sectionName = sectionName;
        }

        [SetUp]
        public void SetUp()
        {
            germanConfiguration = new Configuration(_sectionName, new CultureInfo("de"));
        }

        [Test]
        public void Method_should_convert_to_int()
        {
            var actual = configuration.NumberOfRetries<int>();

            Assert.That(actual, Is.EqualTo(12));
        }

        [Test]
        public void Method_should_convert_to_decimal()
        {
            var actual = configuration.AcceptableFailurePercentage<decimal>();

            Assert.That(actual, Is.EqualTo(1.05));
        }

        [Test]
        public void Method_should_convert_to_DateTime()
        {
            var actual = configuration.ApplicationBuildDate<DateTime>();

            Assert.That(actual, Is.EqualTo(new DateTime(1999, 11, 4, 6, 23, 0)));
        }

        [Test]
        public void Method_should_convert_to_DateTime_of_ConfiguredCulture()
        {
            var actual = germanConfiguration.GermanDate<DateTime>();

            Assert.That(actual, Is.EqualTo(new DateTime(2002, 1, 22)));
        }

        [Test]
        public void Method_should_convert_to_bool()
        {
            var actual = configuration.IsLoggingEnabled<bool>();

            Assert.That(actual, Is.EqualTo(true));
        }
    }

    public class When_key_is_in_configuration_file : ConfigurationTestBase
    {
        public When_key_is_in_configuration_file()
        {
            
        }
        public When_key_is_in_configuration_file(string sectionName)
            : base(sectionName)
        {
        }

        [Test]
        public void Property_should_return_expected_value()
        {
            Assert.AreEqual("a0c5837ebb094b578b436f03121bb022", configuration.ApiKey);
        }

        [Test]
        public void Method_should_return_expected_value()
        {
            Assert.AreEqual("a0c5837ebb094b578b436f03121bb022", configuration.ApiKey());
        }

        [Test]
        public void Method_should_ignore_defaults()
        {
            var actual = configuration.ApiKey("defaultvalue");
            Assert.AreNotEqual("defaultvalue", actual);
        }

        [Test]
        public void Method_should_ignore_many_defaults()
        {
            var actual = configuration.ApiKey("test", "another");
            Assert.AreNotEqual("test", actual);
            Assert.AreNotEqual("another", actual);
        }
    }

    public class When_key_isnt_in_configuration_file : ConfigurationTestBase
    {
        public When_key_isnt_in_configuration_file()
        {
            
        }
        public When_key_isnt_in_configuration_file(string sectionName)
            : base(sectionName)
        {
        }

        [Test]
        public void Property_should_be_null()
        {
            Assert.Null(configuration.Missing);
        }

        [Test]
        public void Method_should_be_null()
        {
            Assert.Null(configuration.Misssing());
        }

        [Test]
        public void Method_with_param_should_return_first()
        {
            Assert.AreEqual("blargh", configuration.Missing("blargh"));
        }

        [Test]
        public void Method_with_many_params_should_return_first_non_null()
        {
            string first = null;
            var second = default(string);
            var third = "i exist";
            Assert.AreEqual(third, configuration.Missing(first, second, third));
        }

        [Test]
        public void Method_looking_for_bool_should_behave_as_ConfigurationManager()
        {
            var key = "IsSettingMissing";
            var expected = ConfigurationManager.AppSettings[key];
            var actual = configuration.IsSettingMissing<bool>();

            Assert.AreSame(expected, actual);
        }
    }

    public class ConfigurationTestBase
    {
        public ConfigurationTestBase(string sectionName)
        {
            configuration = new Configuration(sectionName);
        }

        public ConfigurationTestBase()
        {
            configuration = new Configuration();
        }

        protected dynamic configuration;
    }
}
