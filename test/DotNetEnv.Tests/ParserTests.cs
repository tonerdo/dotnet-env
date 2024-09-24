using System;
using System.Collections.Concurrent;
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

        public ParserTests ()
        {
            Parsers.EnvVarSnapshot = new ConcurrentDictionary<string, string>()
            {
                [EV_TEST] = "ENV value"
            };
        }

        public void Dispose ()
        {
            Parsers.EnvVarSnapshot.Clear();
        }

        [Fact]
        public void ParseIdentifier ()
        {
            Assert.Equal("name", Parsers.Identifier.AtEnd().Parse("name"));
            Assert.Equal("_n", Parsers.Identifier.AtEnd().Parse("_n"));
            Assert.Equal("__", Parsers.Identifier.AtEnd().Parse("__"));
            Assert.Equal("_0", Parsers.Identifier.AtEnd().Parse("_0"));
            Assert.Equal("a_b", Parsers.Identifier.AtEnd().Parse("a_b"));
            Assert.Equal("_a_b", Parsers.Identifier.AtEnd().Parse("_a_b"));
            Assert.Equal("a.b", Parsers.Identifier.AtEnd().Parse("a.b"));
            Assert.Equal("a-b", Parsers.Identifier.AtEnd().Parse("a-b"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("\"name"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("0name"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse(".a.b"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("-a.b"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("a!b"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("a?b"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("a*b"));
            Assert.Throws<ParseException>(() => Parsers.Identifier.AtEnd().Parse("a:b"));
        }

        [Fact]
        public void ParseOctalByte ()
        {
            Assert.Equal(33, Parsers.OctalByte.AtEnd().Parse(@"\41"));
            Assert.Equal(33, Parsers.OctalByte.AtEnd().Parse(@"\041"));
            Assert.Equal(90, Parsers.OctalByte.AtEnd().Parse(@"\132"));

            // NOTE that bash accepts values outside of ASCII range?
            // printf "\412"
            //Assert.Equal(???, Parsers.OctalChar.AtEnd().Parse(@"\412"));
        }

        [Fact]
        public void ParseOctalChar ()
        {
            Assert.Equal("!", Parsers.OctalChar.AtEnd().Parse(@"\41"));
            Assert.Equal("!", Parsers.OctalChar.AtEnd().Parse(@"\041"));
            Assert.Equal("Z", Parsers.OctalChar.AtEnd().Parse(@"\132"));

            // as above for values outside of ASCII range

            // TODO: tests for octal combinations to utf8?
        }

        [Fact]
        public void ParseHexByte ()
        {
            Assert.Equal(90, Parsers.HexByte.AtEnd().Parse(@"\x5a"));
        }

        [Fact]
        public void ParseUtf8Char ()
        {
            // https://stackoverflow.com/questions/602912/how-do-you-echo-a-4-digit-unicode-character-in-bash
            // printf '\xE2\x98\xA0'
            // printf ☠ | hexdump  # hexdump has bytes flipped per word (2 bytes, 4 hex)

            Assert.Equal("Z", Parsers.Utf8Char.AtEnd().Parse(@"\x5A"));
            Assert.Equal("®", Parsers.Utf8Char.AtEnd().Parse(@"\xc2\xae"));
            Assert.Equal("☠", Parsers.Utf8Char.AtEnd().Parse(@"\xE2\x98\xA0"));
            Assert.Equal(RocketChar, Parsers.Utf8Char.AtEnd().Parse(@"\xF0\x9F\x9A\x80"));

            Assert.Equal(
                "日本",
                Parsers.Utf8Char.AtEnd().Parse(@"\xe6\x97\xa5")
                    + Parsers.Utf8Char.AtEnd().Parse(@"\xe6\x9c\xac")
            );
        }

        [Fact]
        public void ParseUtf16Char ()
        {
            Assert.Equal("®", Parsers.Utf16Char.AtEnd().Parse(@"\u00ae"));
            Assert.Equal("®", Parsers.Utf16Char.AtEnd().Parse(@"\uae"));

            Assert.Equal("®", Encoding.Unicode.GetString(new byte[] { 0xae, 0x00 }));
        }

        [Fact]
        public void ParseUtf32Char ()
        {
            Assert.Equal(RocketChar, Parsers.Utf32Char.AtEnd().Parse(@"\U0001F680"));
            Assert.Equal(RocketChar, Parsers.Utf32Char.AtEnd().Parse(@"\U1F680"));

            Assert.Equal(RocketChar, Encoding.UTF32.GetString(new byte[] { 0x80, 0xf6, 0x01, 0x00 }));
            Assert.Equal(RocketChar, Encoding.UTF32.GetString(new byte[] { 0x80, 0xf6, 0x1, 0x0 }));
        }

        [Fact]
        public void ParseEscapedChar ()
        {
            Assert.Equal("\b", Parsers.EscapedChar.AtEnd().Parse("\\b"));
            Assert.Equal("'", Parsers.EscapedChar.AtEnd().Parse("\\'"));
            Assert.Equal("\"", Parsers.EscapedChar.AtEnd().Parse("\\\""));
            Assert.Throws<ParseException>(() => Parsers.EscapedChar.AtEnd().Parse("\n"));
        }

        [Fact]
        public void ParseInterpolatedEnvVar ()
        {
            Assert.Equal("ENV value", Parsers.InterpolatedEnvVar.AtEnd().Parse("$ENVVAR_TEST").GetValue());
            Assert.Equal("ENV value", Parsers.InterpolatedBracesEnvVar.AtEnd().Parse("${ENVVAR_TEST}").GetValue());
        }

        [Fact]
        public void ParseInterpolated ()
        {
            Assert.Equal("ENV value", Parsers.InterpolatedValue.AtEnd().Parse("$ENVVAR_TEST").GetValue());
            Assert.Equal("ENV value", Parsers.InterpolatedValue.AtEnd().Parse("${ENVVAR_TEST}").GetValue());
            Assert.Equal("", Parsers.InterpolatedValue.AtEnd().Parse("${ENVVAR_TEST_DNE}").GetValue());
        }

        [Fact]
        public void ParseNotControlNorWhitespace ()
        {
            Assert.Equal("a", Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse("a"));
            Assert.Equal("%", Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse("%"));
            Assert.Throws<ParseException>(() => Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse(" "));
            Assert.Throws<ParseException>(() => Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse("\n"));
            Assert.Throws<ParseException>(() => Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse("'"));
            Assert.Throws<ParseException>(() => Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse("\""));
            Assert.Throws<ParseException>(() => Parsers.NotControlNorWhitespace(EXCEPT_CHARS).AtEnd().Parse("$"));
        }

        [Fact]
        public void ParseSpecialChar ()
        {
            Assert.Equal("Z", Parsers.SpecialChar.AtEnd().Parse(@"\x5A"));
            Assert.Equal("®", Parsers.SpecialChar.AtEnd().Parse(@"\xc2\xae"));
            Assert.Equal("☠", Parsers.SpecialChar.AtEnd().Parse(@"\xE2\x98\xA0"));
            Assert.Equal(RocketChar, Parsers.SpecialChar.AtEnd().Parse(@"\xF0\x9F\x9A\x80"));
            Assert.Equal(
                "日本",
                Parsers.SpecialChar.AtEnd().Parse(@"\xe6\x97\xa5")
                    + Parsers.SpecialChar.AtEnd().Parse(@"\xe6\x9c\xac")
            );

            Assert.Equal("®", Parsers.SpecialChar.AtEnd().Parse(@"\u00ae"));
            Assert.Equal("®", Parsers.SpecialChar.AtEnd().Parse(@"\uae"));

            Assert.Equal(RocketChar, Parsers.SpecialChar.AtEnd().Parse(@"\U0001F680"));
            Assert.Equal(RocketChar, Parsers.SpecialChar.AtEnd().Parse(@"\U1F680"));

            Assert.Equal("\b", Parsers.SpecialChar.AtEnd().Parse("\\b"));
            Assert.Equal("\\m", Parsers.SpecialChar.AtEnd().Parse("\\m"));
            Assert.Equal("'", Parsers.SpecialChar.AtEnd().Parse("\\'"));
            Assert.Equal("\"", Parsers.SpecialChar.AtEnd().Parse("\\\""));

            // this caught a bug, once upon a time, where backslashes were
            // getting processed by NCNW rather than EscapedChar inside SpecialChar
            var parser = Parsers.SpecialChar
                .Or(Parsers.NotControlNorWhitespace("\"$"))
                .Or(Character.WhiteSpace.AtLeastOnce().Text());
            Assert.Equal("\"", parser.AtEnd().Parse("\\\""));

            Assert.Throws<ParseException>(() => Parsers.SpecialChar.AtEnd().Parse("a"));
            Assert.Throws<ParseException>(() => Parsers.SpecialChar.AtEnd().Parse("%"));
            Assert.Throws<ParseException>(() => Parsers.SpecialChar.AtEnd().Parse(" "));
            Assert.Throws<ParseException>(() => Parsers.SpecialChar.AtEnd().Parse("\n"));
        }

        [Fact]
        public void ParseComment ()
        {
            Assert.Equal(" comment 1", Parsers.Comment.AtEnd().Parse("# comment 1"));
            Assert.Equal("", Parsers.Comment.AtEnd().Parse("#"));
            Assert.Equal(" ", Parsers.Comment.AtEnd().Parse("# "));
        }

        [Fact]
        public void ParseEmpty ()
        {
            var kvp = new KeyValuePair<string, string>(null, null);

            Assert.Throws<ParseException>(() => Parsers.Empty.AtEnd().Parse(""));

            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("# comment 1"));
            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("# comment 2\r\n"));
            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("# comment 3\n"));

            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("\r\n"));
            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("\n"));

            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("   # comment 1"));
            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("    \r\n"));
            Assert.Equal(kvp, Parsers.Empty.AtEnd().Parse("#export EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"#ccccc\n"));
        }

        [Fact]
        public void ParseUnquotedValue ()
        {
            Assert.Equal("abc", Parsers.UnquotedValue.AtEnd().Parse("abc").Value);
            Assert.Equal("a b c", Parsers.UnquotedValue.AtEnd().Parse("a b c").Value);
            Assert.Equal("041", Parsers.UnquotedValue.AtEnd().Parse("041").Value);
            Assert.Equal("日本", Parsers.UnquotedValue.AtEnd().Parse("日本").Value);
            // TODO: is it possible to get the system to recognize when a complete unicode char is present and start the next one then, without a space?
//            Assert.Equal("日本", Parsers.UnquotedValue.Parse(@"\xe6\x97\xa5\xe6\x9c\xac"));

            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse("0\n1"));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse("'"));

            Assert.Equal("0", Parsers.UnquotedValue.Parse("0\n1").Value);   // value ends on linebreak

            // leading singlequotes/doublequotes/whitespaces/tabs are not allowed
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.Parse("'"));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.Parse("\""));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.Parse(" "));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.Parse("\t"));

            Assert.Equal("", Parsers.UnquotedValue.Parse("#").Value); // no value, empty comment
            Assert.Equal("", Parsers.UnquotedValue.Parse("#commentOnly").Value);

            // prevent quotationChars inside unquoted values
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse("a'b'c"));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse("a\"b\"c"));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse("a 'b' c"));
            Assert.Throws<ParseException>(() => Parsers.UnquotedValue.AtEnd().Parse("a \"b\" c"));

            Assert.Equal("a\\?b", Parsers.UnquotedValue.AtEnd().Parse("a\\?b").Value);
            Assert.Equal(@"\xe6\x97\xa5ENV value本", Parsers.UnquotedValue.AtEnd().Parse("\\xe6\\x97\\xa5${ENVVAR_TEST}本").Value);
        }

        [Fact]
        public void ParseDoubleQuotedValueContents ()
        {
            Assert.Equal("abc", Parsers.DoubleQuotedValueContents.AtEnd().Parse("abc").Value);
            Assert.Equal("a b c", Parsers.DoubleQuotedValueContents.AtEnd().Parse("a b c").Value);
            Assert.Equal("0\n1", Parsers.DoubleQuotedValueContents.AtEnd().Parse("0\n1").Value);
            Assert.Equal("日 本", Parsers.DoubleQuotedValueContents.AtEnd().Parse(@"\xe6\x97\xa5 \xe6\x9c\xac").Value);
            Assert.Equal("☠ ®", Parsers.DoubleQuotedValueContents.AtEnd().Parse(@"\xE2\x98\xA0 \uae").Value);

            Assert.Equal("日 ENV value 本", Parsers.DoubleQuotedValueContents.AtEnd().Parse("\\xe6\\x97\\xa5 $ENVVAR_TEST 本").Value);

            Assert.Equal("a\"b c", Parsers.DoubleQuotedValueContents.AtEnd().Parse("a\\\"b c").Value);
            Assert.Equal("a'b c", Parsers.DoubleQuotedValueContents.AtEnd().Parse("a'b c").Value);
        }

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
            // TODO: is it possible to get the system to recognize when a complete unicode char is present and start the next one then, without a space?
