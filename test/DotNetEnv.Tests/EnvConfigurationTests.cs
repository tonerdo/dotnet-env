using System;
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

        public void Dispose()
        {
            foreach (var conf in this.configuration.AsEnumerable())
            {
                Environment.SetEnvironmentVariable(conf.Key, null);
            }
        }


        [Fact]
        public void AddSourceToBuilderAndLoad()
        {
            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnv(options: LoadOptions.NoEnvVars())
                .Build();

            Assert.Empty(this.configuration["EMPTY"]);
            Assert.Equal("'", this.configuration["QUOTE"]);
            Assert.Equal("https://github.com/tonerdo", this.configuration["URL"]);
            Assert.Equal("user=test;password=secret", this.configuration["CONNECTION"]);
            Assert.Equal("  leading and trailing white space   ", this.configuration["WHITEBOTH"]);
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", this.configuration["SSL_CERT"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadDotenvHigherSkip()
        {
            // ./DotNetEnv.Tests/bin/Debug/netcoreapp3.1/DotNetEnv.Tests.dll -- get to the ./ (root of `test` folder)
            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnv("../../../../", LoadOptions.NoEnvVars())
                .Build();

            Assert.Null(this.configuration["NAME"]);
            Assert.Equal("here", this.configuration["TEST"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMulti()
        {
            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" }, LoadOptions.NoEnvVars())
                .Build();

            Assert.Equal("Other", this.configuration["NAME"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMultiWithNoClobber()
        {
            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" }, LoadOptions.NoEnvVars().NoClobber())
                .Build();

            Assert.Equal("Toni", this.configuration["NAME"]);
        }

        [Fact]
        public void AddSourceToBuilderAndFileDoesNotExist()
        {
            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_DNE", LoadOptions.NoEnvVars())
                .Build();

            Assert.Empty(this.configuration.AsEnumerable());
        }

        [Fact]
        public void AddSourceToBuilderAndGetSection()
        {
            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_sections", LoadOptions.NoEnvVars())
                .Build();

            var section = this.configuration.GetSection("SECTION");

            Assert.Equal("value1", section["Key1"]);
            Assert.Equal("value2", section["Key2"]);
        }

        [Fact()]
        public void AddSourceToBuilderAndParseInterpolatedTest()
        {
            Environment.SetEnvironmentVariable("EXISTING_ENVIRONMENT_VARIABLE", "value");
            Environment.SetEnvironmentVariable("DNE_VAR", null);

            // Have to remove since it's recursive and can be set by the `EnvTests.cs`
            Environment.SetEnvironmentVariable("TEST4", null);

            this.configuration = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_embedded")
                .Build();

            Assert.Equal("test", this.configuration["TEST"]);
            Assert.Equal("test1", this.configuration["TEST1"]);
            Assert.Equal("test", this.configuration["TEST2"]);
            Assert.Equal("testtest", this.configuration["TEST3"]);
            Assert.Equal("testtest1", this.configuration["TEST4"]);

            Assert.Equal("test:testtest1 $$ '\" ® and test1", this.configuration["TEST5_DOUBLE"]);
            Assert.Equal("$TEST:$TEST4 \\$\\$ \" \\uae and $TEST1", this.configuration["TEST5_SINGLE"]);
            Assert.Equal("test:testtest1\\uaeandtest1", this.configuration["TEST5_UNQUOTED"]);

            Assert.Equal("value1", this.configuration["FIRST_KEY"]);
            Assert.Equal("value2andvalue1", this.configuration["SECOND_KEY"]);
            // EXISTING_ENVIRONMENT_VARIABLE already set to "value"
            Assert.Equal("value;andvalue3", this.configuration["THIRD_KEY"]);
            // DNE_VAR does not exist (has no value)
            Assert.Equal(";nope", this.configuration["FOURTH_KEY"]);

            Assert.Equal("^((?!Everyone).)*$", this.configuration["GROUP_FILTER_REGEX"]);

            Assert.Equal("value$", this.configuration["DOLLAR1_U"]);
            Assert.Equal("valuevalue$$", this.configuration["DOLLAR2_U"]);
            Assert.Equal("value$.$", this.configuration["DOLLAR3_U"]);
            Assert.Equal("value$$", this.configuration["DOLLAR4_U"]);

            Assert.Equal("value$", this.configuration["DOLLAR1_S"]);
            Assert.Equal("value$DOLLAR1_S$", this.configuration["DOLLAR2_S"]);
            Assert.Equal("value$.$", this.configuration["DOLLAR3_S"]);
            Assert.Equal("value$$", this.configuration["DOLLAR4_S"]);

            Assert.Equal("value$", this.configuration["DOLLAR1_D"]);
            Assert.Equal("valuevalue$$", this.configuration["DOLLAR2_D"]);
            Assert.Equal("value$.$", this.configuration["DOLLAR3_D"]);
            Assert.Equal("value$$", this.configuration["DOLLAR4_D"]);
        }
    }
}
