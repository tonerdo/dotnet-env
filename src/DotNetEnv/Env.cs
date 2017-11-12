using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load(string path, bool trimWhitespace = true, bool isEmbeddedHashComment = true)
        {
            Vars envFile = Parser.Parse(File.ReadAllLines(path), trimWhitespace, isEmbeddedHashComment);
            LoadVars.SetEnvironmentVariables(envFile);
        }

        public static void Load(bool trimWhitespace = true, bool isEmbeddedHashComment = true)
            => Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"), trimWhitespace, isEmbeddedHashComment);
    }
}
