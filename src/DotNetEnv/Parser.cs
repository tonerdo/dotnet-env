namespace DotNetEnv
{
    internal class Parser
    {
        private static bool IsComment(string line)
        {
            return line.TrimStart(' ').StartsWith("#");
        }

        private static string RemoveInlineComment(string line)
        {
            int pos = line.IndexOf('#');
            if (pos == -1)
                return line;

            return line.Substring(0, pos);
        }

        private static string RemoveExportKeyword(string line)
        {
            line = line.TrimStart(' ');
            if (!line.StartsWith("export "))
                return line;

            return line.Substring(7);
        }

        public static Vars Parse(string[] lines, bool ignoreWhiteSpace = false)
        {
            Vars vars = new Vars();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // skip comments
                if (IsComment(line))
                    continue;

                line = RemoveInlineComment(line);
                line = RemoveExportKeyword(line);

                string[] keyValuePair = line.Split('=');

                // skip malformed lines
                if (keyValuePair.Length != 2)
                    continue;

                if (!ignoreWhiteSpace)
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
