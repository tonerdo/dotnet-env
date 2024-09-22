using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using DotNetEnv.Extensions;
using Xunit;
using Superpower;

namespace DotNetEnv.Tests
{
    public class EnvTests
    {
        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private static string[] OldEnvVars = new string[]
        {
            "NAME",
            "EMPTY",
            "QUOTE",
            "URL",
            "CONNECTION",
            "WHITEBOTH",
            "SSL_CERT",
            "IP",
            "PORT",
            "DOMAIN",
            "EMBEDEXPORT",
            "COMMENTLEAD",
            "WHITELEAD",
            "UNICODE",
            "CASING",

            "TEST",
            "TEST1",
            "TEST2",
            "TEST3",
            "TEST4",
            "TEST5_DOUBLE",
            "TEST5_SINGLE",
            "TEST5_UNQUOTED",
            "TEST_UNQUOTED_WITH_INTERPOLATED_SURROUNDING_SPACES",
            "FIRST_KEY",
            "SECOND_KEY",
            "THIRD_KEY",
            "FOURTH_KEY",
            "GROUP_FILTER_REGEX",
            "DOLLAR1_U",
            "DOLLAR2_U",
            "DOLLAR3_U",
            "DOLLAR4_U",
            "DOLLAR1_S",
            "DOLLAR2_S",
            "DOLLAR3_S",
            "DOLLAR4_S",
            "DOLLAR1_D",
            "DOLLAR2_D",
            "DOLLAR3_D",
            "DOLLAR4_D",
        };

