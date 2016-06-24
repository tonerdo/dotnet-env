using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public static void Load()
        {
            string path = "./.env";
            Vars envFile = Parser.Parse(File.ReadAllLines(path));
            LoadVars.SetEnvironmentVariables(envFile);
        }
    }
}
