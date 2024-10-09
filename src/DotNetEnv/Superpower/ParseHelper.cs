using Superpower;
using Superpower.Model;

namespace DotNetEnv.Superpower
{
    internal static class ParseHelper
    {
        public static TextParser<T> IsAtEnd<T>()
            => input => input.IsAtEnd ? Result.Value(default(T), input, input) : Result.Empty<T>(input, "Expected end of input");
    }
}
