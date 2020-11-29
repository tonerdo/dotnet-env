using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sprache;

namespace DotNetEnv
{
    class Parsers
    {
        public static KeyValuePair<string, string> SetEnvVar (KeyValuePair<string, string> kvp)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            return kvp;
        }

        public static KeyValuePair<string, string> DoNotSetEnvVar (KeyValuePair<string, string> kvp)
        {
            return kvp;
        }

        public static KeyValuePair<string, string> NoClobberSetEnvVar (KeyValuePair<string, string> kvp)
        {
            if (Environment.GetEnvironmentVariable(kvp.Key) == null)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
            // not sure if maybe should return something different if avoided clobber... (current value?)
            // probably not since the point is to return what the dotenv file reported, but it's arguable
            return kvp;
        }

        // helpful blog I discovered only after digging through all the Sprache source myself:
        // https://justinpealing.me.uk/post/2020-03-11-sprache1-chars/

        private static readonly Parser<char> DollarSign = Parse.Char('$');
        private static readonly Parser<char> Backslash = Parse.Char('\\');
        private static readonly Parser<char> Underscore = Parse.Char('_');

        private const string EscapeChars = "abfnrtv\\'\"?$`";

        private static string ToEscapeChar (char escapedChar)
        {
            switch (escapedChar)
            {
                case 'a': return "\a";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
                case '\\': return "\\";
                case '\'': return "'";
                case '"': return "\"";
                case '?': return "?";
                case '$': return "$";
                case '`': return "`";
                default: return $"\\{escapedChar}";
            }
        }

        // https://thomaslevesque.com/2017/02/23/easy-text-parsing-in-c-with-sprache/
        internal static readonly Parser<string> EscapedChar =
            from _ in Backslash
            from c in Parse.AnyChar
            select ToEscapeChar(c);

        //public static readonly Parser<string> Identifier = Parse.RegexMatch(@"[a-zA-Z_][a-zA-Z_0-9]*").Token();
        // https://github.com/sprache/Sprache/blob/c8a3b0c5d06dcf5f0d8d4e0087cd8a628aa6549c/samples/XmlExample/Program.cs#L52
        // I am not clear why that uses XOr instead of just Or, which seems more accurate and cheaper
        // maybe should be using https://github.com/sprache/Sprache/blob/c8a3b0c5d06dcf5f0d8d4e0087cd8a628aa6549c/src/Sprache/Parse.Primitives.cs#L26
        internal static readonly Parser<string> Identifier =
            from head in Parse.Letter.Or(Underscore)
            from tail in Parse.LetterOrDigit.Or(Underscore).Many().Text()
            select head + tail;

        private static byte ToOctalByte (string value)
        {
            return Convert.ToByte(value, 8);
        }

        private static byte ToHexByte (string value)
        {
            return Convert.ToByte(value, 16);
        }

