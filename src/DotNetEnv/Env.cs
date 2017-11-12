using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load(string path, bool keepWhiteSpace = false)
        {
            Vars envFile = Parser.Parse(File.ReadAllLines(path), keepWhiteSpace);
            LoadVars.SetEnvironmentVariables(envFile);
        }

        public static void Load(bool keepWhiteSpace = false)
            => Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"), keepWhiteSpace);
    }
}
