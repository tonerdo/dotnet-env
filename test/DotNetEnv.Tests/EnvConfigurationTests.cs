using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DotNetEnv.Tests
{
    /// <summary>
    /// This class tests a few basic operations, but the settings are mostly passed directly
    /// to `DotNetEnv.Env.Load()`
    /// </summary>
    public class EnvConfigurationTests
    {
        [Fact]
        public void AddSourceToBuilderAndLoad()
        {
            var config = new ConfigurationBuilder()
                .AddDotNetEnv()
                .Build();

            Assert.Empty(config["EMPTY"]);
            Assert.Equal("'", config["QUOTE"]);
            Assert.Equal("https://github.com/tonerdo", config["URL"]);
            Assert.Equal("user=test;password=secret", config["CONNECTION"]);
            Assert.Equal("  leading and trailing white space   ", config["WHITEBOTH"]);
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", config["SSL_CERT"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadDotenvHigherSkip()
        {
            // ./DotNetEnv.Tests/bin/Debug/netcoreapp3.1/DotNetEnv.Tests.dll -- get to the ./ (root of `test` folder)
            var config = new ConfigurationBuilder()
                .AddDotNetEnv("../../../../")
                .Build();

            Assert.Null(config["NAME"]);
            Assert.Equal("here", config["TEST"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMulti()
        {
            var config = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" })
                .Build();

            Assert.Equal("Other", config["NAME"]);
        }

        [Fact]
        public void AddSourceToBuilderAndLoadMultiWithNoClobber()
        {
            var config = new ConfigurationBuilder()
                .AddDotNetEnvMulti(new[] { "./.env", "./.env2" }, LoadOptions.NoClobber())
                .Build();

            Assert.Equal("Toni", config["NAME"]);
        }

        [Fact]
        public void AddSourceToBuilderAndFileDoesNotExist()
        {
            var config = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_DNE")
                .Build();

            Assert.Empty(config.AsEnumerable());
        }

        [Fact]
        public void AddSourceToBuilderAndGetSection()
        {
            var config = new ConfigurationBuilder()
                .AddDotNetEnv("./.env_sections")
                .Build();

            var section = config.GetSection("SECTION");

            Assert.Equal("value1", section["Key1"]);
            Assert.Equal("value2", section["Key2"]);
        }
    }
}