//            Assert.Equal("日本", Parsers.Value.AtEnd().Parse(@"\xe6\x97\xa5\xe6\x9c\xac"));

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
            void TestParse(string key, string value, string input)
            {
                var kvp = Parsers.Assignment.AtEnd().Parse(input);
                Assert.Equal(key, kvp.Key);
                Assert.Equal(value, kvp.Value);
            }

            TestParse("EV_DNE", "abc", "EV_DNE=abc");
            TestParse("EV_DNE", "a b c", "EV_DNE=a b c");
            TestParse("EV_DNE", "a b c", "EV_DNE='a b c'");
            TestParse("EV_DNE", "041", "EV_DNE=041 # comment");
            // Note that there are no comments without whitespace in unquoted strings!
            TestParse("EV_DNE", "日本#c", "EV_DNE=日本#c");

            TestParse("EV_DNE", @"\xe6\x97\xa5 \xe6\x9c\xac", @"EV_DNE=\xe6\x97\xa5 \xe6\x9c\xac");
            TestParse("EV_DNE", @"\xE2\x98\xA0 \uae", @"EV_DNE=\xE2\x98\xA0 \uae");

            var kvp = Parsers.Assignment.AtEnd().Parse("EV_DNE=");
            Assert.Equal("EV_DNE", kvp.Key);
            Assert.Equal("", kvp.Value);

            // TODO: is it possible to get the system to recognize when a complete unicode char is present and start the next one then, without a space?
