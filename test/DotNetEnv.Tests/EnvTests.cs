using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace DotNetEnv.Tests
{
    public class EnvTests : IDisposable
    {
        readonly IDictionary<string, string> _oldEnvironment;

        static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Saves the environment before each test is run.
        /// </summary>

        public EnvTests() =>
            _oldEnvironment =
                Environment.GetEnvironmentVariables()
                           .Cast<DictionaryEntry>()
                           .ToDictionary(e => e.Key.ToString(),
                                         e => e.Value.ToString());

        /// <summary>
        /// Resets the environment after each test is run.
        /// </summary>

        public void Dispose()
        {
            foreach (var var in _oldEnvironment)
                Environment.SetEnvironmentVariable(var.Key, var.Value);

            var comparer = IsWindows
                         ? StringComparer.OrdinalIgnoreCase
                         : StringComparer.Ordinal;

            var newNames = Environment.GetEnvironmentVariables().Keys
                                      .Cast<string>()
                                      .Except(_oldEnvironment.Keys,
                                              comparer);

            foreach (var name in newNames)
                Environment.SetEnvironmentVariable(name, null);
        }

        [Fact]
        public void LoadTest()
        {
            DotNetEnv.Env.Load();
            Assert.Equal(Environment.GetEnvironmentVariable("NAME"), "Toni");
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tonerdo");
            Assert.Equal(Environment.GetEnvironmentVariable("CONNECTION"), "user=test;password=secret");
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash");
        }

        [Fact]
        public void LoadPathTest()
        {
            DotNetEnv.Env.Load("./.env2");
            Assert.Equal(Environment.GetEnvironmentVariable("IP"), "127.0.0.1");
            Assert.Equal(Environment.GetEnvironmentVariable("PORT"), "8080");
            Assert.Equal(Environment.GetEnvironmentVariable("DOMAIN"), "example.com");
            Assert.Equal(Environment.GetEnvironmentVariable("EMBEDEXPORT"), "some text export other text");
            Assert.Equal(Environment.GetEnvironmentVariable("COMMENTLEAD"), null);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment");
            Assert.Equal(Environment.GetEnvironmentVariable("UNICODE"), "® 🚀 日本");
        }

        [Fact]
        public void LoadArgsTest()
        {
            DotNetEnv.Env.Load(true);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
            DotNetEnv.Env.Load(false);
            Assert.Equal(Environment.GetEnvironmentVariable("  WHITEBOTH  "), "  leading and trailing white space   ");
            DotNetEnv.Env.Load(true, true);
            Assert.Equal(Environment.GetEnvironmentVariable("PASSWORD"), "Google");
            DotNetEnv.Env.Load(true, false);
            Assert.Equal(Environment.GetEnvironmentVariable("PASSWORD"), "Google#Facebook");
            DotNetEnv.Env.Load(true, true, true);
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash");
            DotNetEnv.Env.Load(true, true, false);
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "\"SPECIAL STUFF---\\nLONG-BASE64\\ignore\"slash\"");
        }

        [Fact]
        public void LoadPathArgsTest()
        {
            DotNetEnv.Env.Load("./.env2", true, true);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment");
            DotNetEnv.Env.Load("./.env2", false, true);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "  leading white space followed by comment  ");
            DotNetEnv.Env.Load("./.env2", true, false);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment  # comment");
            DotNetEnv.Env.Load("./.env2", false, false);
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "  leading white space followed by comment  # comment");
            DotNetEnv.Env.Load("./.env2", false, false, true);
            Assert.Equal(Environment.GetEnvironmentVariable("UNICODE"), "® 🚀 日本");
            DotNetEnv.Env.Load("./.env2", false, false, false);
            Assert.Equal(Environment.GetEnvironmentVariable("UNICODE"), "'\\u00ae \\U0001F680 日本'");
        }

        [Theory]
        [InlineData("URL", "URL")]
        [InlineData("url", "URL")]
        public void LoadNoClobberTest(string setName, string getName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                setName = getName;

            var expected = "totally the original value";
            Environment.SetEnvironmentVariable(setName, expected);
            DotNetEnv.Env.Load(false, false, false, false);
            Assert.Equal(Environment.GetEnvironmentVariable(getName), expected);

            Environment.SetEnvironmentVariable(setName, "i'm going to be overwritten");
            DotNetEnv.Env.Load(false, false, false, true);
            Assert.Equal(Environment.GetEnvironmentVariable(getName), "https://github.com/tonerdo");
        }
    }
}
