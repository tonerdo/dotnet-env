using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public const string DEFAULT_ENVFILENAME = ".env";

        public static IEnumerable<KeyValuePair<string, string>> LoadMulti (string[] paths, LoadOptions options = null)
        {
            return paths.Aggregate(
                Enumerable.Empty<KeyValuePair<string, string>>(),
                (kvps, path) => kvps.Concat(Load(path, options))
            );
        }

        public static IEnumerable<KeyValuePair<string, string>> Load (string path = null, LoadOptions options = null)
        {
            if (options == null) options = LoadOptions.DEFAULT;

            var file = Path.GetFileName(path);
            if (file == null || file == string.Empty) file = DEFAULT_ENVFILENAME;
            var dir = Path.GetDirectoryName(path);
            if (dir == null || dir == string.Empty) dir = Directory.GetCurrentDirectory();
            path = Path.Combine(dir, file);

            if (options.OnlyExactPath)
            {
                if (!File.Exists(path)) path = null;
            }
            else
            {
                while (!File.Exists(path))
                {
                    var parent = Directory.GetParent(dir);
                    if (parent == null)
                    {
                        path = null;
                        break;
                    }
                    dir = parent.FullName;
                    path = Path.Combine(dir, file);
                }
            }

            // in production, there should be no .env file, so this should be the common code path
            if (path == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return LoadContents(File.ReadAllText(path), options);
        }

        public static IEnumerable<KeyValuePair<string, string>> Load (Stream file, LoadOptions options = null)
        {
            using (var reader = new StreamReader(file))
            {
                return LoadContents(reader.ReadToEnd(), options);
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> LoadContents(string contents,
            LoadOptions options = null)
        {
            if (options == null) options = LoadOptions.DEFAULT;

            var envVarSnapshot = new Dictionary<string, string>(Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString()));
            Parsers.EnvVarSnapshot = new ConcurrentDictionary<string, string>(envVarSnapshot);

            var pairs = Parsers.ParseDotenvFile(contents, options.ClobberExistingVars).ToList();

            if (options.SetEnvVars)
                SetEnvVars(pairs, options.ClobberExistingVars);

            if (options.ClobberExistingVars)
                return pairs;

            // prepend the pairs with all EnvVars with keys, which are present in pairs
            // when taking first elements (noClobber) you get the EnvVars first, but all values from Env are still present in the result
            // one could argue, that at this place we should return a dictionary instead; if someone needs "raw" values he can use ParseDotenvFile directly;
            return pairs
                .Join(envVarSnapshot
                    , pair => pair.Key
                    , envVar => envVar.Key
                    , (pair, envVar) => envVar)
                .Concat(pairs);
        }

        private static void SetEnvVars(List<KeyValuePair<string, string>> pairs, bool clobberExistingVars)
        {
            foreach (var pair in pairs)
                if (clobberExistingVars || Environment.GetEnvironmentVariable(pair.Key) == null)
                    Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }

        public static string GetString (string key, string fallback = default(string)) =>
            Environment.GetEnvironmentVariable(key) ?? fallback;

        public static bool GetBool (string key, bool fallback = default(bool)) =>
            bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static int GetInt (string key, int fallback = default(int)) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static double GetDouble (string key, double fallback = default(double)) =>
            double.TryParse(Environment.GetEnvironmentVariable(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : fallback;

        public static LoadOptions NoEnvVars () => LoadOptions.NoEnvVars();
        public static LoadOptions NoClobber () => LoadOptions.NoClobber();
        public static LoadOptions TraversePath () => LoadOptions.TraversePath();
    }
}
