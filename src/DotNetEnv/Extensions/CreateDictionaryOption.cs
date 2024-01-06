namespace DotNetEnv.Extensions
{
    public enum CreateDictionaryOption
    {
        /// <summary>
        /// Throws on duplicates.
        /// </summary>
        Throw,

        /// <summary>
        /// Takes the first occurrence on duplicates instead of throwing.
        /// </summary>
        TakeFirst,

        /// <summary>
        /// Takes the last occurrence on duplicates instead of throwing.
        /// </summary>
        TakeLast,
    }
}
