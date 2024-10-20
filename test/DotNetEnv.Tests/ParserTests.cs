using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotNetEnv.Superpower;
using Xunit;
using Superpower;
using Superpower.Parsers;

namespace DotNetEnv.Tests
{
    public class ParserTests : IDisposable
    {
        // C# wow that you can't handle 32 bit unicode as chars. wow. strings for 4 byte chars.
        private static readonly string RocketChar = char.ConvertFromUtf32(0x1F680); // 🚀

        private const string EXCEPT_CHARS = "'\"$";

        private const string EV_TEST = "ENVVAR_TEST";
        private const string EV_DNE = "EV_DNE";
        private const string EV_TEST_1 = "EV_TEST_1";
        private const string EV_TEST_2 = "EV_TEST_2";

        private readonly Dictionary<string,string> oldEnvvars = new();
        private static readonly string[] ALL_EVS = { EV_TEST, EV_DNE, EV_TEST_1, EV_TEST_2 };

        public ParserTests ()
        {
            foreach (var ev in ALL_EVS)
            {
                oldEnvvars[ev] = Environment.GetEnvironmentVariable(ev);
            }

            Environment.SetEnvironmentVariable(EV_TEST, "ENV value");
        }

        public void Dispose ()
        {
            foreach (var ev in ALL_EVS)
            {
                Environment.SetEnvironmentVariable(ev, oldEnvvars[ev]);
            }
        }

        [Theory]
        [InlineData("name")]
        [InlineData("_n")]
        [InlineData("__")]
        [InlineData("_0")]
        [InlineData("a_b")]
        [InlineData("_a_b")]
        [InlineData("a.b")]
        [InlineData("a-b")]
        public void IdentifierShouldParseUntilEnd(string identifier) =>
            Assert.Equal(identifier, Parsers.Identifier.AtEnd().Parse(identifier));

        [Theory]
        [InlineData("\"name")]
        [InlineData("0name")]
        [InlineData(".a.b")]
        [InlineData("-a.b")]
        [InlineData("a!b")]
        [InlineData("a?b")]
        [InlineData("a*b")]
        [InlineData("a:b")]
        public void IdentifierShouldThrowOnParseUntilEnd(string invalidIdentifier) =>
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse(invalidIdentifier));

        [Theory]
        [InlineData(33, @"\41")]
        [InlineData(33, @"\041")]
        [InlineData(90, @"\132")]
        //[InlineData(???, @"\412")] // bash accepts values outside of ASCII range? check printf "\412"
        public void OctalByteShouldParseUntilEnd(byte expected, string input) =>
            Assert.Equal(expected, Parsers.OctalByte.AtEnd().Parse(input));

