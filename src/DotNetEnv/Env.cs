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

        public static IEnumerable<KeyValuePair<string, string>> LoadLines(string[] lines, LoadOptions options = null)
        {
            return Load(String.Join("\n", lines), options);
        }

        public static IEnumerable<KeyValuePair<string, string>> LoadFile(string path, LoadOptions options = null)
        {
            // in production, there should be no .env file, so this should be the common code path
            if (!File.Exists(path))
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return Load(File.ReadAllText(path), options);
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(string contents, LoadOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            if (options.ParseEnvVars)
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
            public bool ParseEnvVars { get; }
            public bool ClobberExistingVars { get; }

            public LoadOptions(
                bool parseEnvVars = true,
                bool clobberExistingVars = true
            ) {
                ParseEnvVars = parseEnvVars;
                ClobberExistingVars = clobberExistingVars;
            }
        }
    }
}
