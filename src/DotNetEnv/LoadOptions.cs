using System.Collections.Generic;

namespace DotNetEnv
{
    public class LoadOptions
    {
        public bool SetEnvVars { get; set; } = true;
        public bool ClobberExistingVars { get; set; } = true;
        public bool OnlyExactPath { get; set; } = true;

        public LoadOptions(bool? setEnvVars = null, bool? clobberExistingVars = null, bool? onlyExactPath = null)
        {
            SetEnvVars = setEnvVars ?? SetEnvVars;
            ClobberExistingVars = clobberExistingVars ?? ClobberExistingVars;
            OnlyExactPath = onlyExactPath ?? OnlyExactPath;
        }

        public static LoadOptions DEFAULT => new LoadOptions();

        public static LoadOptions NoEnvVars(LoadOptions options = null) =>
            (options ?? DEFAULT).NoEnvVars();

        public static LoadOptions NoClobber(LoadOptions options = null) =>
            (options ?? DEFAULT).NoClobber();

        public static LoadOptions TraversePath(LoadOptions options = null) =>
            (options ?? DEFAULT).TraversePath();
    }

    public static class LoadOptionExtensions
    {
        public static LoadOptions NoEnvVars(this LoadOptions options)
        {
            options.SetEnvVars = false;
            return options;
        }

        public static LoadOptions NoClobber(this LoadOptions options)
        {
            options.ClobberExistingVars = false;
            return options;
        }

        public static LoadOptions TraversePath(this LoadOptions options)
        {
            options.OnlyExactPath = false;
            return options;
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(this LoadOptions options, string path = null) => Env.Load(path, options);
        public static IEnumerable<KeyValuePair<string, string>> LoadMulti(this LoadOptions options, string[] paths) => Env.LoadMulti(paths, options);
    }
}
