namespace DotNetEnv
{
    internal class Parser
    {
        private static bool IsComment(string line)
        {
            return line.TrimStart(' ').StartsWith("#");
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
