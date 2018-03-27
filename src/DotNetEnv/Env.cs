using System.Collections.Generic;
using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load(
            string[] lines,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        {
            Vars envFile = Parser.Parse(
                lines,
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues
            );
            LoadVars.SetEnvironmentVariables(envFile, clobberExistingVars);
        }
        
        public static void Load(
            string path,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        => Load(
            File.ReadAllLines(path),
            trimWhitespace,
            isEmbeddedHashComment,
            unescapeQuotedValues,
            clobberExistingVars
        );

        public static void Load(
            Stream file,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        {
            var lines = new List<string>();
            var currentLine = "";
            using (var reader = new StreamReader(file))
            {
                while (currentLine != null)
                {
                    currentLine = reader.ReadLine();
                    if (currentLine != null) lines.Add(currentLine);
                }
            }
            Load(
                lines.ToArray(),
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues,
                clobberExistingVars
            );
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
