using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotNetEnv.Superpower;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace DotNetEnv
{
    class Parsers
    {
        public static ConcurrentDictionary<string, string> ActualValuesSnapshot = new ConcurrentDictionary<string, string>();

        // helpful blog I discovered only after digging through all the Sprache source myself:
        // https://justinpealing.me.uk/post/2020-03-11-sprache1-chars/

        private static readonly TextParser<TextSpan> LineTerminator = Span.EqualTo("\r\n").Or(Span.EqualTo("\n")).Or(ParseHelper.IsAtEnd<TextSpan>());
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

        private static readonly TextParser<char> Octal = Character.In("01234567".ToCharArray());
        private static readonly TextParser<char> Hex = Character.In("0123456789abcdefABCDEF".ToCharArray());

        internal static readonly TextParser<byte> HexByte =
            (from start in Span.EqualTo("\\x")
            from value in Hex.Repeat(1, 2).Text()
            select ToHexByte(value)).Try();

        internal static readonly TextParser<byte> OctalByte =
            (from _ in Backslash
            from value in Octal.Repeat(1, 3).Text()
            select ToOctalByte(value)).Try();

        internal static readonly TextParser<string> OctalChar =
            (from value in OctalByte.Repeat(1, 8)
            select ToUtf8Char(value)).Try();

        internal static readonly TextParser<string> Utf8Char =
            (from firstByte in HexByte
                from nextBytes in HexByte.Repeat(GetUtf8CharByteCount(firstByte) - 1)
                select ToUtf8Char(new[] { firstByte }.Concat(nextBytes))).Try();

        /// <summary>
        /// Returns byte-count of a UTF-8 character by its first byte.
        /// </summary>
        /// <param name="firstByte">The first byte of the UTF-8 char</param>
        /// <returns>the byte-count of a UTF-8 char.</returns>
        /// <remarks>https://en.wikipedia.org/wiki/UTF-8#Description</remarks>
        private static int GetUtf8CharByteCount(byte firstByte)
            => firstByte < (byte)'\x80' ? 1
                : firstByte < (byte)'\xE0' ? 2
                : firstByte < (byte)'\xF0' ? 3
                : 4;

        internal static readonly TextParser<string> Utf16Char =
            (from start in Span.EqualTo("\\u")
            from value in Hex.Repeat(2, 4).Text()
            select ToUtf16Char(value)).Try();

        internal static readonly TextParser<string> Utf32Char =
            (from start in Span.EqualTo("\\U")
                from value in Hex.Repeat(2, 8).Text()
                select ToUtf32Char(value)).Try();

        internal static TextParser<string> NotControlNorWhitespace (string exceptChars) =>
            Character.Matching(
                c => !char.IsControl(c) && !char.IsWhiteSpace(c) && !exceptChars.Contains(c),
                $"not control nor whitespace nor {exceptChars}"
            ).AtLeastOnce().Text();
        
        // officially *nix env vars can only be /[a-zA-Z_][a-zA-Z_0-9]*/
        // but because technically you can set env vars that are basically anything except equals signs, allow some flexibility
        private static readonly TextParser<char> IdentifierSpecialChars = Character.In('.', '-');
        internal static readonly TextParser<string> Identifier =
            from head in Character.Letter.Or(Underscore)
            from tail in Character.LetterOrDigit.Or(Underscore).Or(IdentifierSpecialChars).Many().Text()
            select head + tail;

        internal static readonly TextParser<IValue> InterpolatedEnvVar =
            (from _d in DollarSign
                from id in Identifier
                select new ValueInterpolated(id) as IValue).Try();

        internal static readonly TextParser<IValue> InterpolatedBracesEnvVar =
            (from _d in DollarSign
                from _o in Character.EqualTo('{')
                from id in Identifier
                from _c in Character.EqualTo('}')
                select new ValueInterpolated(id) as IValue).Try();

        internal static readonly TextParser<IValue> JustDollarValue =
            (from d in DollarSign
                select new ValueActual(d.ToString()) as IValue).Try();

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
                .Or((from inlineWhitespaces in InlineWhitespace
                    from _ in
                        Parse.Not(Character
                            .EqualTo('#')) // "#" after a whitespace is the beginning of a comment --> not allowed
                    from partOfValue in
                        NotControlNorWhitespace(
                            "$\"'") // quotes are not allowed in values, because in a shell they mean something different
                    select new ValueActual(string.Concat(inlineWhitespaces, partOfValue)) as IValue).Try())
                .Many()
                .Select(vs => new ValueCalculator(vs));

        internal static readonly TextParser<ValueCalculator> UnquotedValue =
            from _ in Parse.Not(Character.In(" \t\"'".ToCharArray()))
            from value in UnquotedValueContents
            select value;

        // double quoted values can have everything: interpolated variables,
        // plus whitespace, escaped chars, and byte code chars
        internal static readonly TextParser<ValueCalculator> DoubleQuotedValueContents =
            InterpolatedValue.Or(
                SpecialChar
                .Or(NotControlNorWhitespace("\"\\$"))
                .Or(Character.WhiteSpace.AtLeastOnce().Text())
                .AtLeastOnce()
                .Select(strs => new ValueActual(strs) as IValue)
            ).Many().Select(vs => new ValueCalculator(vs));

        // single quoted values can have whitespace,
        // but no interpolation, no escaped chars, no byte code chars
        // notably no single quotes inside either -- no escaping!
        // single quotes are for when you want truly raw values
        internal static readonly TextParser<ValueCalculator> SingleQuotedValueContents =
            NotControlNorWhitespace("'")
                .Or(Character.WhiteSpace.AtLeastOnce().Text())
                .AtLeastOnce()
                .Select(strs => new ValueActual(strs))
                .Many()
                .Select(vs => new ValueCalculator(vs));

        // compare against bash quoting rules:
        // https://stackoverflow.com/questions/6697753/difference-between-single-and-double-quotes-in-bash/42082956#42082956

        internal static readonly TextParser<ValueCalculator> SingleQuotedValue =
            from _o in Character.EqualTo('\'')
            from value in SingleQuotedValueContents
            from _c in Character.EqualTo('\'')
            select value;

        internal static readonly TextParser<ValueCalculator> DoubleQuotedValue =
            from _o in Character.EqualTo('"')
            from value in DoubleQuotedValueContents
            from _c in Character.EqualTo('"')
            select value;

        internal static readonly TextParser<ValueCalculator> Value =
            SingleQuotedValue.Or(DoubleQuotedValue).Or(UnquotedValue);

        internal static readonly TextParser<string> Comment =
            from _h in Character.EqualTo('#')
            from comment in Character.ExceptIn('\r', '\n').Many().Text()
            select comment;

        private static readonly TextParser<TextSpan> ExportIdentifier =
            Span.EqualTo("export")
                .Or(Span.EqualTo("set -x").Try())
                .Or(Span.EqualTo("set"))
                .Or(Span.EqualTo("SET"));

        internal static readonly TextParser<string> ExportExpression =
            (from export in ExportIdentifier
                from _ws in InlineWhitespaceChars.AtLeastOnce()
                select export.ToStringValue()).Try();

        internal static readonly TextParser<KeyValuePair<string, string>> Assignment =
            (from _ws_head in InlineWhitespace
                from export in ExportExpression.OptionalOrDefault()
                from name in Identifier
                from _ws_pre in InlineWhitespace
                from _eq in Character.EqualTo('=')
                from _ws_post in InlineWhitespace
                from value in Value
                from _ws_tail in InlineWhitespace
                from _c in Comment.OptionalOrDefault()
                from _lt in LineTerminator
                select new KeyValuePair<string, string>(name, value.Value)).Try();

        internal static readonly TextParser<KeyValuePair<string, string>> Empty =
            Parse.Not(ParseHelper.IsAtEnd<KeyValuePair<string, string>>()).Then(_ =>
                from _ws in InlineWhitespace
                from _c in Comment.OptionalOrDefault()
                from _lt in LineTerminator
                select new KeyValuePair<string, string>(null, null));

        public static IEnumerable<KeyValuePair<string, string>> ParseDotenvFile(string contents,
            bool clobberExistingVariables = true, IDictionary<string, string> actualValues = null)
        {
            ActualValuesSnapshot = new ConcurrentDictionary<string, string>(actualValues ?? new Dictionary<string, string>());

            return Assignment.Select(UpdateEnvVarSnapshot).Or(Empty)
                .Many()
                .AtEnd()
                .Parse(contents)
                .Where(kvp => kvp.Key != null);

            KeyValuePair<string, string> UpdateEnvVarSnapshot(KeyValuePair<string, string> pair)
            {
                if (clobberExistingVariables || !ActualValuesSnapshot.ContainsKey(pair.Key))
                    ActualValuesSnapshot.AddOrUpdate(pair.Key, pair.Value, (key, oldValue) => pair.Value);

                return pair;
            }
        }
    }
}