        [Theory]
        [InlineData("!", @"\41")]
        [InlineData("!", @"\041")]
        [InlineData("Z", @"\132")]
        //[InlineData(???, @"\412")] // as above with ShouldParseOctalByte() for values outside of ASCII range
        // TODO: tests for octal combinations to utf8?
        public void OctalCharShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.OctalChar.AtEnd().Parse(input));

        [Theory]
        [InlineData(90, @"\x5a")]
        public void HexByteShouldParseUntilEnd(byte expected, string input) =>
            Assert.Equal(expected, Parsers.HexByte.AtEnd().Parse(input));

        [Theory]
        [InlineData("Z", @"\x5a")]
        [InlineData("®", @"\xc2\xae")]
        [InlineData("☠", @"\xE2\x98\xA0")]
        [InlineData("日", @"\xe6\x97\xa5")]
        [InlineData("本", @"\xe6\x9c\xac")]
        [InlineData(UnicodeChars.Rocket, @"\xF0\x9F\x9A\x80")]
        public void Utf8CharShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.Utf8Char.AtEnd().Parse(input));

        [Theory]
        [InlineData("®", @"\u00ae")]
        [InlineData("®", @"\uae")]
        public void Utf16CharShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.Utf16Char.AtEnd().Parse(input));

        [Theory]
        [InlineData(UnicodeChars.Rocket, @"\U0001F680")]
        [InlineData(UnicodeChars.Rocket, @"\U1F680")]
        public void Utf32CharShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.Utf32Char.AtEnd().Parse(input));

        [InlineData("\b", "\\b")]
        [InlineData("'", "\\'")]
        [InlineData("\"", "\\\"")]
        public void EscapedCharShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.EscapedChar.AtEnd().Parse(input));

        [Theory]
        [InlineData("\n")]
        public void EscapedCharShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.EscapedChar.AtEnd().Parse(invalidInput));

        [Theory]
        [InlineData("ENV value", "$ENVVAR_TEST")]
        public void InterpolatedEnvVarShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.InterpolatedEnvVar.AtEnd().Parse(input).GetValue());

        [Theory]
        [InlineData("ENV value", "${ENVVAR_TEST}")]
        public void InterpolatedBracesEnvVarShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.InterpolatedBracesEnvVar.AtEnd().Parse(input).GetValue());

        [Theory]
        [InlineData("ENV value", "$ENVVAR_TEST")]
        [InlineData("ENV value", "${ENVVAR_TEST}")]
        [InlineData("", "${ENVVAR_TEST_DNE}")]
        public void InterpolatedValueShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.InterpolatedValue.AtEnd().Parse(input).GetValue());

        [Theory]
        [InlineData("a", "a", "")]
        [InlineData("%", "%", "")]
        [InlineData("\"", "\"", "1")]
        [InlineData("$","$", "1")]
        [InlineData("a","a", "1")]
        public void NotControlNorWhitespaceShouldParseUntilEnd(string expected, string input, string excludedChars) =>
            Assert.Equal(expected, Parsers.NotControlNorWhitespace(excludedChars).AtEnd().Parse(input));

        [Theory]
        [InlineData(" ", "")]
        [InlineData(" ", "1234")]
        [InlineData("\n", "")]
        [InlineData("'", "'")]
        [InlineData("\"", "\"")]
        [InlineData("$", "$")]
        [InlineData("a", "a")]
        public void NotControlNorWhitespaceShouldThrowOnParseUntilEnd(string invalidInput, string excludedChars) =>
            Assert.Throws<ParseException>(() =>
                Parsers.NotControlNorWhitespace(excludedChars).AtEnd().Parse(invalidInput));

        [Theory]
        [InlineData("Z", @"\x5A")]
        [InlineData("®", @"\xc2\xae")]
        [InlineData("☠", @"\xE2\x98\xA0")]
        [InlineData("日", @"\xe6\x97\xa5")]
        [InlineData("本", @"\xe6\x9c\xac")]
        [InlineData("®", @"\u00ae")]
        [InlineData("®", @"\uae")]
        [InlineData("\b", "\\b")]
        [InlineData("\\m", "\\m")]
        [InlineData("'", "\\'")]
        [InlineData("\"", "\\\"")]
        [InlineData(UnicodeChars.Rocket, @"\xF0\x9F\x9A\x80")]
        [InlineData(UnicodeChars.Rocket, @"\U0001F680")]
        [InlineData(UnicodeChars.Rocket, @"\U1F680")]
        public void SpecialCharShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.SpecialChar.AtEnd().Parse(input));

        [Theory]
        [InlineData("a")]
        [InlineData("%")]
        [InlineData(" ")]
        [InlineData("\n")]
        public void SpecialCharShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.SpecialChar.AtEnd().Parse(invalidInput));

        [Theory]
        [InlineData(" comment 1", "# comment 1")]
        [InlineData("", "#")]
        [InlineData(" ", "# ")]
        public void CommentShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.Comment.AtEnd().Parse(input));

        [Theory]
        [InlineData("# comment 1")]
        [InlineData("# comment 2\r\n")]
        [InlineData("# comment 3\n")]
        [InlineData("\r\n")]
        [InlineData("\n")]
        [InlineData("   # comment 1")]
        [InlineData("    \r\n")]
        [InlineData("#export EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"#ccccc\n")]
        public void EmptyShouldParseUntilEnd(string input)
        {
            var expected = new KeyValuePair<string, string>(null, null);

            Assert.Equal(expected, Parsers.Empty.AtEnd().Parse(input));
        }

        [Theory]
        [InlineData("")]
        [InlineData("that's not empty")]
        public void EmptyShouldThrowOnParseUntilEnd(string input) =>
            Assert.Throws<ParseException>(() => Parsers.Empty.AtEnd().Parse(input));

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a b c", "a b c")]
        [InlineData("041", "041")]
        [InlineData("日本", "日本")]
        [InlineData("a\\?b", "a\\?b")]
        [InlineData(@"\xe6\x97\xa5ENV value本", "\\xe6\\x97\\xa5${ENVVAR_TEST}本")]
        public void UnquotedValueShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.UnquotedValue.AtEnd().Parse(input).Value);

        [Theory]
        [InlineData("0", "0\n1")] // value ends on linebreak
        [InlineData("", "#")] // no value, empty comment
        [InlineData("", "#commentOnly")]
        public void UnquotedValueShouldParse(string expected, string input) =>
            Assert.Equal(expected, Parsers.UnquotedValue.Parse(input).Value);

        [Theory]
        [InlineData("0\n1")] // linebreak within value
        [InlineData("'")] // single quote only
        [InlineData("\"")] // double quote only
        [InlineData("'value")] // leading single quote
        [InlineData("\"value")] // leading double quote
        [InlineData(" value")] // leading whitespace
        [InlineData("\tvalue")] // leading tab
        [InlineData("a'b'c")] // inline single quote
        [InlineData("a\"b\"c")] // inline double quote
        [InlineData("a 'b' c")] // inline single quotes surrounded with whitespaces 
        [InlineData("a \"b\" c")] // inline double quotes surrounded with whitespaces
        public void UnquotedValueShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse(invalidInput));

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a b c", "a b c")]
        [InlineData("0\n1", "0\n1")]
        [InlineData("日", @"\xe6\x97\xa5")]
        [InlineData("本", @"\xe6\x9c\xac")]
        [InlineData("日 本", @"\xe6\x97\xa5 \xe6\x9c\xac")]
        [InlineData("日本", @"\xe6\x97\xa5\xe6\x9c\xac")]
        [InlineData("☠ ®", @"\xE2\x98\xA0 \uae")]
        [InlineData("日 ENV value 本", @"\xe6\x97\xa5 $ENVVAR_TEST 本")]
        [InlineData("a\"b c", "a\\\"b c")]
        [InlineData("a'b c", "a'b c")]
        public void DoubleQuotedValueContentsShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.DoubleQuotedValueContents.AtEnd().Parse(input).Value);

        [Fact]
        public void ParseSingleQuotedValueContents ()
        {
            Assert.Equal("abc", Parsers.SingleQuotedValueContents.AtEnd().Parse("abc").Value);
            Assert.Equal("a b c", Parsers.SingleQuotedValueContents.AtEnd().Parse("a b c").Value);
            Assert.Equal("0\n1", Parsers.SingleQuotedValueContents.AtEnd().Parse("0\n1").Value);
            Assert.Equal(@"\xe6\x97\xa5 \xe6\x9c\xac", Parsers.SingleQuotedValueContents.AtEnd().Parse(@"\xe6\x97\xa5 \xe6\x9c\xac").Value);
            Assert.Equal(@"\xE2\x98\xA0 \uae", Parsers.SingleQuotedValueContents.AtEnd().Parse(@"\xE2\x98\xA0 \uae").Value);

            Assert.Equal("\\xe6\\x97\\xa5 $ENVVAR_TEST 本", Parsers.SingleQuotedValueContents.AtEnd().Parse("\\xe6\\x97\\xa5 $ENVVAR_TEST 本").Value);

            Assert.Equal("a\"b c", Parsers.SingleQuotedValueContents.AtEnd().Parse("a\"b c").Value);
        }

        [Fact]
        public void ParseSingleQuotedValue ()
        {
            Assert.Equal("abc", Parsers.SingleQuotedValue.AtEnd().Parse("'abc'").Value);
            Assert.Equal("a b c", Parsers.SingleQuotedValue.AtEnd().Parse("'a b c'").Value);
            Assert.Equal("0\n1", Parsers.SingleQuotedValue.AtEnd().Parse("'0\n1'").Value);
            Assert.Equal("a\"bc", Parsers.SingleQuotedValue.AtEnd().Parse("'a\"bc'").Value);

            Assert.Equal("\\xe6\\x97\\xa5 $ENVVAR_TEST 本", Parsers.SingleQuotedValue.AtEnd().Parse("'\\xe6\\x97\\xa5 $ENVVAR_TEST 本'").Value);

            Assert.Throws<ParseException>(() => Parsers.SingleQuotedValue.AtEnd().Parse("'a\\'b c'").Value);
        }

        [Fact]
        public void ParseDoubleQuotedValue ()
        {
            Assert.Equal("abc", Parsers.DoubleQuotedValue.AtEnd().Parse("\"abc\"").Value);
            Assert.Equal("a b c", Parsers.DoubleQuotedValue.AtEnd().Parse("\"a b c\"").Value);
            Assert.Equal("0\n1", Parsers.DoubleQuotedValue.AtEnd().Parse("\"0\n1\"").Value);
            Assert.Equal("a'bc", Parsers.DoubleQuotedValue.AtEnd().Parse("\"a'bc\"").Value);
            Assert.Equal("a\"bc", Parsers.DoubleQuotedValue.AtEnd().Parse("\"a\\\"bc\"").Value);

            Assert.Equal("日 ENV value 本", Parsers.DoubleQuotedValue.AtEnd().Parse("\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"").Value);
        }

        [Fact]
        public void TestExportExpression()
        {
            Assert.Throws<ParseException>(() => Parsers.ExportExpression.AtEnd().Parse("identifier "));
            Assert.Equal("export", Parsers.ExportExpression.AtEnd().Parse("export "));
            Assert.Equal("set -x", Parsers.ExportExpression.AtEnd().Parse("set -x "));
            Assert.Equal("set", Parsers.ExportExpression.AtEnd().Parse("set "));
            Assert.Equal("SET", Parsers.ExportExpression.AtEnd().Parse("SET "));
        }

        [Fact]
        public void ParseValue ()
        {
            Assert.Equal("abc", Parsers.Value.AtEnd().Parse("abc").Value);
            Assert.Equal("a b c", Parsers.Value.AtEnd().Parse("a b c").Value);
            Assert.Equal("a b c", Parsers.Value.AtEnd().Parse("'a b c'").Value);
            Assert.Equal("a#b#c", Parsers.Value.AtEnd().Parse("a#b#c").Value);
            Assert.Equal("041", Parsers.Value.AtEnd().Parse("041").Value);
            Assert.Equal("日本", Parsers.Value.AtEnd().Parse("日本").Value);

            Assert.Equal(@"\xe6\x97\xa5\xe6\x9c\xac", Parsers.Value.AtEnd().Parse(@"\xe6\x97\xa5\xe6\x9c\xac").Value);
            Assert.Equal(@"\xe6\x97\xa5\xe6\x9c\xac", Parsers.Value.AtEnd().Parse(@"'\xe6\x97\xa5\xe6\x9c\xac'").Value);
            Assert.Equal("日本", Parsers.Value.AtEnd().Parse("\"\\xe6\\x97\\xa5\\xe6\\x9c\\xac\"").Value);

            Assert.Throws<ParseException>(() => Parsers.Value.AtEnd().Parse("0\n1"));
            Assert.Throws<ParseException>(() => Parsers.Value.Parse(" "));

            Assert.Equal("0", Parsers.Value.Parse("0\n1").Value);   // value ends on linebreak

            // throw on unmatched quotes
            Assert.Throws<ParseException>(() => Parsers.Value.Parse("'"));
            Assert.Throws<ParseException>(() => Parsers.Value.Parse("\""));

            Assert.Equal(@"\xe6\x97\xa5", Parsers.Value.AtEnd().Parse(@"\xe6\x97\xa5").Value);
            Assert.Equal(@"\xE2\x98\xA0", Parsers.Value.AtEnd().Parse(@"\xE2\x98\xA0").Value);

            Assert.Equal("abc", Parsers.Value.AtEnd().Parse("'abc'").Value);
            Assert.Equal("a b c", Parsers.Value.AtEnd().Parse("'a b c'").Value);
            Assert.Equal("0\n1", Parsers.Value.AtEnd().Parse("'0\n1'").Value);
            Assert.Equal(@"\xe6\x97\xa5 \xe6\x9c\xac", Parsers.Value.AtEnd().Parse(@"'\xe6\x97\xa5 \xe6\x9c\xac'").Value);
            Assert.Equal(@"\xE2\x98\xA0 \uae", Parsers.Value.AtEnd().Parse(@"'\xE2\x98\xA0 \uae'").Value);

            Assert.Equal("abc", Parsers.Value.AtEnd().Parse("\"abc\"").Value);
            Assert.Equal("a b c", Parsers.Value.AtEnd().Parse("\"a b c\"").Value);
            Assert.Equal("0\n1", Parsers.Value.AtEnd().Parse("\"0\n1\"").Value);
            Assert.Equal("日 本", Parsers.Value.AtEnd().Parse("\"\\xe6\\x97\\xa5 \\xe6\\x9c\\xac\"").Value);
            Assert.Equal("☠ ®", Parsers.Value.AtEnd().Parse("\"\\xE2\\x98\\xA0 \\uae\"").Value);

            Assert.Equal("日 ENV value 本", Parsers.Value.AtEnd().Parse("\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"").Value);
        }

        [Fact]
        public void ParseAssignment ()
        {
            Action<string, string, string> testParse = (key, value, input) =>
            {
                var kvp = Parsers.Assignment.AtEnd().Parse(input);
                Assert.Equal(key, kvp.Key);
                Assert.Equal(value, kvp.Value);
            };

            testParse("EV_DNE", "abc", "EV_DNE=abc");
            testParse("EV_DNE", "a b c", "EV_DNE=a b c");
            testParse("EV_DNE", "a b c", "EV_DNE='a b c'");
            testParse("EV_DNE", "041", "EV_DNE=041 # comment");
            // Note that there are no comments without whitespace in unquoted strings!
            testParse("EV_DNE", "日本#c", "EV_DNE=日本#c");

            testParse("EV_DNE", @"\xe6\x97\xa5 \xe6\x9c\xac", @"EV_DNE=\xe6\x97\xa5 \xe6\x9c\xac");
            testParse("EV_DNE", @"\xE2\x98\xA0 \uae", @"EV_DNE=\xE2\x98\xA0 \uae");

            var kvp = Parsers.Assignment.AtEnd().Parse("EV_DNE=");
            Assert.Equal("EV_DNE", kvp.Key);
            Assert.Equal("", kvp.Value);
            // Note that dotnet returns null if the env var is empty -- even if it was set to empty!
            Assert.Null(Environment.GetEnvironmentVariable("EV_DNE"));

            // TODO: is it possible to get the system to recognize when a complete unicode char is present and start the next one then, without a space?
//            Assert.Equal("EV_DNE=日本", Parsers.Assignment.AtEnd().Parse(@"EV_DNE=\xe6\x97\xa5\xe6\x9c\xac"));

            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE='"));
            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE=0\n1"));

            testParse("EV_DNE", "", "EV_DNE=");
            testParse("EV_DNE", "EV_DNE=", "EV_DNE=EV_DNE=");

            testParse("EV_DNE", "test", "EV_DNE= test #basic comment");
            testParse("EV_DNE", "", "EV_DNE=#no value just comment");
            testParse("EV_DNE", "", "EV_DNE= #no value just comment");
            testParse("EV_DNE", "a#b#c", "EV_DNE=a#b#c #inner hashes are allowed in unquoted value");
            testParse("EV_DNE", "test", "EV_DNE= test  #a'bc allow singleQuotes in comment");
            testParse("EV_DNE", "test", "EV_DNE= test  #a\"bc allow doubleQuotes in comment");
            testParse("EV_DNE", "test", "EV_DNE= test #a$bc allow dollarSign in comment");
            testParse("EV_DNE", "a#b#c# not a comment", "EV_DNE=a#b#c# not a comment");

            testParse("EV_DNE", "http://www.google.com/#anchor", "EV_DNE=http://www.google.com/#anchor #inner hash is part of value");

            testParse("EV_DNE", "abc", "EV_DNE='abc'");
            testParse("EV_DNE", "a b c", "EV_DNE='a b c' # comment");
            testParse("EV_DNE", "0\n1", "EV_DNE='0\n1'");
            testParse("EV_DNE", @"\xe6\x97\xa5 \xe6\x9c\xac", @"set -x EV_DNE='\xe6\x97\xa5 \xe6\x9c\xac'#c");
            testParse("EV_DNE", @"\xE2\x98\xA0 \uae", @"EV_DNE='\xE2\x98\xA0 \uae'#c");

            testParse("EV_DNE", "abc", "EV_DNE=\"abc\"");
            testParse("EV_DNE", "a b c", "set EV_DNE=\"a b c\" # comment");
            testParse("EV_DNE", "0\n1", "EV_DNE=\"0\n1\"");
            testParse("EV_DNE", "日 本", "export EV_DNE=\"\\xe6\\x97\\xa5 \\xe6\\x9c\\xac\"#c");
            testParse("EV_DNE", "☠ ®", "EV_DNE=\"\\xE2\\x98\\xA0 \\uae\"");

            testParse("EV_DNE", "日 ENV value 本", "export EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"#ccccc");

            testParse("exportEV_DNE", "abc", "exportEV_DNE=\"abc\"");

            testParse("EV_DNE", "a b c", "EV_DNE = 'a b c' # comment");
            testParse("EV_DNE", "a b c", "EV_DNE= \"a b c\" # comment");
            testParse("EV_DNE", "a b c", "EV_DNE ='a b c' # comment");
            testParse("EV_DNE", "abc", "EV_DNE = abc # comment");

            testParse("EV_DNE", "a'b'' 'c' d", "EV_DNE=\"a'b'' 'c' d\" #allow singleQuotes in doubleQuoted values");
            testParse("EV_DNE", "a\"b\"\" \"c\" d", "EV_DNE='a\"b\"\" \"c\" d' #allow doubleQuotes in singleQuoted values");
            testParse("EV_DNE", "a\"b\"\" \"c\" d", "EV_DNE=\"a\\\"b\\\"\\\" \\\"c\\\" d\" #allow escaped doubleQuotes in doubleQuoted values");
            Assert.Throws<ParseException>(() => Parsers.Assignment.Parse("EV_DNE='a'b'' 'c' d'"));  // no singleQuotes inside singleQuoted values
            Assert.Throws<ParseException>(() => Parsers.Assignment.Parse("EV_DNE=\"a\"b\""));  // no unescaped doubleQuotes inside doubleQuoted values

            testParse("EV_DNE", "VAL UE", "EV_DNE=VAL UE");
            testParse("EV_DNE", "VAL UE", "EV_DNE=VAL UE #comment");

            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE='a b c'EV_TEST_1=more"));
            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE='a b c' EV_TEST_1=more"));
        }

        [Fact]
        public void ParseDotenvFile ()
        {
            Action<KeyValuePair<string, string>[], string> testParse = (expecteds, input) =>
            {
                var outputs = Parsers.ParseDotenvFile(input, Parsers.SetEnvVar).ToArray();
                Assert.Equal(expecteds.Length, outputs.Length);

                for (var i = 0; i < outputs.Length; i++)
                {
                    Assert.Equal(expecteds[i].Key, outputs[i].Key);
                    Assert.Equal(expecteds[i].Value, outputs[i].Value);
                    Assert.Equal(expecteds[i].Value, Environment.GetEnvironmentVariable(outputs[i].Key));
                }
            };

            string contents;
            KeyValuePair<string, string>[] expecteds;

            contents = @"";
            expecteds = new KeyValuePair<string, string>[] {};
            testParse(expecteds, contents);

            contents = @"EV_DNE=abc";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "abc"),
            };
            testParse(expecteds, contents);

            contents = "SET EV_DNE=\"0\n1\"";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "0\n1"),
            };
            testParse(expecteds, contents);

            contents = "EV_DNE=0\n1";
            Assert.Throws<ParseException>(() => Parsers.ParseDotenvFile(contents, Parsers.SetEnvVar));

            contents = @"
# this is a header

export EV_DNE='a b c' #works!
";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "a b c"),
            };
            testParse(expecteds, contents);

            contents = "# this is a header\nexport EV_DNE='d e f' #works!";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "d e f"),
            };
            testParse(expecteds, contents);

            contents = "#\n# this is a header\n#\n\nexport EV_DNE='g h i' #yep still\n";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "g h i"),
            };
            testParse(expecteds, contents);

            contents = "#\n# this is a header\n#\n\nexport EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\" #yep still\n";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "日 ENV value 本"),
            };
            testParse(expecteds, contents);

            contents = @"#
# this is a header
#

export EV_DNE='x y z' #yep still

     EV_TEST_1='日 $ENVVAR_TEST 本'#ccccc
#export EV_DNE='日 $ENVVAR_TEST 本'#ccccc

SET EV_TEST_2='☠
®'#c

ENVVAR_TEST = ' yahooooo '
";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "x y z"),
                new KeyValuePair<string, string>("EV_TEST_1", "日 $ENVVAR_TEST 本"),
                new KeyValuePair<string, string>("EV_TEST_2", "☠\n®"),
                new KeyValuePair<string, string>("ENVVAR_TEST", " yahooooo "),
            };
            testParse(expecteds, contents);
        }

        // C# wow that you can't handle 32 bit unicode as chars. wow. strings for 4 byte chars.
        private struct UnicodeChars
        {
            // https://stackoverflow.com/questions/602912/how-do-you-echo-a-4-digit-unicode-character-in-bash
            // printf '\xE2\x98\xA0'
            // printf ☠ | hexdump  # hexdump has bytes flipped per word (2 bytes, 4 hex)

            public const string Rocket = "\ud83d\ude80"; // 🚀
        }
    }
}
