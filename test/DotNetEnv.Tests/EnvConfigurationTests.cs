using System;
using System.Collections.Generic;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DotNetEnv.Tests
{
    /// <summary>
    /// This class tests a few basic operations, but the settings are mostly passed directly
    /// to `DotNetEnv.Env.Load()`
    /// </summary>
    public class EnvConfigurationTests : IDisposable
    {
        private IConfigurationRoot configuration;

        private const string EV_TEST = "ENVVAR_TEST";
        private const string EV_DNE = "EV_DNE";
        private const string EV_TEST_1 = "EV_TEST_1";
        private const string EV_TEST_2 = "EV_TEST_2";

        private readonly Dictionary<string,string> oldEnvvars = new();
        private static readonly string[] ALL_EVS = { EV_TEST, EV_DNE, EV_TEST_1, EV_TEST_2 };

        public EnvConfigurationTests ()
        {
            foreach (var ev in ALL_EVS)
            {
                oldEnvvars[ev] = Environment.GetEnvironmentVariable(ev);
            }

            Environment.SetEnvironmentVariable(EV_TEST, "ENV value");
        }

        public void Dispose()
        {
            foreach (var conf in configuration.AsEnumerable())
            {
                Environment.SetEnvironmentVariable(conf.Key, null);
            }

            foreach (var ev in ALL_EVS)
            {
                Environment.SetEnvironmentVariable(ev, oldEnvvars[ev]);
            }
        }

        [Fact]
        public void AddSourceToBuilderAndLoad()
        {
            configuration = new ConfigurationBuilder()
                .AddDotNetEnv(options: LoadOptions.NoEnvVars())
                .Build();

            Assert.Empty(configuration["EMPTY"]);
            Assert.Equal("'", configuration["QUOTE"]);
            Assert.Equal("https://github.com/tonerdo", configuration["URL"]);
            Assert.Equal("user=test;password=secret", configuration["CONNECTION"]);
            Assert.Equal("  leading and trailing white space   ", configuration["WHITEBOTH"]);
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", configuration["SSL_CERT"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadDotenvHigherSkip()
        {
            // ./DotNetEnv.Tests/bin/Debug/netcoreapp3.1/DotNetEnv.Tests.dll -- get to the ./ (root of `test` folder)
            configuration = new ConfigurationBuilder()
                .AddDotNetEnv("../../../../", LoadOptions.NoEnvVars())
                .Build();

            Assert.Null(configuration["NAME"]);
            Assert.Equal("here", configuration["TEST"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMulti()
        {
            configuration = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" }, LoadOptions.NoEnvVars())
                .Build();

            Assert.Equal("Other", configuration["NAME"]);
            Assert.Equal("overridden_2", configuration["ENVVAR_TEST"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMultiWithNoClobber()
        {
            configuration = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" }, LoadOptions.NoEnvVars().NoClobber())
                .Build();

            Assert.Equal("Toni", configuration["NAME"]);
            Assert.Equal("ENV value", configuration["ENVVAR_TEST"]);
            Assert.Equal("ENV value", configuration["ClobberEnvVarTest"]); // should contain ENVVAR_TEST from EnvironmentVariable
            Assert.Equal("https://github.com/tonerdo", configuration["UrlFromVariable"]); // should contain Url from .env
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMultiWithClobber()
        {
            configuration = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" }, LoadOptions.NoEnvVars())
                .Build();

            Assert.Equal("Other", configuration["NAME"]);
            Assert.Equal("overridden_2", configuration["ENVVAR_TEST"]);
            Assert.Equal("overridden_2", configuration["ClobberEnvVarTest"]); // should contain ENVVAR_TEST from .env
            Assert.Equal("https://github.com/tonerdo", configuration["UrlFromPreviousEnv"]); // should contain Url from .env
        }

        [Fact]
        public void AddSourceToBuilderAndFileDoesNotExist()
        {
            configuration = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_DNE", LoadOptions.NoEnvVars())
                .Build();

            Assert.Empty(configuration.AsEnumerable());
        }

        [Fact]
        public void AddSourceToBuilderAndGetSection()
        {
            configuration = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_sections", LoadOptions.NoEnvVars())
                .Build();

            var section = configuration.GetSection("SECTION");

            Assert.Equal("value1", section["Key1"]);
            Assert.Equal("value2", section["Key2"]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddSourceToBuilderAndParseInterpolatedTest(bool setEnvVars)
        {
            Environment.SetEnvironmentVariable("EXISTING_ENVIRONMENT_VARIABLE", "value");
            Environment.SetEnvironmentVariable("DNE_VAR", null);

            // Have to remove since it's recursive and can be set by the `EnvTests.cs`
            Environment.SetEnvironmentVariable("TEST4", null);

            configuration = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_embedded", new LoadOptions(setEnvVars: setEnvVars))
                .Build();

            Assert.Equal("test", configuration["TEST"]);
            Assert.Equal("test1", configuration["TEST1"]);
            Assert.Equal("test", configuration["TEST2"]);
            Assert.Equal("testtest", configuration["TEST3"]);
            Assert.Equal("testtest1", configuration["TEST4"]);

            Assert.Equal("test:testtest1 $$ '\" ® and test1", configuration["TEST5_DOUBLE"]);
            Assert.Equal("$TEST:$TEST4 \\$\\$ \" \\uae and $TEST1", configuration["TEST5_SINGLE"]);
            Assert.Equal("test:testtest1\\uaeandtest1", configuration["TEST5_UNQUOTED"]);

            Assert.Equal("value1", configuration["FIRST_KEY"]);
            Assert.Equal("value2andvalue1", configuration["SECOND_KEY"]);
            // EXISTING_ENVIRONMENT_VARIABLE already set to "value"
            Assert.Equal("value;andvalue3", configuration["THIRD_KEY"]);
            // DNE_VAR does not exist (has no value)
            Assert.Equal(";nope", configuration["FOURTH_KEY"]);

            Assert.Equal("^((?!Everyone).)*$", configuration["GROUP_FILTER_REGEX"]);

            Assert.Equal("value$", configuration["DOLLAR1_U"]);
            Assert.Equal("valuevalue$$", configuration["DOLLAR2_U"]);
            Assert.Equal("value$.$", configuration["DOLLAR3_U"]);
            Assert.Equal("value$$", configuration["DOLLAR4_U"]);

            Assert.Equal("value$", configuration["DOLLAR1_S"]);
            Assert.Equal("value$DOLLAR1_S$", configuration["DOLLAR2_S"]);
            Assert.Equal("value$.$", configuration["DOLLAR3_S"]);
            Assert.Equal("value$$", configuration["DOLLAR4_S"]);

            Assert.Equal("value$", configuration["DOLLAR1_D"]);
            Assert.Equal("valuevalue$$", configuration["DOLLAR2_D"]);
            Assert.Equal("value$.$", configuration["DOLLAR3_D"]);
            Assert.Equal("value$$", configuration["DOLLAR4_D"]);
        }
    }
}
