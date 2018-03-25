using System;

namespace DotNetEnv
{
    internal class LoadVars
    {
        public static void SetEnvironmentVariables(Vars vars, bool clobberExistingVars = true)
        {
            foreach (var keyValuePair in vars)
            {
                if (clobberExistingVars || Environment.GetEnvironmentVariable(keyValuePair.Key) == null)
                    Environment.SetEnvironmentVariable(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
