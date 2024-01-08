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
        public static LoadOptions NoEnvVars(this LoadOptions @this)
        {
            @this.SetEnvVars = false;
            return @this;
        }

        public static LoadOptions NoClobber(this LoadOptions @this)
        {
            @this.ClobberExistingVars = false;
            return @this;
        }

        public static LoadOptions TraversePath(this LoadOptions @this)
        {
            @this.OnlyExactPath = false;
            return @this;
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(this LoadOptions @this, string path = null) => Env.Load(path, @this);
        public static IEnumerable<KeyValuePair<string, string>> LoadMulti(this LoadOptions @this, string[] paths) => Env.LoadMulti(paths, @this);
    }
}
