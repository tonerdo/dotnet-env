namespace DotNetEnv.Extensions
{
    public enum CreateDictionaryOption
    {
        /// <summary>
        /// Default behaviour, throws on duplicates.
        /// </summary>
        Default,

        /// <summary>
        /// Takes the first occurrence for duplicates.
        /// </summary>
        TakeFirst,

        /// <summary>
        /// Takes the last occurrence for duplicates.
        /// </summary>
        TakeLast,
    }
}