        public EnvTests() {
            // Clear all env vars set from normal interpolated test
            for (var i = 0; i < OldEnvVars.Length; i++) {
                Environment.SetEnvironmentVariable(OldEnvVars[i], null);
            }
        }

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
        public void LoadDotenvHigherSkip()
        {
            Environment.SetEnvironmentVariable("TEST", null);
            Environment.SetEnvironmentVariable("NAME", null);
            // ./DotNetEnv.Tests/bin/Debug/netcoreapp3.1/DotNetEnv.Tests.dll -- get to the ./
            DotNetEnv.Env.Load("../../../../");
            Assert.Equal("here", Environment.GetEnvironmentVariable("TEST"));
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));
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
        public void LoadMultiTest()
        {
            DotNetEnv.Env.LoadMulti(new[] { "./.env", "./.env2" });
            Assert.Equal("Other", Environment.GetEnvironmentVariable("NAME"));
            Environment.SetEnvironmentVariable("NAME", null);
            DotNetEnv.Env.NoClobber().LoadMulti(new[] { "./.env", "./.env2" });
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
            Environment.SetEnvironmentVariable("NAME", "Person");
            DotNetEnv.Env.NoClobber().LoadMulti(new[] { "./.env", "./.env2" });
            Assert.Equal("Person", Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadMultiTestNoEnvVars()
        {
            var pairs = DotNetEnv.Env.NoEnvVars().LoadMulti(new[] { "./.env", "./.env2" });
            Assert.Equal("Other", pairs.LastOrDefault(x => x.Key == "NAME").Value);
            Environment.SetEnvironmentVariable("NAME", null);
            pairs = DotNetEnv.Env.NoEnvVars().NoClobber().LoadMulti(new[] { "./.env", "./.env2" });
            Assert.Equal("Toni", pairs.FirstOrDefault(x => x.Key == "NAME").Value);
            Environment.SetEnvironmentVariable("NAME", "Person");
            pairs = DotNetEnv.Env.NoEnvVars().NoClobber().LoadMulti(new[] { "./.env", "./.env2" });
            Assert.Equal("Person", pairs.FirstOrDefault(x => x.Key == "NAME").Value);
        }

        [Fact]
        public void LoadNoClobberTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions(clobberExistingVars: false));
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));

            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions(clobberExistingVars: true));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadNoSetEnvVarsTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", expected);
            DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions(setEnvVars: false));
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            // this env var remaining null is the difference between NoSetEnvVars and NoClobber
            Assert.Null(Environment.GetEnvironmentVariable("NAME"));

            Environment.SetEnvironmentVariable("NAME", null);
            Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
            DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions(setEnvVars: true));
            Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
            Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
        }

        [Fact]
        public void LoadNoRequireEnvTest()
        {
            var expected = "totally the original value";
            Environment.SetEnvironmentVariable("URL", expected);
            // this env file Does Not Exist
            DotNetEnv.Env.Load("./.env_DNE");
            Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
            // it didn't throw an exception and crash for a missing file
        }

        [Fact]
        public void LoadOsCasingTest()
        {
            Environment.SetEnvironmentVariable("CASING", "neither");
            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.LoadOptions(clobberExistingVars: false));
            Assert.Equal(IsWindows ? "neither" : "lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal("neither", Environment.GetEnvironmentVariable("CASING"));

            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.LoadOptions(clobberExistingVars: true));
            Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "lower" : "neither", Environment.GetEnvironmentVariable("CASING"));

            Environment.SetEnvironmentVariable("CASING", null);
            Environment.SetEnvironmentVariable("casing", "neither");
            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.LoadOptions(clobberExistingVars: false));
            Assert.Equal("neither", Environment.GetEnvironmentVariable("casing"));
            Assert.Equal(IsWindows ? "neither" : null, Environment.GetEnvironmentVariable("CASING"));

            DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.LoadOptions(clobberExistingVars: true));
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

            Assert.Equal("test:testtest1 $$ '\" ® and test1", Environment.GetEnvironmentVariable("TEST5_DOUBLE"));
            Assert.Equal("$TEST:$TEST4 \\$\\$ \" \\uae and $TEST1", Environment.GetEnvironmentVariable("TEST5_SINGLE"));
            Assert.Equal("test:testtest1\\uaeandtest1", Environment.GetEnvironmentVariable("TEST5_UNQUOTED"));

            // note that interpolated values will keep whitespace! (as they should, esp if surrounding them with other values)
            Assert.Equal(" surrounded by spaces ", Environment.GetEnvironmentVariable("TEST_UNQUOTED_WITH_INTERPOLATED_SURROUNDING_SPACES"));

            Assert.Equal("value1", System.Environment.GetEnvironmentVariable("FIRST_KEY"));
            Assert.Equal("value2andvalue1", System.Environment.GetEnvironmentVariable("SECOND_KEY"));
            // EXISTING_ENVIRONMENT_VARIABLE already set to "value"
            Assert.Equal("value;andvalue3", System.Environment.GetEnvironmentVariable("THIRD_KEY"));
            // DNE_VAR does not exist (has no value)
            Assert.Equal(";nope", System.Environment.GetEnvironmentVariable("FOURTH_KEY"));

            Assert.Equal("^((?!Everyone).)*$", Environment.GetEnvironmentVariable("GROUP_FILTER_REGEX"));

            Assert.Equal("value$", Environment.GetEnvironmentVariable("DOLLAR1_U"));
            Assert.Equal("valuevalue$$", Environment.GetEnvironmentVariable("DOLLAR2_U"));
            Assert.Equal("value$.$", Environment.GetEnvironmentVariable("DOLLAR3_U"));
            Assert.Equal("value$$", Environment.GetEnvironmentVariable("DOLLAR4_U"));

            Assert.Equal("value$", Environment.GetEnvironmentVariable("DOLLAR1_S"));
            Assert.Equal("value$DOLLAR1_S$", Environment.GetEnvironmentVariable("DOLLAR2_S"));
            Assert.Equal("value$.$", Environment.GetEnvironmentVariable("DOLLAR3_S"));
            Assert.Equal("value$$", Environment.GetEnvironmentVariable("DOLLAR4_S"));

            Assert.Equal("value$", Environment.GetEnvironmentVariable("DOLLAR1_D"));
            Assert.Equal("valuevalue$$", Environment.GetEnvironmentVariable("DOLLAR2_D"));
            Assert.Equal("value$.$", Environment.GetEnvironmentVariable("DOLLAR3_D"));
            Assert.Equal("value$$", Environment.GetEnvironmentVariable("DOLLAR4_D"));
        }

        [Fact]
        public void ParseInterpolatedNoEnvVarsTest()
        {
            System.Environment.SetEnvironmentVariable("EXISTING_ENVIRONMENT_VARIABLE", "value");
            System.Environment.SetEnvironmentVariable("DNE_VAR", null);
            var environmentDictionary = DotNetEnv.Env.NoEnvVars().Load("./.env_embedded").ToDotEnvDictionary();

            Assert.Equal("test", environmentDictionary["TEST"]);
            Assert.Equal("test1", environmentDictionary["TEST1"]);
            Assert.Equal("test", environmentDictionary["TEST2"]);
            Assert.Equal("testtest", environmentDictionary["TEST3"]);
            Assert.Equal("testtest1", environmentDictionary["TEST4"]);

            Assert.Equal("test:testtest1 $$ '\" ® and test1", environmentDictionary["TEST5_DOUBLE"]);
            Assert.Equal("$TEST:$TEST4 \\$\\$ \" \\uae and $TEST1", environmentDictionary["TEST5_SINGLE"]);
            Assert.Equal("test:testtest1\\uaeandtest1", environmentDictionary["TEST5_UNQUOTED"]);

            // note that interpolated values will keep whitespace! (as they should, esp if surrounding them with other values)
            Assert.Equal(" surrounded by spaces ", environmentDictionary["TEST_UNQUOTED_WITH_INTERPOLATED_SURROUNDING_SPACES"]);

            Assert.Equal("value1", environmentDictionary["FIRST_KEY"]);
            Assert.Equal("value2andvalue1", environmentDictionary["SECOND_KEY"]);
            // EXISTING_ENVIRONMENT_VARIABLE already set to "value"
            Assert.Equal("value;andvalue3", environmentDictionary["THIRD_KEY"]);
            // DNE_VAR does not exist (has no value)
            Assert.Equal(";nope", environmentDictionary["FOURTH_KEY"]);

            Assert.Equal("^((?!Everyone).)*$", environmentDictionary["GROUP_FILTER_REGEX"]);

            Assert.Equal("value$", environmentDictionary["DOLLAR1_U"]);
            Assert.Equal("valuevalue$$", environmentDictionary["DOLLAR2_U"]);
            Assert.Equal("value$.$", environmentDictionary["DOLLAR3_U"]);
            Assert.Equal("value$$", environmentDictionary["DOLLAR4_U"]);

            Assert.Equal("value$", environmentDictionary["DOLLAR1_S"]);
            Assert.Equal("value$DOLLAR1_S$", environmentDictionary["DOLLAR2_S"]);
            Assert.Equal("value$.$", environmentDictionary["DOLLAR3_S"]);
            Assert.Equal("value$$", environmentDictionary["DOLLAR4_S"]);

            Assert.Equal("value$", environmentDictionary["DOLLAR1_D"]);
            Assert.Equal("valuevalue$$", environmentDictionary["DOLLAR2_D"]);
            Assert.Equal("value$.$", environmentDictionary["DOLLAR3_D"]);
            Assert.Equal("value$$", environmentDictionary["DOLLAR4_D"]);

            // Validate that the env vars are still not set
            for (var i = 0; i < OldEnvVars.Length; i++) {
                Assert.Null(Environment.GetEnvironmentVariable(OldEnvVars[i]));
            }
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
            Assert.Equal("This isnt working", Environment.GetEnvironmentVariable("QTEST10"));
            Assert.Equal("This isn't \"working\" #amiright?", Environment.GetEnvironmentVariable("QTEST11"));
            Assert.Equal("This isnt \"working\" #amiright?", Environment.GetEnvironmentVariable("QTEST12"));
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
        public void ExamplesTest()
        {
            Environment.SetEnvironmentVariable("ENVVAR", "already here");
            DotNetEnv.Env.Load("./.env_examples");
            Assert.Equal("extra already here value", Environment.GetEnvironmentVariable("KEY_DOUBLE"));
            Assert.Equal("extraalready herevalue", Environment.GetEnvironmentVariable("KEY_UNQUOTED"));
            Assert.Equal("value#notcomment", Environment.GetEnvironmentVariable("KEY_UNQUOTED_HASH"));
            Assert.Equal("value\nand more", Environment.GetEnvironmentVariable("KEY_MULTILINE"));
            Assert.Equal("#not_comment\nline2", Environment.GetEnvironmentVariable("OTHER_MULTILINE"));

            Assert.Equal("value", Environment.GetEnvironmentVariable("WHITE_PRE"));
            Assert.Equal("value", Environment.GetEnvironmentVariable("WHITE_POST"));
            Assert.Equal("value", Environment.GetEnvironmentVariable("WHITE_BOTH"));
            Assert.Equal(" value ", Environment.GetEnvironmentVariable("WHITE_QUOTED"));

            Assert.Equal("Initial Catalog=dbname", Environment.GetEnvironmentVariable("ConnectionString1"));
            Assert.Equal("Initial Catalog=dbname", Environment.GetEnvironmentVariable("ConnectionString2"));
            Assert.Equal("value#notcomment more	words here", Environment.GetEnvironmentVariable("KEY_UNQUOTED_HASH_MULTI"));
        }

        [Fact]
        public void OtherTest()
        {
            Environment.SetEnvironmentVariable("VAR1", "_var1_");
            Environment.SetEnvironmentVariable("VAR1test", "_var1TEST_");
            Environment.SetEnvironmentVariable("VAR2", "_var2_");
            Environment.SetEnvironmentVariable("VAR2test", "_var2TEST_");
            Environment.SetEnvironmentVariable("VAR3", "_var3_");
            Environment.SetEnvironmentVariable("NVAR1", "_nvar1_");
            Environment.SetEnvironmentVariable("NVAR2", "_nvar2_");

            var kvps = DotNetEnv.Env.Load("./.env_other").ToArray();
            Assert.Equal(35, kvps.Length);
            var dict = kvps.ToDotEnvDictionary();

            // note that env vars get only the final assignment, but all are returned
            Assert.Equal("dupe2", Environment.GetEnvironmentVariable("DUPLICATE"));
            Assert.Equal("dupe2", dict["DUPLICATE"]);
            Assert.Equal("DUPLICATE", kvps[0].Key);
            Assert.Equal("DUPLICATE", kvps[1].Key);
            Assert.Equal("dupe1", kvps[0].Value);
            Assert.Equal("dupe2", kvps[1].Value);

            Assert.Equal("bar", Environment.GetEnvironmentVariable("TEST_KEYWORD_1"));
            Assert.Equal("12345", Environment.GetEnvironmentVariable("TEST_KEYWORD_2"));
            Assert.Equal("TRUE", Environment.GetEnvironmentVariable("TEST_KEYWORD_3"));
            Assert.Equal("Hello", Environment.GetEnvironmentVariable("TEST_VARIABLE"));

            Assert.Equal("_var1_ test test_var2TEST_ test", Environment.GetEnvironmentVariable("TEST_INTERPOLATION_VARIABLE"));
            Assert.Equal("test test{_nvar1_}test{_nvar2_}test test", Environment.GetEnvironmentVariable("TEST_INTERPOLATION_SYNTAX_ONE"));
            Assert.Equal("test test_nvar1_test_nvar2_test test", Environment.GetEnvironmentVariable("TEST_INTERPOLATION_SYNTAX_TWO"));
            Assert.Equal("test_var1TEST_ test {VAR2}test test_var3_test", Environment.GetEnvironmentVariable("TEST_INTERPOLATION_SYNTAX_ALL"));

            Assert.Equal("bar", Environment.GetEnvironmentVariable("TEST_UNQUOTED"));
            Assert.Null(Environment.GetEnvironmentVariable("TEST_UNQUOTED_NO_VALUE"));

            Assert.Null(Environment.GetEnvironmentVariable("TEST_WHITE_SPACE"));
            Assert.Equal("Hello", Environment.GetEnvironmentVariable("TEST_WHITE_SPACE_STRING"));
            Assert.Equal("bar", Environment.GetEnvironmentVariable("TEST_WHITE_SPACE_UNQUOTED"));
            Assert.Equal("false", Environment.GetEnvironmentVariable("TEST_WHITE_SPACE_UNQUOTED_BOOL"));
            Assert.Equal("20", Environment.GetEnvironmentVariable("TEST_WHITE_SPACE_UNQUOTED_NUM"));

            Assert.Equal("true", Environment.GetEnvironmentVariable("TEST_TRUE"));
            Assert.Equal("false", Environment.GetEnvironmentVariable("TEST_FALSE"));
            Assert.Equal("null", Environment.GetEnvironmentVariable("TEST_NULL"));
            Assert.Equal("TRUE", Environment.GetEnvironmentVariable("TEST_TRUE_CAPITAL"));
            Assert.Equal("FALSE", Environment.GetEnvironmentVariable("TEST_FALSE_CAPITAL"));
            Assert.Equal("NULL", Environment.GetEnvironmentVariable("TEST_NULL_CAPITAL"));

            Assert.Equal("54", Environment.GetEnvironmentVariable("TEST_NUM_DECIMAL"));
            Assert.Equal("5.3", Environment.GetEnvironmentVariable("TEST_NUM_FLOAT"));
            Assert.Equal("1e10", Environment.GetEnvironmentVariable("TEST_NUM"));
            Assert.Equal("-42", Environment.GetEnvironmentVariable("TEST_NUM_NEGATIVE"));
            Assert.Equal("057", Environment.GetEnvironmentVariable("TEST_NUM_OCTAL"));
            Assert.Equal("0x1A", Environment.GetEnvironmentVariable("TEST_NUM_HEX"));

            Assert.NotEqual("foobar", Environment.GetEnvironmentVariable("TEST_ONE"));
            Assert.Null(Environment.GetEnvironmentVariable("TEST_ONE"));
            Assert.NotEqual("foobar", Environment.GetEnvironmentVariable("TEST_TWO"));
            Assert.Null(Environment.GetEnvironmentVariable("TEST_TWO"));
            Assert.NotEqual("foobar", Environment.GetEnvironmentVariable("TEST_THREE"));
            Assert.Null(Environment.GetEnvironmentVariable("TEST_THREE"));
            Assert.Equal("test test test", Environment.GetEnvironmentVariable("TEST_FOUR"));
            Assert.Equal("comment symbol # inside string", Environment.GetEnvironmentVariable("TEST_FIVE"));
            Assert.Equal("comment symbol # and quotes \" \' inside quotes", Environment.GetEnvironmentVariable("TEST_SIX"));

            Assert.Equal("escaped characters \n \t \r \" \' $ or maybe a backslash \\...", Environment.GetEnvironmentVariable("TEST_ESCAPE"));
            Assert.Equal("Lorem {_var1_} _var2_ _var3_ ipsum dolor sit amet\n\r\t\\", Environment.GetEnvironmentVariable("TEST_DOUBLE"));
            Assert.Equal("Lorem {$VAR1} ${VAR2} $VAR3 ipsum dolor sit amet\\n\\r\\t\\\\", Environment.GetEnvironmentVariable("TEST_SINGLE"));
        }

        [Fact]
        public void BadSyntaxTest()
        {
            ParseException ex;

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("NOVALUE")
            );
            Assert.Equal("Syntax error (line 1, column 1): unexpected `N`.", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("MULTI WORD KEY")
            );
            Assert.Equal("Syntax error (line 1, column 1): unexpected `M`.", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("UNMATCHEDQUOTE='abc")
            );
            Assert.Equal("Syntax error (line 1, column 1): unexpected `U`.", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("BADQUOTE='\\''")
            );
            Assert.Equal("Syntax error (line 1, column 1): unexpected `B`.", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("UNMATCHEDQUOTE=\"abc")
            );
            Assert.Equal("Syntax error (line 1, column 1): unexpected `U`.", ex.Message);

            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("SSL_CERT=\"SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash\"")
            );
            Assert.Equal("Syntax error (line 1, column 1): unexpected `S`.", ex.Message);

            // this test confirms that the entire file must be valid, not just at least one assignment at the start
            // otherwise it silently discards any remainder after the first failure, so long as at least one success...
            ex = Assert.Throws<ParseException>(
                () => DotNetEnv.Env.LoadContents("OK=GOOD\nSSL_CERT=\"SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash\"")
            );
            Assert.Equal("Syntax error (line 2, column 1): unexpected `S`.", ex.Message);
        }

        [Fact]
        public void BasicsTest()
        {
            DotNetEnv.Env.LoadContents("ENV_TEST_KEY=VALUE");
            Assert.Equal("VALUE", Environment.GetEnvironmentVariable("ENV_TEST_KEY"));
            DotNetEnv.Env.LoadContents("ENV_TEST_KEY=VAL UE");
            Assert.Equal("VAL UE", Environment.GetEnvironmentVariable("ENV_TEST_KEY"));

            DotNetEnv.Env.LoadContents("ENV_TEST_K1=V1\nENV_TEST_K2=V2");
            Assert.Equal("V1", Environment.GetEnvironmentVariable("ENV_TEST_K1"));
            Assert.Equal("V2", Environment.GetEnvironmentVariable("ENV_TEST_K2"));
        }
    }
}
