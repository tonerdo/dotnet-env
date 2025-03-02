using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using DotNetEnv.Superpower;
using DotNetEnv.Tests.Helper;
using DotNetEnv.Tests.XUnit;
using Xunit;
using Superpower;
using Superpower.Parsers;

namespace DotNetEnv.Tests
{
    public class ParserTests
    {
        private const string EV_TEST = "ENVVAR_TEST";
        private readonly IDictionary<string, string> _actualValuesDictionary = new Dictionary<string, string>()
        {
            [EV_TEST] = "ENV value"
        };

        public ParserTests()
        {
            Parsers.ActualValuesSnapshot = new ConcurrentDictionary<string, string>(_actualValuesDictionary);
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

        [Theory]
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

        /// <summary>
        /// this caught a bug, once upon a time, where backslashes were
        /// processed by NCNW rather than EscapedChar inside SpecialChar
        /// </summary>
        [Fact]
        public void SpecialCharShouldParseEscapedChars()
        {
            var parser = Parsers.SpecialChar
                .Or(Parsers.NotControlNorWhitespace("\"$"))
                .Or(Character.WhiteSpace.AtLeastOnce().Text());

            Assert.Equal("\"", parser.AtEnd().Parse("\\\""));
        }

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

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a b c", "a b c")]
        [InlineData("0\n1", "0\n1")]
        [InlineData(@"\xe6\x97\xa5 \xe6\x9c\xac", @"\xe6\x97\xa5 \xe6\x9c\xac")]
        [InlineData(@"\xE2\x98\xA0 \uae", @"\xE2\x98\xA0 \uae")]
        [InlineData(@"\xe6\x97\xa5 $ENVVAR_TEST 本", @"\xe6\x97\xa5 $ENVVAR_TEST 本")]
        [InlineData("a\"b c", "a\"b c")]
        public void SingleQuotedValueContentsShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.SingleQuotedValueContents.AtEnd().Parse(input).Value);

        [Theory]
        [InlineData("abc", "'abc'")]
        [InlineData("a b c", "'a b c'")]
        [InlineData("0\n1", "'0\n1'")]
        [InlineData("a\"bc", "'a\"bc'")]
        [InlineData(@"\xe6\x97\xa5 $ENVVAR_TEST 本", @"'\xe6\x97\xa5 $ENVVAR_TEST 本'")]
        public void SingleQuotedValueShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.SingleQuotedValue.AtEnd().Parse(input).Value);

