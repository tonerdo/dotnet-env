using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace DotNetEnv.Tests
{
    public class EnvTests
    {
        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private enum QuoteType
        {
            None,
            Single,
            Double,
        }

        // private enum SpacePosition
        // {
        //     None,
        //     Both,
        //     Before,
        //     After,
        // }
/*
        private enum CommentPosition
        {
            None,
            WithSpace,
            Adjacent,
        }

        private static void TestSingleBasic(
            bool spaceInKey = false,
            bool spaceAroundKey = false,
            bool spaceInValue = false,
            bool spaceAroundValue = false,
            QuoteType keyQuotes = QuoteType.None,
            QuoteType valueQuotes = QuoteType.None,
//            SpacePosition equalsSpacePosition = SpacePosition.None,
            CommentPosition commentPosition = CommentPosition.None
        )
        {
            var env = new StringBuilder();
            var key = new StringBuilder("KEY");
            var val = new StringBuilder("VALUE");

            if (spaceInKey)
            {
                key.Insert(2, " ");
            }
            if (spaceInValue)
            {
                value.Insert(3, " ");
            }

            if (spaceAroundKey)
            {
                key.Insert(0, " ");
                key.Append(" ");
            }
            if (spaceAroundValue)
            {
                value.Insert(0, " ");
                value.Append(" ");
            }

            var keyStr = key.ToString();
            var valStr = val.ToString();

            if (keyQuotes == QuoteType.Single)
            {
                key.Insert(0, "'");
                key.Append("'");
            }
            else if (keyQuotes == QuoteType.Double)
            {
                key.Insert(0, "\"");
                key.Append("\"");
            }

            if (valueQuotes == QuoteType.Single)
            {
                value.Insert(0, "'");
                value.Append("'");
            }
            else if (valueQuotes == QuoteType.Double)
            {
                value.Insert(0, "\"");
                value.Append("\"");
            }

            // if (equalsSpacePosition == SpacePosition.After)
            // {
            //     env.Append($"{key}= {value}");
            // }
            // else if (equalsSpacePosition == SpacePosition.Before)
            // {
            //     env.Append($"{key} ={value}");
            // }
            // else if (equalsSpacePosition == SpacePosition.Both)
            // {
            //     env.Append($"{key} = {value}");
            // }

            if (commentPosition == CommentPosition.AfterSpace)
            {
                env.Append("  # comment");
            }
            else if (commentPosition == CommentPosition.Adjacent)
            {
                env.Append("# comment");
            }

            DotNetEnv.Env.Load(new [] { env.ToString() });
            Assert.Equal(valStr, Environment.GetEnvironmentVariable(keyStr));
            Environment.SetEnvironmentVariable(keyStr, null);  // clean up for next test
        }

//         [Fact]
//         public void LoadTest()
//         {
//             DotNetEnv.Env.Load();
//             Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
//             // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
//             //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
//             Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
//             Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
//             Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
//             Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
//             Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
//             Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
//         }

//         [Fact]
//         public void LoadPathTest()
//         {
//             DotNetEnv.Env.Load("./.env2");
//             Assert.Equal("127.0.0.1", Environment.GetEnvironmentVariable("IP"));
//             Assert.Equal("8080", Environment.GetEnvironmentVariable("PORT"));
//             Assert.Equal("example.com", Environment.GetEnvironmentVariable("DOMAIN"));
//             Assert.Equal("some text export other text", Environment.GetEnvironmentVariable("EMBEDEXPORT"));
//             Assert.Null(Environment.GetEnvironmentVariable("COMMENTLEAD"));
//             Assert.Equal("leading white space followed by comment", Environment.GetEnvironmentVariable("WHITELEAD"));
//             Assert.Equal("® 🚀 日本", Environment.GetEnvironmentVariable("UNICODE"));
//         }

//         [Fact]
//         public void LoadStreamTest()
//         {
//             DotNetEnv.Env.Load(File.OpenRead("./.env"));
//             Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
//             // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
//             //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
//             Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
//             Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
//             Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
//             Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
//             Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
//             Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
//         }

//         [Fact]
//         public void LoadLinesTest()
//         {
//             DotNetEnv.Env.Load(File.ReadAllLines("./.env"));
//             Assert.Equal("Toni", Environment.GetEnvironmentVariable("NAME"));
//             // unfortunately .NET removes empty env vars -- there can NEVER be an empty string env var value
//             //  https://msdn.microsoft.com/en-us/library/z46c489x(v=vs.110).aspx#Remarks
//             Assert.Null(Environment.GetEnvironmentVariable("EMPTY"));
//             Assert.Equal("'", Environment.GetEnvironmentVariable("QUOTE"));
//             Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
//             Assert.Equal("user=test;password=secret", Environment.GetEnvironmentVariable("CONNECTION"));
//             Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
//             Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
//         }

//         [Fact]
//         public void LoadArgsTest()
//         {
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true));
//             Assert.Equal("leading and trailing white space", Environment.GetEnvironmentVariable("WHITEBOTH"));
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false));
//             Assert.Equal("  leading and trailing white space   ", Environment.GetEnvironmentVariable("  WHITEBOTH  "));
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true));
//             Assert.Equal("Google", Environment.GetEnvironmentVariable("PASSWORD"));
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, false));
//             Assert.Equal("Google#Facebook", Environment.GetEnvironmentVariable("PASSWORD"));
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true, true));
//             Assert.Equal("SPECIAL STUFF---\nLONG-BASE64\\ignore\"slash", Environment.GetEnvironmentVariable("SSL_CERT"));
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(true, true, false));
//             Assert.Equal("\"SPECIAL STUFF---\\nLONG-BASE64\\ignore\"slash\"", Environment.GetEnvironmentVariable("SSL_CERT"));
//         }

//         [Fact]
//         public void LoadPathArgsTest()
//         {
//             DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(true, true));
//             Assert.Equal("leading white space followed by comment", Environment.GetEnvironmentVariable("WHITELEAD"));
//             DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, true));
//             Assert.Equal("  leading white space followed by comment  ", Environment.GetEnvironmentVariable("WHITELEAD"));
//             DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(true, false));
//             Assert.Equal("leading white space followed by comment  # comment", Environment.GetEnvironmentVariable("WHITELEAD"));
//             DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false));
//             Assert.Equal("  leading white space followed by comment  # comment", Environment.GetEnvironmentVariable("WHITELEAD"));
//             DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false, true));
//             Assert.Equal("® 🚀 日本", Environment.GetEnvironmentVariable("UNICODE"));
//             DotNetEnv.Env.Load("./.env2", new DotNetEnv.Env.LoadOptions(false, false, false));
//             Assert.Equal("'\\u00ae \\U0001F680 日本'", Environment.GetEnvironmentVariable("UNICODE"));
//         }

//         [Fact]
//         public void LoadNoClobberTest()
//         {
//             var expected = "totally the original value";
//             Environment.SetEnvironmentVariable("URL", expected);
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false, false, false, false));
//             Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));

//             Environment.SetEnvironmentVariable("URL", "i'm going to be overwritten");
//             DotNetEnv.Env.Load(new DotNetEnv.Env.LoadOptions(false, false, false, true));
//             Assert.Equal("https://github.com/tonerdo", Environment.GetEnvironmentVariable("URL"));
//         }

//         [Fact]
//         public void LoadNoRequireEnvTest()
//         {
//             var expected = "totally the original value";
//             Environment.SetEnvironmentVariable("URL", expected);
//             // this env file Does Not Exist
//             DotNetEnv.Env.Load("./.env_DNE", new DotNetEnv.Env.LoadOptions());
//             Assert.Equal(expected, Environment.GetEnvironmentVariable("URL"));
//             // it didn't throw an exception and crash for a missing file
//         }

//         [Fact]
//         public void LoadOsCasingTest()
//         {
//             Environment.SetEnvironmentVariable("CASING", "neither");
//             DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
//             Assert.Equal(IsWindows ? "neither" : "lower", Environment.GetEnvironmentVariable("casing"));
//             Assert.Equal("neither", Environment.GetEnvironmentVariable("CASING"));

//             DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
//             Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
//             Assert.Equal(IsWindows ? "lower" : "neither", Environment.GetEnvironmentVariable("CASING"));

//             Environment.SetEnvironmentVariable("CASING", null);
//             Environment.SetEnvironmentVariable("casing", "neither");
//             DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: false));
//             Assert.Equal("neither", Environment.GetEnvironmentVariable("casing"));
//             Assert.Equal(IsWindows ? "neither" : null, Environment.GetEnvironmentVariable("CASING"));

//             DotNetEnv.Env.Load("./.env_casing", new DotNetEnv.Env.LoadOptions(clobberExistingVars: true));
//             Assert.Equal("lower", Environment.GetEnvironmentVariable("casing"));
//             Assert.Equal(IsWindows ? "lower" : null, Environment.GetEnvironmentVariable("CASING"));
//         }

//         [Fact]
//         public void ParseVariablesTest()
//         {
//             DotNetEnv.Env.Load("./.env_embedded");
//             Assert.Equal("test", Environment.GetEnvironmentVariable("TEST"));
//             Assert.Equal("test1", Environment.GetEnvironmentVariable("TEST1"));
//             Assert.Equal("test", Environment.GetEnvironmentVariable("TEST2"));
//             Assert.Equal("testtest", Environment.GetEnvironmentVariable("TEST3"));
//             Assert.Equal("testtest1", Environment.GetEnvironmentVariable("TEST4"));
//             Assert.Equal("test:testtest1 $$ and test1", Environment.GetEnvironmentVariable("TEST5"));

//             DotNetEnv.Env.Load("./.env_embedded", new DotNetEnv.Env.LoadOptions(parseVariables: false));
//             Assert.Equal("test", Environment.GetEnvironmentVariable("TEST"));
//             Assert.Equal("test1", Environment.GetEnvironmentVariable("TEST1"));
//             Assert.Equal("$TEST", Environment.GetEnvironmentVariable("TEST2"));
//             Assert.Equal("$TEST$TEST2", Environment.GetEnvironmentVariable("TEST3"));
//             Assert.Equal("$TEST$TEST4$TEST1", Environment.GetEnvironmentVariable("TEST4"));
//             Assert.Equal("$TEST:$TEST4 $$ and $TEST1", Environment.GetEnvironmentVariable("TEST5"));
//         }

//         [Fact]
//         public void QuotedHashTest()
//         {
//             DotNetEnv.Env.Load("./.env_quoted_hash");
//             Assert.Equal("1234", Environment.GetEnvironmentVariable("QTEST0"));
//             Assert.Equal("1234#this is not a comment", Environment.GetEnvironmentVariable("QTEST1"));
//             Assert.Equal("This is a test", Environment.GetEnvironmentVariable("QTEST2"));
//             Assert.Equal("1234 #not_comment", Environment.GetEnvironmentVariable("QTEST3"));
//             Assert.Equal("9876#this is not a comment", Environment.GetEnvironmentVariable("QTEST4"));
//             Assert.Equal("This is a another test", Environment.GetEnvironmentVariable("QTEST5"));
//             Assert.Equal("9876 #not_comment", Environment.GetEnvironmentVariable("QTEST6"));
//             Assert.Equal("This isn't working", Environment.GetEnvironmentVariable("QTEST7"));
//             Assert.Equal("Hi \"Bob\"!", Environment.GetEnvironmentVariable("QTEST8"));
//             Assert.Equal("Hi \"Bob\"!", Environment.GetEnvironmentVariable("QTEST9"));
//             Assert.Equal("This isn't working", Environment.GetEnvironmentVariable("QTEST10"));
//             Assert.Equal("This isn't \"working\" #amiright?", Environment.GetEnvironmentVariable("QTEST11"));
//             Assert.Equal("This isn't \"working\" #amiright?", Environment.GetEnvironmentVariable("QTEST12"));
//         }

//         [Fact]
//         public void ExportMultilineTest()
//         {
//             DotNetEnv.Env.Load("./.env_export_multiline");
//             Assert.Equal("some value", Environment.GetEnvironmentVariable("BASHVAR"));
//             Assert.Equal("other value", Environment.GetEnvironmentVariable("exportOTHERVAR"));
//             Assert.Equal("env var", Environment.GetEnvironmentVariable("EXPORT ENVVAR1"));
//             Assert.Equal(@"
// HEADER
// ---
// BODY
// ---
// FOOTER
// ", Environment.GetEnvironmentVariable("MULTILINE"));
//             Assert.Equal("works", Environment.GetEnvironmentVariable("LEADEXPORT"));
//             Assert.Equal(@"
// RSA HEADER
// ---
// base64
// base64
// base64
// ---
// ", Environment.GetEnvironmentVariable("PRIVATE_KEY"));

//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE1"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE1B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE2"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE2B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE3"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE3B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE4"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE4B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE5"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE5B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE6"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE6B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE7"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE7B"));
//             Assert.Equal("irrelevant", Environment.GetEnvironmentVariable("IGNORE8"));
//             Assert.Null(Environment.GetEnvironmentVariable("IGNORE8B"));
//         }

//         [Fact]
//         public void OtherTest()
//         {
//             DotNetEnv.Env.Load("./.env_other");
//             // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
//             // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
//             // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
//             // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
//             // Assert.Equal("xxx", Environment.GetEnvironmentVariable("TEST1"));
//         }

//         [Fact]
//         public void BadSyntaxTest()
//         {
//             Exception ex;
//             ex = Assert.Throws<Exception>(
//                 () => DotNetEnv.Env.Load(new [] {
//                     "NOVALUE",
//                 })
//             );
//             // Assert.Equal("xxx", ex.Message);
//             ex = Assert.Throws<Exception>(
//                 () => DotNetEnv.Env.Load(new [] {
//                     "MULTI WORD KEY",
//                 })
//             );
//             ex = Assert.Throws<Exception>(
//                 () => DotNetEnv.Env.Load(new [] {
//                     "UNMATCHEDQUOTE='",
//                 })
//             );
//             ex = Assert.Throws<Exception>(
//                 () => DotNetEnv.Env.Load(new [] {
//                     "UNMATCHEDQUOTE=\"",
//                 })
//             );
//         }

        [Fact]
        public void BasicsTest()
        {
            DotNetEnv.Env.Load(new [] { "KEY=VALUE" });
            Assert.Equal("VALUE", Environment.GetEnvironmentVariable("KEY"));

            DotNetEnv.Env.Load(new [] { "KEY=VAL UE" });
            Assert.Equal("VAL UE", Environment.GetEnvironmentVariable("KEY"));

            DotNetEnv.Env.Load(new [] { "K1=V1", "K2=V2" });
            Assert.Equal("V1", Environment.GetEnvironmentVariable("K1"));
            Assert.Equal("V2", Environment.GetEnvironmentVariable("K2"));

            ex = Assert.Throws<Exception>(
                () => DotNetEnv.Env.Load(new [] { "KEY=\"" })
            );
            Assert.Equal("Unmatched quote", ex.Message);
        }

        [Fact]
        public void LongBasicsTest()
        {
            var quoteTypes = Enum.GetValues(typeof(QuoteType)).Cast<QuoteType>();
//            var spacePositions = Enum.GetValues(typeof(SpacePosition)).Cast<SpacePosition>();
            var commentPositions = Enum.GetValues(typeof(CommentPosition)).Cast<CommentPosition>();

            for (int keySpaces = 0; keySpaces < 4; spaces++)
            for (int valSpaces = 0; valSpaces < 4; spaces++)
            foreach (var keyQuoteType in quoteTypes)
            foreach (var valQuoteType in quoteTypes)
//            foreach (var spacePosition in spacePositions)
            foreach (var commentPosition in commentPositions)
            {
                TestSingleBasic(
                    spaceInKey: keySpaces % 2 == 0,
                    spaceAroundKey: keySpaces > 1,
                    spaceInValue: valSpaces % 2 == 0,
                    spaceAroundValue: valSpaces > 1,
                    keyQuotes: keyQuoteType,
                    valueQuotes: valQuoteType,
//                    equalsSpacePosition: spacePosition,
                    commentPosition: commentPosition
                );
            }
        }
        */
    }
}
