using System;

namespace DotNetEnv
{
    internal class LoadVars
    {
        public static void SetEnvironmentVariables(Vars vars)
        {
            foreach (var keyValuePair in vars)
            {
                Environment.SetEnvironmentVariable(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