        private static string ToUtf8Char (IEnumerable<byte> value)
        {
            return Encoding.UTF8.GetString(value.ToArray());
        }

        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/311179#311179
        private static byte[] StringToByteArray (String hex, int len)
        {
            hex = hex.PadLeft(len, '0');
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                // note the (len - i - 2) for little endian
                bytes[(len - i - 2) / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static string ToUtf16Char (string hex)
        {
            return Encoding.Unicode.GetString(StringToByteArray(hex, 4));
        }

        private static string ToUtf32Char (string hex)
        {
            return Encoding.UTF32.GetString(StringToByteArray(hex, 8));
        }

        private static readonly Parser<char> Hex = Parse.Chars("0123456789abcdefABCDEF");

        internal static readonly Parser<byte> HexByte =
            from start in Parse.String("\\x")
            from value in Parse.Repeat(Hex, 1, 2).Text()
            select ToHexByte(value);

        internal static readonly Parser<byte> OctalByte =
            from _ in Backslash
            from value in Parse.Repeat(Parse.Chars("01234567"), 1, 3).Text()
            select ToOctalByte(value);

        internal static readonly Parser<string> OctalChar =
            from value in Parse.Repeat(OctalByte, 1, 8)
            select ToUtf8Char(value);

        internal static readonly Parser<string> Utf8Char =
            from value in Parse.Repeat(HexByte, 1, 4)
            select ToUtf8Char(value);

        internal static readonly Parser<string> Utf16Char =
            from start in Parse.String("\\u")
            from value in Parse.Repeat(Hex, 2, 4).Text()
            select ToUtf16Char(value);

        internal static readonly Parser<string> Utf32Char =
            from start in Parse.String("\\U")
            from value in Parse.Repeat(Hex, 2, 8).Text()
            select ToUtf32Char(value);

        internal static Parser<string> NotControlNorWhitespace (string exceptChars) =>
            Parse.Char(
                c => !char.IsControl(c) && !char.IsWhiteSpace(c) && !exceptChars.Contains(c),
                $"not control nor whitespace nor {exceptChars}"
            ).AtLeastOnce().Text();

        internal static readonly Parser<IValue> InterpolatedEnvVar =
            from _d in DollarSign
            from id in Identifier
            select new ValueInterpolated(id);

        internal static readonly Parser<IValue> InterpolatedBracesEnvVar =
            from _d in DollarSign
            from _o in Parse.Char('{')
            from id in Identifier
            from _c in Parse.Char('}')
            select new ValueInterpolated(id);

        internal static readonly Parser<IValue> InterpolatedValue =
            InterpolatedEnvVar.Or(InterpolatedBracesEnvVar);

        internal static readonly Parser<string> SpecialChar =
            Utf32Char
                .Or(Utf16Char)
                .Or(Utf8Char)
                .Or(OctalChar)
                .Or(EscapedChar);

        internal static readonly Parser<ValueCalculator> UnquotedValue =
            InterpolatedValue.Or(SpecialChar
                .Or(NotControlNorWhitespace("'\"\\$"))
                .AtLeastOnce()
                .Select(strs => new ValueActual(strs))
            ).Many().Select(vs => new ValueCalculator(vs));

        // only difference between quoted vs unquoted is addition of whitespace on printablechars
        internal static Parser<ValueCalculator> QuotedValueContents (string exceptChars) =>
            InterpolatedValue.Or(SpecialChar
                .Or(NotControlNorWhitespace(exceptChars))
                .Or(Parse.WhiteSpace.AtLeastOnce().Text())
                .AtLeastOnce()
                .Select(strs => new ValueActual(strs))
            ).Many().Select(vs => new ValueCalculator(vs));

        internal static readonly Parser<ValueCalculator> SingleQuotedValue =
            from _d in Parse.Char('$').Optional()
            from _o in Parse.Char('\'')
            from value in QuotedValueContents("'\\$")
            from _c in Parse.Char('\'')
            select value;

        internal static readonly Parser<ValueCalculator> DoubleQuotedValue =
            from _o in Parse.Char('"')
            from value in QuotedValueContents("\"\\$")
            from _c in Parse.Char('"')
            select value;

        // MUST do single quoted first because of possible leading dollar sign which otherwise would be start of unquoted value
        internal static readonly Parser<ValueCalculator> Value =
            SingleQuotedValue.Or(DoubleQuotedValue).Or(UnquotedValue);

        internal static readonly Parser<string> Comment =
            from _h in Parse.Char('#')
            from comment in Parse.CharExcept("\r\n").Many().Text()
            select comment;

        private static readonly Parser<string> InlineWhitespace =
            Parse.Chars(" \t").Many().Text();

        private static readonly Parser<string> ExportExpression =
            from export in Parse.String("export")
                .Or(Parse.String("set -x"))
                .Or(Parse.String("set"))
                .Or(Parse.String("SET"))
                .Text()
            from _ws in Parse.Chars(" \t").AtLeastOnce()
            select export;

        internal static readonly Parser<KeyValuePair<string, string>> Assignment =
            from _ws1 in InlineWhitespace
            from export in ExportExpression.Optional()
            from name in Identifier
            from _eq in Parse.Char('=')
            from value in Value
            from _ws2 in InlineWhitespace
            from _c in Comment.Optional()
            from _lt in Parse.LineTerminator
            select new KeyValuePair<string, string>(name, value.Value);

        internal static readonly Parser<KeyValuePair<string, string>> Empty =
            from _ws in InlineWhitespace
            from _c in Comment.Optional()
            from _lt in Parse.LineTerminator
            select new KeyValuePair<string, string>(null, null);

        public static IEnumerable<KeyValuePair<string, string>> ParseDotenvFile (
            string contents,
            Func<KeyValuePair<string, string>, KeyValuePair<string, string>> tranform
        ) {
            return Assignment.Select(tranform).Or(Empty).AtLeastOnce().End()
                .Parse(contents).Where(kvp => kvp.Key != null);
        }
    }
}
