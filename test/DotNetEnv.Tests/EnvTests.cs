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
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
            Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
            Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
        }

        [Fact]
        public void LoadPathTest()
        {
            DotNetEnv.Env.Load("./.env2");
            Assert.Equal("127.0.0.1", Environment.GetEnvironmentVariable("IP"));
            Assert.Equal("8080", Environment.GetEnvironmentVariable("PORT"));
            Assert.Equal("example.com", Environment.GetEnvironmentVariable("DOMAIN"));
            Assert.Equal("some text export other text", Environment.GetEnvironmentVariable("EMBEDEXPORT"));
            Assert.Null(Environment.GetEnvironmentVariable("COMMENTLEAD"));
            Assert.Equal("leading white space followed by comment", Environment.GetEnvironmentVariable("WHITELEAD"));
            Assert.Equal("® 🚀 日本", Environment.GetEnvironmentVariable("UNICODE"));
        }

        [Fact]
        public void LoadStreamTest()
        {
            DotNetEnv.Env.Load(File.OpenRead("./.env"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
            Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
            Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
        }

        [Fact]
        public void LoadLinesTest()
        {
            DotNetEnv.Env.Load(File.ReadAllLines("./.env"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
            Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
            Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
        }

        [Fact]
        public void LoadArgsTest()
        {
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true));
            Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false));
            Assert.Equal("  leading and trailing white space   ", Environment.GetEnvironmentVariable("  WHITEBOTH  "));
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true));
            Assert.Equal("Google", Environment.GetEnvironmentVariable("PASSWORD"));
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, false));
            Assert.Equal("Google#Facebook", Environment.GetEnvironmentVariable("PASSWORD"));
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true, true));
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true, false));
            Assert.Equal("\"SPECIAL STUFF---\\nLONG-BASE64\\ignore\"slash\"", Environment.GetEnvironmentVariable("SSL_CERT"));
        }

        [Fact]
        public void LoadPathArgsTest()
        {
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(true, true));
            Assert.Equal("leading white space followed by comment", Environment.GetEnvironmentVariable("WHITELEAD"));
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, true));
            Assert.Equal("  leading white space followed by comment  ", Environment.GetEnvironmentVariable("WHITELEAD"));
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(true, false));
            Assert.Equal("leading white space followed by comment  # comment", Environment.GetEnvironmentVariable("WHITELEAD"));
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false));
            Assert.Equal("  leading white space followed by comment  # comment", Environment.GetEnvironmentVariable("WHITELEAD"));
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false, true));
            Assert.Equal("® 🚀 日本", Environment.GetEnvironmentVariable("UNICODE"));
            DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false, false));
            Assert.Equal("'\\u00ae \\U0001F680 日本'", Environment.GetEnvironmentVariable("UNICODE"));
        }

        [Fact]
        public void LoadNoClobberTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false, false, false, false));
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));

            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false, false, false, true));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
        }

        [Fact]
        public void LoadNoRequireEnvTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("URL", expected);
            // this env file Does Not Exist
            DotNetEnv.Env.Load("./.envDNE", new DotNetEnv.Env.LoadOptions());
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            // it didn't throw an exception and crash for a missing file
        }

        [Fact]
        public void LoadOsCasingTest()
        {
            Environment.SetEnvironmentVariable("CASING", "neither");
            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal(IsWindows ? "neither" : "lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal("neither", Environment.GetEnvironmentVariable("CASING"));

            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "lower" : "neither", Environment.GetEnvironmentVariable("CASING"));

            Environment.SetEnvironmentVariable("CASING", null);
            Environment.SetEnvironmentVariable("casing", "neither");
            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal("neither", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "neither" : null, Environment.GetEnvironmentVariable("CASING"));

            DotNetEnv.Env.Load("./.env3", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "lower" : null, Environment.GetEnvironmentVariable("CASING"));
        }

        [Fact]
        public void ParseVariablesTest()
        {
            DotNetEnv.Env.Load("./.env4");
            Assert.Equal("test", Environment.GetEnvironmentVariable("TEST"));
            Assert.Equal("test1", Environment.GetEnvironmentVariable("TEST1"));
            Assert.Equal("test", Environment.GetEnvironmentVariable("TEST2"));
            Assert.Equal("testtest", Environment.GetEnvironmentVariable("TEST3"));
            Assert.Equal("testtest1", Environment.GetEnvironmentVariable("TEST4"));
            Assert.Equal("test:testtest1 and test1", Environment.GetEnvironmentVariable("TEST5"));
        }

        [Fact]
        public void QuotedHashTest()
        {
            DotNetEnv.Env.Load("./.env5");
            Assert.Equal("1234", Environment.GetEnvironmentVariable("QTEST0"));
            Assert.Equal("1234#this is not a comment", Environment.GetEnvironmentVariable("QTEST1"));
            Assert.Equal("This is a test", Environment.GetEnvironmentVariable("QTEST2"));
            Assert.Equal("1234 #not_comment", Environment.GetEnvironmentVariable("QTEST3"));
            Assert.Equal("9876#this is not a comment", Environment.GetEnvironmentVariable("QTEST4"));
            Assert.Equal("This is a another test", Environment.GetEnvironmentVariable("QTEST5"));
            Assert.Equal("9876 #not_comment", Environment.GetEnvironmentVariable("QTEST6"));
            Assert.Equal("This isn't working", Environment.GetEnvironmentVariable("QTEST7"));
            Assert.Equal("Hi \"Bob\"!", Environment.GetEnvironmentVariable("QTEST8"));
            Assert.Equal("Hi \"Bob\"!", Environment.GetEnvironmentVariable("QTEST9"));
            Assert.Equal("This isn't working", Environment.GetEnvironmentVariable("QTEST10"));
            Assert.Equal("This isn't \"working\" #amiright?", Environment.GetEnvironmentVariable("QTEST11"));
            Assert.Equal("This isn't \"working\" #amiright?", Environment.GetEnvironmentVariable("QTEST12"));
        }

        [Fact]
        public void ExportMultilineTest()
        {
            DotNetEnv.Env.Load("./.env6");
            Assert.Equal("some value", Environment.GetEnvironmentVariable("BASHVAR"));
            Assert.Equal("other value", Environment.GetEnvironmentVariable("exportOTHERVAR"));
            Assert.Equal("env var", Environment.GetEnvironmentVariable("EXPORT ENVVAR1"));
            Assert.Equal(@"
HEADER
---
BODY
---
FOOTER
", Environment.GetEnvironmentVariable("MULTILINE"));
            Assert.Equal("works", Environment.GetEnvironmentVariable("LEADEXPORT"));
            Assert.Equal(@"
RSA HEADER
---
base64
base64
base64
---
", Environment.GetEnvironmentVariable("PRIVATE_KEY"));
        }

        [Fact]
        public void OtherTest()
        {
            DotNetEnv.Env.Load("./.env7");
            // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
            // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
            // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
            // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
            // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
        }
    }
}
