using System;
using Xunit;

namespace DotNetEnv.Tests
{
    public class Env
    {
        [Fact]
        public void LoadTest()
        {
            DotNetEnv.Env.Load();
            Assert.Equal(Environment.GetEnvironmentVariable("NAME"), "Toni");
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tsolarin");
        }

        [Fact]
        public void LoadPathTest()
        {
            DotNetEnv.Env.Load("./.env2");
            Assert.Equal(Environment.GetEnvironmentVariable("IP"), "127.0.0.1");
            Assert.Equal(Environment.GetEnvironmentVariable("PORT"), "8080");
            Assert.Equal(Environment.GetEnvironmentVariable("DOMAIN"), "example.com");
        }

        [Fact]
        public void MultipleEquals()
        {
            DotNetEnv.Env.Load("./.env3");
            Assert.Equal("OU=Default People,DC=google,DC=com", Environment.GetEnvironmentVariable("DN"));
        }
        
        [Fact]
        public void IgnoreCommentOptions()
        {
            DotNetEnv.Env.Load("./.env3", false);
            Assert.Equal("Google#Facebook", Environment.GetEnvironmentVariable("PASSWORD"));
        }
    }
}
