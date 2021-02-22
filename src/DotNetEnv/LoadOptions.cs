using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotNetEnv
{
    public class LoadOptions
    {
        public static readonly LoadOptions DEFAULT = new LoadOptions();

        public bool SetEnvVars { get; }
        public bool ClobberExistingVars { get; }
        public bool OnlyExactPath { get; }

        public LoadOptions(
            bool setEnvVars = true,
            bool clobberExistingVars = true,
            bool onlyExactPath = true
        ) {
            SetEnvVars = setEnvVars;
            ClobberExistingVars = clobberExistingVars;
            OnlyExactPath = onlyExactPath;
        }

        public LoadOptions(
            LoadOptions old,
            bool? setEnvVars = null,
            bool? clobberExistingVars = null,
            bool? onlyExactPath = null
        ) {
            SetEnvVars = setEnvVars ?? old.SetEnvVars;
            ClobberExistingVars = clobberExistingVars ?? old.ClobberExistingVars;
            OnlyExactPath = onlyExactPath ?? old.OnlyExactPath;
        }

        public static LoadOptions NoEnvVars (LoadOptions options = null) =>
            options == null ? DEFAULT.NoEnvVars() : options.NoEnvVars();

        public static LoadOptions NoClobber (LoadOptions options = null) =>
            options == null ? DEFAULT.NoClobber() : options.NoClobber();

        public static LoadOptions TraversePath (LoadOptions options = null) =>
            options == null ? DEFAULT.TraversePath() : options.TraversePath();

        public LoadOptions NoEnvVars () => new LoadOptions(this, setEnvVars: false);
        public LoadOptions NoClobber () => new LoadOptions(this, clobberExistingVars: false);
        public LoadOptions TraversePath () => new LoadOptions(this, onlyExactPath: false);

        public IEnumerable<KeyValuePair<string, string>> Load (string path = null) => Env.Load(path, this);
        public IEnumerable<KeyValuePair<string, string>> LoadMulti (string[] paths) => Env.LoadMulti(paths, this);
    }
}
