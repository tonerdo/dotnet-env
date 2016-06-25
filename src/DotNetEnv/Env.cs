using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load(string path)
        {
            Vars envFile = Parser.Parse(File.ReadAllLines(path));
            LoadVars.SetEnvironmentVariables(envFile);
        }

        public static void Load() => Load("./.env");
    }
}
