using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotNetEnv
{
    public class Env
    {
        public const string DEFAULT_ENVFILENAME = ".env";

        private static LoadOptions DEFAULT_OPTIONS = new LoadOptions();

        public static Dictionary<string, string> ToDictionary(IEnumerable<KeyValuePair<string, string>> kvps)
        {
            // Distinct does not work on this kind of data -- keys are same but not values, whole point
//            return kvps.Distinct().ToDictionary(kv => kv.Key, kv => kv.Value);
            return kvps.GroupBy(kv => kv.Key).ToDictionary(g => g.Key, g => g.Last().Value);
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(string[] lines, LoadOptions options = null)
        {
            return LoadContents(String.Join("\n", lines), options);
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(string path, LoadOptions options = null)
        {
            // in production, there should be no .env file, so this should be the common code path
            if (!File.Exists(path))
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return LoadContents(File.ReadAllText(path), options);
        }

        public static IEnumerable<KeyValuePair<string, string>> LoadContents(string contents, LoadOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            // TODO: actually inject this into a new Parser instance, instead of all static
            // ... but to do this, I need to use a different syntax than the LINQ for Sprache
            // so it's a big undertaking and I'm waiting on more details to
            // https://github.com/tonerdo/dotnet-env/issues/40
            // SO: THIS OBJECT IS NOT USED, ON PURPOSE, FOR NOW
            var valueFactory = options.ParseVariables
                ? (IValueFactory) new InterpolatedValueFactory()
                : (IValueFactory) new ActualValueFactory();

            // the above refactoring for injecting into a Parser instance
            // would also potentially include passing the "transform" functions below into the ctor
            // with a factory to do this work of checking options and building the right thing, or something
            if (options.SetEnvVars)
            {
                if (options.ClobberExistingVars)
                {
                    return Parsers.ParseDotenvFile(contents, Parsers.SetEnvVar);
                }
                else
                {
                    return Parsers.ParseDotenvFile(contents, Parsers.NoClobberSetEnvVar);
                }
            }
            else
            {
                return Parsers.ParseDotenvFile(contents, Parsers.DoNotSetEnvVar);
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(LoadOptions options = null) =>
            Load(Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_ENVFILENAME), options);

        public static string GetString(string key, string fallback = default(string)) =>
            Environment.GetEnvironmentVariable(key) ?? fallback;

        public static bool GetBool(string key, bool fallback = default(bool)) =>
            bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static int GetInt(string key, int fallback = default(int)) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static double GetDouble(string key, double fallback = default(double)) =>
            double.TryParse(Environment.GetEnvironmentVariable(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : fallback;

        public class LoadOptions
        {
            public bool SetEnvVars { get; }
            public bool ClobberExistingVars { get; }
            public bool ParseVariables { get; }

            public LoadOptions(
                bool setEnvVars = true,
                bool clobberExistingVars = true,
                bool parseVariables = true
            ) {
                SetEnvVars = setEnvVars;
                ClobberExistingVars = clobberExistingVars;
                ParseVariables = parseVariables;
            }
        }
    }
}
