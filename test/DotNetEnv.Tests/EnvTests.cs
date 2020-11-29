using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using Sprache;

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
            Assert.Equal("  leading and trailing white space   ", Environment.GetEnvironmentVariable("WHITEBOTH"));
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
            Assert.Equal("  leading white space followed by comment", Environment.GetEnvironmentVariable("WHITELEAD"));
            Assert.Equal("® 🚀 日本", Environment.GetEnvironmentVariable("UNICODE"));
        }

        [Fact]
        public void LoadFileTest()
        {
            DotNetEnv.Env.Load("./.env");
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
            // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
            //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
            Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
            Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
            Assert.Equal("  leading and trailing white space   ", Environment.GetEnvironmentVariable("WHITEBOTH"));
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
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
            Assert.Equal("  leading and trailing white space   ", Environment.GetEnvironmentVariable("WHITEBOTH"));
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
            Assert.Equal("  leading and trailing white space   ", Environment.GetEnvironmentVariable("WHITEBOTH"));
            Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
        }

        [Fact]
        public void LoadNoClobberTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));

            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadNoSetEnvVarsTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(setEnvVars: false));
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            // this env var remaining null is the difference between NoSetEnvVars and NoClobber
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));

            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(setEnvVars: true));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadNoRequireEnvTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("URL", expected);
            // this env file Does Not Exist
            DotNetEnv.Env.Load("./.env_DNE", new DotNetEnv.Env.LoadOptions());
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            // it didn't throw an exception and crash for a missing file
        }

        [Fact]
        public void LoadOsCasingTest()
        {
            Environment.SetEnvironmentVariable("CASING", "neither");
            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal(IsWindows ? "neither" : "lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal("neither", Environment.GetEnvironmentVariable("CASING"));

            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "lower" : "neither", Environment.GetEnvironmentVariable("CASING"));

            Environment.SetEnvironmentVariable("CASING", null);
            Environment.SetEnvironmentVariable("casing", "neither");
            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
            Assert.Equal("neither", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "neither" : null, Environment.GetEnvironmentVariable("CASING"));

            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
            Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "lower" : null, Environment.GetEnvironmentVariable("CASING"));
        }

        [Fact]
        public void ParseInterpolatedTest()
        {
            System.Environment.SetEnvironmentVariable("EXISTING_ENVIRONMENT_VARIABLE", "value");
            System.Environment.SetEnvironmentVariable("DNE_VAR", null);
            DotNetEnv.Env.Load("./.env_embedded");
            Assert.Equal("test", Environment.GetEnvironmentVariable("TEST"));
            Assert.Equal("test1", Environment.GetEnvironmentVariable("TEST1"));
            Assert.Equal("test", Environment.GetEnvironmentVariable("TEST2"));
            Assert.Equal("testtest", Environment.GetEnvironmentVariable("TEST3"));
            Assert.Equal("testtest1", Environment.GetEnvironmentVariable("TEST4"));
            Assert.Equal("test:testtest1 $$ and test1", Environment.GetEnvironmentVariable("TEST5"));

            Assert.Equal("value1", System.Environment.GetEnvironmentVariable("FIRST_KEY"));
            Assert.Equal("value2andvalue1", System.Environment.GetEnvironmentVariable("SECOND_KEY"));
            // EXISTING_ENVIRONMENT_VARIABLE already set to "value"
            Assert.Equal("value;andvalue3", System.Environment.GetEnvironmentVariable("THIRD_KEY"));
            // DNE_VAR does not exist (has no value)
            Assert.Equal(";nope", System.Environment.GetEnvironmentVariable("FOURTH_KEY"));
        }

        [Fact]
        public void QuotedHashTest()
        {
            DotNetEnv.Env.Load("./.env_quoted_hash");
            Assert.Equal("1234#not_comment", Environment.GetEnvironmentVariable("QTEST0"));
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
            DotNetEnv.Env.Load("./.env_export_multiline");
            Assert.Equal("some value", Environment.GetEnvironmentVariable("BASHVAR"));
            Assert.Equal("other value", Environment.GetEnvironmentVariable("exportOTHERVAR"));
            Assert.Equal("env var 1", Environment.GetEnvironmentVariable("ENVVAR1"));
            Assert.Equal("env var 2", Environment.GetEnvironmentVariable("ENVVAR2"));
            Assert.Equal("env var 3", Environment.GetEnvironmentVariable("ENVVAR3"));
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
            var kvps = DotNetEnv.Env.Load("./.env_other").ToArray();
            Assert.Equal(2, kvps.Length);
            var dict = DotNetEnv.Env.ToDictionary(kvps);

            // note that env vars get only the final assignment, but all are returned
            Assert.Equal("dupe2", Environment.GetEnvironmentVariable("DUPLICATE"));
            Assert.Equal("dupe2", dict["DUPLICATE"]);
            Assert.Equal("DUPLICATE", kvps[0].Key);
            Assert.Equal("DUPLICATE", kvps[1].Key);
            Assert.Equal("dupe1", kvps[0].Value);
            Assert.Equal("dupe2", kvps[1].Value);
        }

        [Fact]
        public void BadSyntaxTest()
        {
            ParseException ex;

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "KEY=VAL UE",
                })
            );
            Assert.Equal("Parsing failure: unexpected 'U'; expected LineTerminator (Line 1, Column 9); recently consumed: KEY=VAL ", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "NOVALUE",
                })
            );
            Assert.Equal("Parsing failure: Unexpected end of input reached; expected = (Line 1, Column 8); recently consumed: NOVALUE", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "MULTI WORD KEY",
                })
            );
            Assert.Equal("Parsing failure: unexpected ' '; expected = (Line 1, Column 6); recently consumed: MULTI", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "UNMATCHEDQUOTE='",
                })
            );
            Assert.Equal("Parsing failure: unexpected '''; expected LineTerminator (Line 1, Column 16); recently consumed: CHEDQUOTE=", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "UNMATCHEDQUOTE=\"",
                })
            );
            Assert.Equal("Parsing failure: unexpected '\"'; expected LineTerminator (Line 1, Column 16); recently consumed: CHEDQUOTE=", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "SSL_CERT=\"SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash\"",
                })
            );
            Assert.Equal("Parsing failure: unexpected 's'; expected LineTerminator (Line 2, Column 20); recently consumed: 64\\ignore\"", ex.Message);

            // this test confirms that the entire file must be valid, not just at least one assignment at the start
            // otherwise it silently discards any remainder after the first failure, so long as at least one success...
            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.Load(new [] {
                    "OK=GOOD",
                    "SSL_CERT=\"SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash\"",
                })
            );
            Assert.Equal("Parsing failure: unexpected 'S'; expected end of input (Line 2, Column 1); recently consumed: OK=GOOD\n", ex.Message);
        }

        [Fact]
        public void BasicsTest()
        {
            DotNetEnv.Env.Load(new [] { "ENV_TEST_KEY=VALUE" });
            Assert.Equal("VALUE", Environment.GetEnvironmentVariable("ENV_TEST_KEY"));

            DotNetEnv.Env.Load(new [] { "ENV_TEST_K1=V1", "ENV_TEST_K2=V2" });
            Assert.Equal("V1", Environment.GetEnvironmentVariable("ENV_TEST_K1"));
            Assert.Equal("V2", Environment.GetEnvironmentVariable("ENV_TEST_K2"));
        }
    }
}
