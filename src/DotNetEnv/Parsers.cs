using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Superpower;
using Superpower.Parsers;

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
            Env.FakeEnvVars.AddOrUpdate(kvp.Key, kvp.Value, (_, v) => v);
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

        private static readonly TextParser<char> DollarSign = Character.EqualTo('$');
        private static readonly TextParser<char> Backslash = Character.EqualTo('\\');
        private static readonly TextParser<char> Underscore = Character.EqualTo('_');
        private static readonly TextParser<char> InlineWhitespaceChars = Character.In(' ', '\t');

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
        internal static readonly TextParser<string> EscapedChar =
            from _ in Backslash
            from c in Character.AnyChar
            select ToEscapeChar(c);

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

        private static readonly TextParser<char> Hex = Parse.Chars("0123456789abcdefABCDEF");

        internal static readonly TextParser<byte> HexByte =
            from start in Parse.String("\\x")
            from value in Parse.Repeat(Hex, 1, 2).Text()
            select ToHexByte(value);

        internal static readonly TextParser<byte> OctalByte =
            from _ in Backslash
            from value in Parse.Repeat(Parse.Chars("01234567"), 1, 3).Text()
            select ToOctalByte(value);

        internal static readonly TextParser<string> OctalChar =
            from value in Parse.Repeat(OctalByte, 1, 8)
            select ToUtf8Char(value);

        internal static readonly TextParser<string> Utf8Char =
            from value in Parse.Repeat(HexByte, 1, 4)
            select ToUtf8Char(value);

        internal static readonly TextParser<string> Utf16Char =
            from start in Parse.String("\\u")
            from value in Parse.Repeat(Hex, 2, 4).Text()
            select ToUtf16Char(value);

        internal static readonly TextParser<string> Utf32Char =
            from start in Parse.String("\\U")
            from value in Parse.Repeat(Hex, 2, 8).Text()
            select ToUtf32Char(value);

        internal static TextParser<string> NotControlNorWhitespace (string exceptChars) =>
            Parse.Char(
                c => !char.IsControl(c) && !char.IsWhiteSpace(c) && !exceptChars.Contains(c),
                $"not control nor whitespace nor {exceptChars}"
            ).AtLeastOnce().Text();


        // officially *nix env vars can only be /[a-zA-Z_][a-zA-Z_0-9]*/
        // but because technically you can set env vars that are basically anything except equals signs, allow some flexibility
        private static readonly TextParser<char> IdentifierSpecialChars = Parse.Chars(".-");
        internal static readonly TextParser<string> Identifier =
            from head in Parse.Letter.Or(Underscore)
            from tail in Parse.LetterOrDigit.Or(Underscore).Or(IdentifierSpecialChars).Many().Text()
            select head + tail;

        internal static readonly TextParser<IValue> InterpolatedEnvVar =
            from _d in DollarSign
            from id in Identifier
            select new ValueInterpolated(id);

        internal static readonly TextParser<IValue> InterpolatedBracesEnvVar =
            from _d in DollarSign
            from _o in Parse.Char('{')
            from id in Identifier
            from _c in Parse.Char('}')
            select new ValueInterpolated(id);

        internal static readonly TextParser<IValue> JustDollarValue =
            from d in DollarSign
            select new ValueActual(d.ToString());

        internal static readonly TextParser<IValue> InterpolatedValue =
            InterpolatedEnvVar.Or(InterpolatedBracesEnvVar).Or(JustDollarValue);

        internal static readonly TextParser<string> SpecialChar =
            Utf32Char
                .Or(Utf16Char)
                .Or(Utf8Char)
                .Or(OctalChar)
                .Or(EscapedChar);

        private static readonly TextParser<string> InlineWhitespace =
            InlineWhitespaceChars.Many().Text();

        // unquoted values can have interpolated variables,
        // but only inline whitespace -- until a comment,
        // and no escaped chars, nor byte code chars
        private static readonly TextParser<ValueCalculator> UnquotedValueContents =
            InterpolatedValue
                .Or(from inlineWhitespaces in InlineWhitespace
                    from _ in Parse.Char('#').Not() // "#" after a whitespace is the beginning of a comment --> not allowed
                    from partOfValue in NotControlNorWhitespace("$\"'") // quotes are not allowed in values, because in a shell they mean something different
                    select new ValueActual(string.Concat(inlineWhitespaces, partOfValue)))
                .Many()
                .Select(vs => new ValueCalculator(vs));

        internal static readonly TextParser<ValueCalculator> UnquotedValue =
            from _ in Parse.Chars(" \t\"'").Not()
            from value in UnquotedValueContents
            select value;

        // double quoted values can have everything: interpolated variables,
        // plus whitespace, escaped chars, and byte code chars
        internal static readonly TextParser<ValueCalculator> DoubleQuotedValueContents =
            InterpolatedValue.Or(
                SpecialChar
                .Or(NotControlNorWhitespace("\"\\$"))
                .Or(Parse.WhiteSpace.AtLeastOnce().Text())
                .AtLeastOnce()
                .Select(strs => new ValueActual(strs))
            ).Many().Select(vs => new ValueCalculator(vs));

        // single quoted values can have whitespace,
        // but no interpolation, no escaped chars, no byte code chars
        // notably no single quotes inside either -- no escaping!
        // single quotes are for when you want truly raw values
        internal static readonly TextParser<ValueCalculator> SingleQuotedValueContents =
            NotControlNorWhitespace("'")
                .Or(Parse.WhiteSpace.AtLeastOnce().Text())
                .AtLeastOnce()
                .Select(strs => new ValueActual(strs))
                .Many()
                .Select(vs => new ValueCalculator(vs));

        // compare against bash quoting rules:
        // https://stackoverflow.com/questions/6697753/difference-between-single-and-double-quotes-in-bash/42082956#42082956

        internal static readonly TextParser<ValueCalculator> SingleQuotedValue =
            from _o in Parse.Char('\'')
            from value in SingleQuotedValueContents
            from _c in Parse.Char('\'')
            select value;

        internal static readonly TextParser<ValueCalculator> DoubleQuotedValue =
            from _o in Parse.Char('"')
            from value in DoubleQuotedValueContents
            from _c in Parse.Char('"')
            select value;

        internal static readonly TextParser<ValueCalculator> Value =
            SingleQuotedValue.Or(DoubleQuotedValue).Or(UnquotedValue);

        internal static readonly TextParser<string> Comment =
            from _h in Parse.Char('#')
            from comment in Parse.CharExcept("\r\n").Many().Text()
            select comment;

        private static readonly TextParser<string> ExportExpression =
            from export in Parse.String("export")
                .Or(Parse.String("set -x"))
                .Or(Parse.String("set"))
                .Or(Parse.String("SET"))
                .Text()
            from _ws in InlineWhitespaceChars.AtLeastOnce()
            select export;

        internal static readonly TextParser<KeyValuePair<string, string>> Assignment =
            from _ws_head in InlineWhitespace
            from export in ExportExpression.Optional()
            from name in Identifier
            from _ws_pre in InlineWhitespace
            from _eq in Parse.Char('=')
            from _ws_post in InlineWhitespace
            from value in Value
            from _ws_tail in InlineWhitespace
            from _c in Comment.Optional()
            from _lt in Parse.LineTerminator
            select new KeyValuePair<string, string>(name, value.Value);

        internal static readonly TextParser<KeyValuePair<string, string>> Empty =
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
