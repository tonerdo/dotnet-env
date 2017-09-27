using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load(string path, bool treatSharpAsComments = true)
        {
            Vars envFile = Parser.Parse(
                lines: File.ReadAllLines(path),
                ignoreWhiteSpace: false,
                treatSharpAsComments: treatSharpAsComments);

            LoadVars.SetEnvironmentVariables(envFile);
        }

        public static void Load(bool treatSharpAsComments = true) => Load("./.env", treatSharpAsComments);
    }
}
