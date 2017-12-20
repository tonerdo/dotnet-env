using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load(
            string path,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        {
            Vars envFile = Parser.Parse(
                File.ReadAllLines(path),
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues
            );
            LoadVars.SetEnvironmentVariables(envFile, clobberExistingVars);
        }

        public static void Load(
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        => Load(
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            trimWhitespace,
            isEmbeddedHashComment,
            unescapeQuotedValues,
            clobberExistingVars
        );
    }
}