        [Theory]
        [InlineData("'a\\'b c'")]
        public void SingleQuotedValueShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.SingleQuotedValue.AtEnd().Parse(invalidInput).Value);

        [Theory]
        [InlineData("abc", "\"abc\"")]
        [InlineData("a b c", "\"a b c\"")]
        [InlineData("0\n1", "\"0\n1\"")]
        [InlineData("a'bc", "\"a'bc\"")]
        [InlineData("a\"bc", "\"a\\\"bc\"")]
        [InlineData("日 ENV value 本", "\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"")]
        public void DoubleQuotedValueShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.DoubleQuotedValue.AtEnd().Parse(input).Value);

        [Theory]
        [InlineData("export", "export ")]
        [InlineData("set -x", "set -x ")]
        [InlineData("set", "set ")]
        [InlineData("SET", "SET ")]
        public void TestExportExpression(string expected, string input) => 
            Assert.Equal(expected, Parsers.ExportExpression.AtEnd().Parse(input));

        [Theory]
        [InlineData("identifier ")]
        public void ExportExpressionShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.ExportExpression.AtEnd().Parse(invalidInput));

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("abc", "'abc'")]
        [InlineData("abc", "\"abc\"")]
        [InlineData("a b c", "a b c")]
        [InlineData("a b c", "'a b c'")]
        [InlineData("a b c", "\"a b c\"")]
        [InlineData("a#b#c", "a#b#c")]
        [InlineData("041", "041")]
        [InlineData("日本", "日本")]
        [InlineData(@"\xe6\x97\xa5\xe6\x9c\xac", @"\xe6\x97\xa5\xe6\x9c\xac")]
        [InlineData(@"\xe6\x97\xa5\xe6\x9c\xac", @"'\xe6\x97\xa5\xe6\x9c\xac'")]
        [InlineData(@"\xe6\x97\xa5 \xe6\x9c\xac", @"'\xe6\x97\xa5 \xe6\x9c\xac'")]
        [InlineData("日本", "\"\\xe6\\x97\\xa5\\xe6\\x9c\\xac\"")]
        [InlineData("日 本", "\"\\xe6\\x97\\xa5 \\xe6\\x9c\\xac\"")]
        [InlineData(@"\xe6\x97\xa5", @"\xe6\x97\xa5")]
        [InlineData(@"\xE2\x98\xA0", @"\xE2\x98\xA0")]
        [InlineData("0\n1", "'0\n1'")]
        [InlineData("0\n1", "\"0\n1\"")]
        [InlineData(@"\xE2\x98\xA0 \uae", @"'\xE2\x98\xA0 \uae'")]
        [InlineData("☠ ®", "\"\\xE2\\x98\\xA0 \\uae\"")]
        [InlineData("日 ENV value 本", "\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"")]
        public void ParseShouldParseUntilEnd(string expected, string input) =>
            Assert.Equal(expected, Parsers.Value.AtEnd().Parse(input).Value);

        [Theory]
        [InlineData("0", "0\n1")] // value ends at linebreak
        public void ParseShouldParse(string expected, string input) =>
            Assert.Equal(expected, Parsers.Value.Parse(input).Value);

        [Theory]
        [InlineData("0\n1")]
        public void ParseShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.Value.AtEnd().Parse(invalidInput));

        [Theory]
        [InlineData(" ")]
        [InlineData("'")] // unmatched quote
        [InlineData("\"")] // unmatched quote
        public void ParseShouldThrowOnParse(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.Value.Parse(invalidInput));

        [Theory]
        [InlineData("EV_DNE", "abc", "EV_DNE=abc")]
        [InlineData("EV_DNE", "a b c", "EV_DNE=a b c")]
        [InlineData("EV_DNE", "a b c", "EV_DNE='a b c'")]
        [InlineData("EV_DNE", "041", "EV_DNE=041 # comment")]
        [InlineData("EV_DNE", "日本#noComment", "EV_DNE=日本#noComment")]
        [InlineData("EV_DNE", @"\xe6\x97\xa5 \xe6\x9c\xac", @"EV_DNE=\xe6\x97\xa5 \xe6\x9c\xac")]
        [InlineData("EV_DNE", @"\xE2\x98\xA0 \uae", @"EV_DNE=\xE2\x98\xA0 \uae")]
        [InlineData("EV_DNE", "", "EV_DNE=")]
        //[InlineData("EV_DNE", "日本", @"EV_DNE=\xe6\x97\xa5\xe6\x9c\xac")] // TODO: is it possible to get the system to recognize when a complete unicode char is present and start the next one then, without a space?
        [InlineData("EV_DNE", "EV_DNE=", "EV_DNE=EV_DNE=")]
        [InlineData("EV_DNE", "test", "EV_DNE= test #basic comment")]
        [InlineData("EV_DNE", "", "EV_DNE=#no value just comment")]
        [InlineData("EV_DNE", "", "EV_DNE= #no value just comment")]
        [InlineData("EV_DNE", "a#b#c", "EV_DNE=a#b#c #inner hashes are allowed in unquoted value")]
        [InlineData("EV_DNE", "test", "EV_DNE= test  #a'bc allow singleQuotes in comment")]
        [InlineData("EV_DNE", "test", "EV_DNE= test  #a\"bc allow doubleQuotes in comment")]
        [InlineData("EV_DNE", "test", "EV_DNE= test #a$bc allow dollarSign in comment")]
        [InlineData("EV_DNE", "a#b#c# not a comment", "EV_DNE=a#b#c# not a comment")]
        [InlineData("EV_DNE", "http://www.google.com/#anchor", "EV_DNE=http://www.google.com/#anchor #inner hash is part of value")]
        [InlineData("EV_DNE", "abc", "EV_DNE='abc'")]
        [InlineData("EV_DNE", "a b c", "EV_DNE='a b c' # comment")]
        [InlineData("EV_DNE", "0\n1", "EV_DNE='0\n1'")]
        [InlineData("EV_DNE", @"\xe6\x97\xa5 \xe6\x9c\xac", @"set -x EV_DNE='\xe6\x97\xa5 \xe6\x9c\xac'#c")]
        [InlineData("EV_DNE", @"\xE2\x98\xA0 \uae", @"EV_DNE='\xE2\x98\xA0 \uae'#c")]
        [InlineData("EV_DNE", "abc", "EV_DNE=\"abc\"")]
        [InlineData("EV_DNE", "a b c", "set EV_DNE=\"a b c\" # comment")]
        [InlineData("EV_DNE", "0\n1", "EV_DNE=\"0\n1\"")]
        [InlineData("EV_DNE", "日 本", "export EV_DNE=\"\\xe6\\x97\\xa5 \\xe6\\x9c\\xac\"#c")]
        [InlineData("EV_DNE", "☠ ®", "EV_DNE=\"\\xE2\\x98\\xA0 \\uae\"")]
        [InlineData("EV_DNE", "日 ENV value 本", "export EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"#ccccc")]
        [InlineData("exportEV_DNE", "abc", "exportEV_DNE=\"abc\"")]
        [InlineData("EV_DNE", "a b c", "EV_DNE = 'a b c' # comment")]
        [InlineData("EV_DNE", "a b c", "EV_DNE= \"a b c\" # comment")]
        [InlineData("EV_DNE", "a b c", "EV_DNE ='a b c' # comment")]
        [InlineData("EV_DNE", "abc", "EV_DNE = abc # comment")]
        [InlineData("EV_DNE", "a'b'' 'c' d", "EV_DNE=\"a'b'' 'c' d\" #allow singleQuotes in doubleQuoted values")]
        [InlineData("EV_DNE", "a\"b\"\" \"c\" d", "EV_DNE='a\"b\"\" \"c\" d' #allow doubleQuotes in singleQuoted values")]
        [InlineData("EV_DNE", "a\"b\"\" \"c\" d", "EV_DNE=\"a\\\"b\\\"\\\" \\\"c\\\" d\" #allow escaped doubleQuotes in doubleQuoted values")]
        [InlineData("EV_DNE", "VAL UE", "EV_DNE=VAL UE")]
        [InlineData("EV_DNE", "VAL UE", "EV_DNE=VAL UE #comment")]
        public void AssignmentShouldParseUntilEnd(string key, string value, string input)
        {
            var expected = new KeyValuePair<string, string>(key, value);
            Assert.Equal(expected, Parsers.Assignment.AtEnd().Parse(input));
        }

        [Theory]
        [InlineData("EV_DNE='")]
        [InlineData("EV_DNE=0\n1")]
        [InlineData("EV_DNE='a b c'EV_TEST_1=more")]
        [InlineData("EV_DNE='a b c' EV_TEST_1=more")]
        public void AssignmentShouldThrowOnParseUntilEnd(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse(invalidInput));
        
        [Theory]
        [InlineData("EV_DNE='a'b'' 'c' d'")] // no singleQuotes inside singleQuoted values
        [InlineData("EV_DNE=\"a\"b\"")] // no unescaped doubleQuotes inside doubleQuoted values
        public void AssignmentShouldThrowOnParse(string invalidInput) =>
            Assert.Throws<ParseException>(() => Parsers.Assignment.Parse(invalidInput));

        /// <summary>
        /// Data: _, contents, expectedPairs
        /// </summary>
        public static readonly TheoryData<string, string, KeyValuePair<string, string>[]> ParseDotEnvTests =
            new IndexedTheoryData<string, KeyValuePair<string, string>[]>()
            {
                { "", Array.Empty<KeyValuePair<string, string>>() },
                { "EV_DNE=abc", new[] { new KeyValuePair<string, string>("EV_DNE", "abc") } },
                { "SET EV_DNE=\"0\n1\"", new[] { new KeyValuePair<string, string>("EV_DNE", "0\n1") } },
                {
                    @"
# this is a header

export EV_DNE='a b c' #works!
",
                    new[] { new KeyValuePair<string, string>("EV_DNE", "a b c") }
                },
                {
                    "# this is a header\nexport EV_DNE='d e f' #works!",
                    new[] { new KeyValuePair<string, string>("EV_DNE", "d e f") }
                },
                {
                    "#\n# this is a header\n#\n\nexport EV_DNE='g h i' #yep still\n",
                    new[] { new KeyValuePair<string, string>("EV_DNE", "g h i") }
                },
                {
                    "#\n# this is a header\n#\n\nexport EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\" #yep still\n",
                    new[] { new KeyValuePair<string, string>("EV_DNE", "日 ENV value 本") }
                },
                {
                    @"#
# this is a header
#

export EV_DNE='x y z' #yep still

     EV_TEST_1='日 $ENVVAR_TEST 本'#ccccc
#export EV_DNE='日 $ENVVAR_TEST 本'#ccccc

SET EV_TEST_2='☠
®'#c

ENVVAR_TEST = ' yahooooo '
",
                    new[]
                    {
                        new KeyValuePair<string, string>("EV_DNE", "x y z"),
                        new KeyValuePair<string, string>("EV_TEST_1", "日 $ENVVAR_TEST 本"),
                        new KeyValuePair<string, string>("EV_TEST_2", "☠\n®"),
                        new KeyValuePair<string, string>("ENVVAR_TEST", " yahooooo "),
                    }
                },
            };

        [Theory]
        [MemberData(nameof(ParseDotEnvTests))]
        public void ParseDotenvFileShouldParseContents(string _, string contents, KeyValuePair<string, string>[] expectedPairs)
        {
            var outputs = Parsers.ParseDotenvFile(contents, actualValues: _actualValuesDictionary).ToArray();
            Assert.Equal(expectedPairs.Length, outputs.Length);

            foreach (var (output, expected) in outputs.Zip(expectedPairs))
                Assert.Equal(expected, output);
        }

        [Theory]
        [InlineData("EV_DNE=0\n1")]
        public void ParseDotenvFileShouldThrowOnContents(string invalidContents) =>
            Assert.Throws<ParseException>(() => Parsers.ParseDotenvFile(invalidContents));
    }
}
