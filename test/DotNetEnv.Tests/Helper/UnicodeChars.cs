namespace DotNetEnv.Tests.Helper;

public struct UnicodeChars
{
    // https://stackoverflow.com/questions/602912/how-do-you-echo-a-4-digit-unicode-character-in-bash
    // printf '\xE2\x98\xA0'
    // printf ☠ | hexdump  # hexdump has bytes flipped per word (2 bytes, 4 hex)

    public const string Rocket = "\ud83d\ude80"; // 🚀
}
