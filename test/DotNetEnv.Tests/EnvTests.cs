using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace DotNetEnv.Tests
{
    public class EnvTests
    {
        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        [Fact]
        public void LoadTest()
        {
            DotNetEnv.Env.Load();
            Assert.Equal(Environment.GetEnvironmentVariable("NAME"), "Toni");
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Equal(Environment.GetEnvironmentVariable("EMPTY"), null);
            Assert.Equal(Environment.GetEnvironmentVariable("QUOTE"), "'");
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
        public void LoadStreamTest()
        {
            DotNetEnv.Env.Load(File.OpenRead("./.env"));
            Assert.Equal(Environment.GetEnvironmentVariable("NAME"), "Toni");
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Equal(Environment.GetEnvironmentVariable("EMPTY"), null);
            Assert.Equal(Environment.GetEnvironmentVariable("QUOTE"), "'");
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tonerdo");
            Assert.Equal(Environment.GetEnvironmentVariable("CONNECTION"), "user=test;password=secret");
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash");
        }
        
        [Fact]
        public void LoadLinesTest()
        {
            DotNetEnv.Env.Load(File.ReadAllLines("./.env"));
            Assert.Equal(Environment.GetEnvironmentVariable("NAME"), "Toni");
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Equal(Environment.GetEnvironmentVariable("EMPTY"), null);
            Assert.Equal(Environment.GetEnvironmentVariable("QUOTE"), "'");
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tonerdo");
            Assert.Equal(Environment.GetEnvironmentVariable("CONNECTION"), "user=test;password=secret");
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash");
        }

        [Fact]
        public void LoadArgsTest()
        {
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true));
            Assert.Equal(Environment.GetEnvironmentVariable("WHITEBOTH"), "leading and trailing white space");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false));
            Assert.Equal(Environment.GetEnvironmentVariable("  WHITEBOTH  "), "  leading and trailing white space   ");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true));
            Assert.Equal(Environment.GetEnvironmentVariable("PASSWORD"), "Google");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, false));
            Assert.Equal(Environment.GetEnvironmentVariable("PASSWORD"), "Google#Facebook");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true, true));
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true, false));
            Assert.Equal(Environment.GetEnvironmentVariable("SSL_CERT"), "\"SPECIAL STUFF---\\nLONG-BASE64\\ignore\"slash\"");
        }

        [Fact]
        public void LoadPathArgsTest()
        {
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(true, true));
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment");
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, true));
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "  leading white space followed by comment  ");
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(true, false));
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "leading white space followed by comment  # comment");
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false));
            Assert.Equal(Environment.GetEnvironmentVariable("WHITELEAD"), "  leading white space followed by comment  # comment");
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false, true));
            Assert.Equal(Environment.GetEnvironmentVariable("UNICODE"), "® 🚀 日本");
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false, false));
            Assert.Equal(Environment.GetEnvironmentVariable("UNICODE"), "'\\u00ae \\U0001F680 日本'");
        }

        [Fact]
        public void LoadNoClobberTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false, false, false, false));
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), expected);

            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false, false, false, true));
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tonerdo");
        }

        [Fact]
        public void LoadOsCasingTest()
        {
            Environment.SetEnvironmentVariable("CASING", "neither");
            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), IsWindows ? "neither" : "lower");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), "neither");

            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), "lower");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), IsWindows ? "lower" : "neither");

            Environment.SetEnvironmentVariable("CASING", null);
            Environment.SetEnvironmentVariable("casing", "neither");
            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), "neither");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), IsWindows ? "neither" : null);

            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), "lower");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), IsWindows ? "lower" : null);
        }

        #region "Obsolete parameters list"

        [Fact]
        public void ObsoleteLoadArgsTest()
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
        public void ObsoleteLoadPathArgsTest()
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

        [Fact]
        public void ObsoleteLoadNoClobberTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(false, false, false, false);
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), expected);

            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(false, false, false, true);
            Assert.Equal(Environment.GetEnvironmentVariable("URL"), "https://github.com/tonerdo");
        }

        [Fact]
        public void ObsoleteLoadOsCasingTest()
        {
            Environment.SetEnvironmentVariable("CASING", "neither");
            DotNetEnv.Env.Load("./.env3", clobberExistingVars: false);
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), IsWindows ? "neither" : "lower");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), "neither");

            DotNetEnv.Env.Load("./.env3", clobberExistingVars: true);
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), "lower");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), IsWindows ? "lower" : "neither");

            Environment.SetEnvironmentVariable("CASING", null);
            Environment.SetEnvironmentVariable("casing", "neither");
            DotNetEnv.Env.Load("./.env3", clobberExistingVars: false);
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), "neither");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), IsWindows ? "neither" : null);

            DotNetEnv.Env.Load("./.env3", clobberExistingVars: true);
            Assert.Equal(Environment.GetEnvironmentVariable("casing"), "lower");
            Assert.Equal(Environment.GetEnvironmentVariable("CASING"), IsWindows ? "lower" : null);
        }

        #endregion
    }
}
