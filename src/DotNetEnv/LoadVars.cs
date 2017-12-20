using System;
using System.Collections;

namespace DotNetEnv
{
    internal class LoadVars
    {
        public static void SetEnvironmentVariables(Vars vars, bool clobberExistingVars = true)
        {
            IDictionary currentVars = null;
            if (!clobberExistingVars)
                currentVars = Environment.GetEnvironmentVariables();
                
            foreach (var keyValuePair in vars)
            {
                if (clobberExistingVars || !currentVars.Contains(keyValuePair.Key))
                    Environment.SetEnvironmentVariable(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