//            Assert.Equal("EV_DNE=日本", Parsers.Assignment.AtEnd().Parse(@"EV_DNE=\xe6\x97\xa5\xe6\x9c\xac"));

            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE='"));
            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE=0\n1"));

            TestParse("EV_DNE", "", "EV_DNE=");
            TestParse("EV_DNE", "EV_DNE=", "EV_DNE=EV_DNE=");

            TestParse("EV_DNE", "test", "EV_DNE= test #basic comment");
            TestParse("EV_DNE", "", "EV_DNE=#no value just comment");
            TestParse("EV_DNE", "", "EV_DNE= #no value just comment");
            TestParse("EV_DNE", "a#b#c", "EV_DNE=a#b#c #inner hashes are allowed in unquoted value");
            TestParse("EV_DNE", "test", "EV_DNE= test  #a'bc allow singleQuotes in comment");
            TestParse("EV_DNE", "test", "EV_DNE= test  #a\"bc allow doubleQuotes in comment");
            TestParse("EV_DNE", "test", "EV_DNE= test #a$bc allow dollarSign in comment");
            TestParse("EV_DNE", "a#b#c# not a comment", "EV_DNE=a#b#c# not a comment");

            TestParse("EV_DNE", "http://www.google.com/#anchor", "EV_DNE=http://www.google.com/#anchor #inner hash is part of value");

            TestParse("EV_DNE", "abc", "EV_DNE='abc'");
            TestParse("EV_DNE", "a b c", "EV_DNE='a b c' # comment");
            TestParse("EV_DNE", "0\n1", "EV_DNE='0\n1'");
            TestParse("EV_DNE", @"\xe6\x97\xa5 \xe6\x9c\xac", @"set -x EV_DNE='\xe6\x97\xa5 \xe6\x9c\xac'#c");
            TestParse("EV_DNE", @"\xE2\x98\xA0 \uae", @"EV_DNE='\xE2\x98\xA0 \uae'#c");

            TestParse("EV_DNE", "abc", "EV_DNE=\"abc\"");
            TestParse("EV_DNE", "a b c", "set EV_DNE=\"a b c\" # comment");
            TestParse("EV_DNE", "0\n1", "EV_DNE=\"0\n1\"");
            TestParse("EV_DNE", "日 本", "export EV_DNE=\"\\xe6\\x97\\xa5 \\xe6\\x9c\\xac\"#c");
            TestParse("EV_DNE", "☠ ®", "EV_DNE=\"\\xE2\\x98\\xA0 \\uae\"");

            TestParse("EV_DNE", "日 ENV value 本", "export EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\"#ccccc");

            TestParse("exportEV_DNE", "abc", "exportEV_DNE=\"abc\"");

            TestParse("EV_DNE", "a b c", "EV_DNE = 'a b c' # comment");
            TestParse("EV_DNE", "a b c", "EV_DNE= \"a b c\" # comment");
            TestParse("EV_DNE", "a b c", "EV_DNE ='a b c' # comment");
            TestParse("EV_DNE", "abc", "EV_DNE = abc # comment");

            TestParse("EV_DNE", "a'b'' 'c' d", "EV_DNE=\"a'b'' 'c' d\" #allow singleQuotes in doubleQuoted values");
            TestParse("EV_DNE", "a\"b\"\" \"c\" d", "EV_DNE='a\"b\"\" \"c\" d' #allow doubleQuotes in singleQuoted values");
            TestParse("EV_DNE", "a\"b\"\" \"c\" d", "EV_DNE=\"a\\\"b\\\"\\\" \\\"c\\\" d\" #allow escaped doubleQuotes in doubleQuoted values");
            Assert.Throws<ParseException>(() => Parsers.Assignment.Parse("EV_DNE='a'b'' 'c' d'"));  // no singleQuotes inside singleQuoted values
            Assert.Throws<ParseException>(() => Parsers.Assignment.Parse("EV_DNE=\"a\"b\""));  // no unescaped doubleQuotes inside doubleQuoted values

            TestParse("EV_DNE", "VAL UE", "EV_DNE=VAL UE");
            TestParse("EV_DNE", "VAL UE", "EV_DNE=VAL UE #comment");

            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE='a b c'EV_TEST_1=more"));
            Assert.Throws<ParseException>(() => Parsers.Assignment.AtEnd().Parse("EV_DNE='a b c' EV_TEST_1=more"));
        }

        [Fact]
        public void ParseDotenvFile ()
        {
            void TestParse(KeyValuePair<string, string>[] expecteds, string input)
            {
                var outputs = Parsers.ParseDotenvFile(input).ToArray();
                Assert.Equal(expecteds.Length, outputs.Length);

                for (var i = 0; i < outputs.Length; i++)
                {
                    Assert.Equal(expecteds[i].Key, outputs[i].Key);
                    Assert.Equal(expecteds[i].Value, outputs[i].Value);
                }
            }

            string contents;
            KeyValuePair<string, string>[] expecteds;

            contents = @"";
            expecteds = new KeyValuePair<string, string>[] {};
            TestParse(expecteds, contents);

            contents = @"EV_DNE=abc";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "abc"),
            };
            TestParse(expecteds, contents);

            contents = "SET EV_DNE=\"0\n1\"";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "0\n1"),
            };
            TestParse(expecteds, contents);

            contents = "EV_DNE=0\n1";
            Assert.Throws<ParseException>(() => Parsers.ParseDotenvFile(contents));

            contents = @"
# this is a header

export EV_DNE='a b c' #works!
";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "a b c"),
            };
            TestParse(expecteds, contents);

            contents = "# this is a header\nexport EV_DNE='d e f' #works!";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "d e f"),
            };
            TestParse(expecteds, contents);

            contents = "#\n# this is a header\n#\n\nexport EV_DNE='g h i' #yep still\n";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "g h i"),
            };
            TestParse(expecteds, contents);

            contents = "#\n# this is a header\n#\n\nexport EV_DNE=\"\\xe6\\x97\\xa5 $ENVVAR_TEST 本\" #yep still\n";
            expecteds = new[] {
                new KeyValuePair<string, string>("EV_DNE", "日 ENV value 本"),
            };
            TestParse(expecteds, contents);

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
            TestParse(expecteds, contents);
        }
    }
}
