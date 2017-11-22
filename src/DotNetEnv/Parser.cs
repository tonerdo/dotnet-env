using System.Text.RegularExpressions;

namespace DotNetEnv
{
    internal class Parser
    {
        private static Regex ExportRegex = new Regex("^\\s*export\\s+");

        private static bool IsComment(string line)
        {
            return line.Trim().StartsWith("#");
        }

        private static string RemoveInlineComment(string line)
        {
            int pos = line.IndexOf('#');
            return pos >= 0 ? line.Substring(0, pos) : line;
        }

        private static string RemoveExportKeyword(string line)
        {
            Match match = ExportRegex.Match(line);
            return match.Success ? line.Substring(match.Length) : line;
        }

        public static Vars Parse(string[] lines, bool trimWhitespace = true, bool isEmbeddedHashComment = true)
        {
            Vars vars = new Vars();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // skip comments
                if (IsComment(line))
                    continue;

                if (isEmbeddedHashComment)
                {
                    line = RemoveInlineComment(line);
                }

                line = RemoveExportKeyword(line);

                string[] keyValuePair = line.Split(new char[] { '=' }, 2);

                // skip malformed lines
                if (keyValuePair.Length != 2)
                    continue;

                if (trimWhitespace)
                {
                    keyValuePair[0] = keyValuePair[0].Trim();
                    keyValuePair[1] = keyValuePair[1].Trim();
                }

                vars.Add(
                    keyValuePair[0],
                    keyValuePair[1]
                );
            }

            return vars;
        }
    }
}
