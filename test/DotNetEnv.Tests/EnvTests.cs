using System;
using Xunit;

namespace DotNetEnv.Tests
{
    public class EnvTests
    {
        [Fact]
        public void LoadTest()
        {
            DotNetEnv.Env.Load();
            Assert.Equal(Environment.GetEnvironmentVariable("NAME"), "Toni");
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tonerdo");
            Assert.Equal(Environment.GetEnvironmentVariable("CONNECTION"), "user=test;password=secret");
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
        }

        [Fact]
        public void LoadPathTest()
        {
            DotNetEnv.Env.Load("./.env2");
            Assert.Equal(Environment.GetEnvironmentVariable("IP"), "127.0.0.1");
            Assert.Equal(Environment.GetEnvironmentVariable("PORT"), "8080");
            Assert.Equal(Environment.GetEnvironmentVariable("DOMAIN"), "example.com");
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment");
        }

        [Fact]
        public void LoadArgsTest()
        {
            DotNetEnv.Env.Load(false);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
            DotNetEnv.Env.Load(true);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "  leading and trailing white space   ");
        }

        [Fact]
        public void LoadPathArgsTest()
        {
            DotNetEnv.Env.Load("./.env2", false);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment");
            DotNetEnv.Env.Load("./.env2", true);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "  leading white space followed by comment  ");
        }
    }
}
