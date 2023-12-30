using System;
using System.Linq;
using Xunit;
using DotNetEnv.Extensions;

namespace DotNetEnvTraverse.Tests
{
    public class TraverseTests
    {
        public TraverseTests ()
        {
            Environment.SetEnvironmentVariable("TEST", null);
            Environment.SetEnvironmentVariable("NAME", null);
        }

        [Fact]
        public void LoadDotenvTraverse()
        {
            var kvps = DotNetEnv.Env.TraversePath().Load().ToArray();
            Assert.Single(kvps);
            var dict = kvps.ToDotEnvDictionary();
            Assert.Equal("here", dict["TEST"]);
            Assert.Equal("here", Environment.GetEnvironmentVariable("TEST"));
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadRenamedDotenvTraverse()
        {
            var kvps = DotNetEnv.Env.TraversePath().Load("./.env").ToArray();
            Assert.Single(kvps);
            var dict = kvps.ToDotEnvDictionary();
            Assert.Equal("here", dict["TEST"]);
            Assert.Equal("here", Environment.GetEnvironmentVariable("TEST"));
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadRenamedDotenvMuchTraverse()
        {
            var kvps = DotNetEnv.Env.TraversePath().Load(".env_much_higher").ToArray();
            Assert.Single(kvps);
            var dict = kvps.ToDotEnvDictionary();
            Assert.Equal("See DotNetEnvTraverse.Tests for why this is here", dict["TEST"]);
            Assert.Equal("See DotNetEnvTraverse.Tests for why this is here", Environment.GetEnvironmentVariable("TEST"));
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadOnlyDirectoryDotenvTraverse()
        {
            var kvps = DotNetEnv.Env.TraversePath().Load("./").ToArray();
            Assert.Single(kvps);
            var dict = kvps.ToDotEnvDictionary();
            Assert.Equal("here", dict["TEST"]);
            Assert.Equal("here", Environment.GetEnvironmentVariable("TEST"));
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void DoNotLoadSkippedDotenv()
        {
            var kvps = DotNetEnv.Env.TraversePath().Load("../../../../../../").ToArray();
            Assert.Empty(kvps);
            Assert.Null(Environment.GetEnvironmentVariable("TEST"));
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));
        }
    }
}
