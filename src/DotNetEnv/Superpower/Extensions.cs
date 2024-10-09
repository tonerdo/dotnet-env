using System;
using System.Collections.Generic;
using Superpower;
using Superpower.Model;

namespace DotNetEnv.Superpower
{
    internal static class Extensions
    {
        public static TextParser<string> Text(this TextParser<char[]> @this)
            => @this.Select(chars => new string(chars));

        public static TextParser<T[]> Repeat<T>(this TextParser<T> @this, int min, int max)
        {
            if(min < 0)
                throw new ArgumentOutOfRangeException(nameof(min));
            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(max));

            return input =>
            {
                var objectList = new List<T>();
                var textSpan = input;
                for (var i = 0; i < max; i++)
                {
                    var result = @this(textSpan);
                    if (!result.HasValue && i < min)
                        return Result.CastEmpty<T, T[]>(result);
                    if (!result.HasValue)
                        break;
                    objectList.Add(result.Value);
                    textSpan = result.Remainder;
                }
                return Result.Value(objectList.ToArray(), input, textSpan);
            };
        }
    }
}
