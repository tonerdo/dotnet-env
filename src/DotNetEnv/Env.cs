using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotNetEnv.Extensions;

namespace DotNetEnv
{
    public class Env
    {
        public const string DEFAULT_ENVFILENAME = ".env";

        public static IEnumerable<KeyValuePair<string, string>> LoadMulti(string[] paths, LoadOptions options = null)
        {
            return paths.Aggregate(
                Array.Empty<KeyValuePair<string, string>>(),
                (kvps, path) => kvps.Concat(Load(path, options, kvps)).ToArray()
            );
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(string path = null, LoadOptions options = null)
            => Load(path, options, null);

        private static IEnumerable<KeyValuePair<string, string>> Load(string path, LoadOptions options,
            IEnumerable<KeyValuePair<string, string>> actualValues)
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

            return LoadContents(File.ReadAllText(path), options, actualValues);
        }

        public static IEnumerable<KeyValuePair<string, string>> Load(Stream file, LoadOptions options = null)
        {
            using (var reader = new StreamReader(file))
            {
                return LoadContents(reader.ReadToEnd(), options);
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> LoadContents(string contents,
            LoadOptions options = null)
            => LoadContents(contents, options, null);

        private static IEnumerable<KeyValuePair<string, string>> LoadContents(string contents,
            LoadOptions options, IEnumerable<KeyValuePair<string, string>> actualValues)
        {
            if (options == null)
                options = LoadOptions.DEFAULT;

            var dictionaryOption = options.ClobberExistingVars
                ? CreateDictionaryOption.TakeLast
                : CreateDictionaryOption.TakeFirst;

            var previousValueDictionary = actualValues?.ToDotEnvDictionary(dictionaryOption);
            var actualValueProvider = previousValueDictionary == null
                ? (IValueProvider)new EnvironmentValueProvider()
                : new ChainedValueProvider(options.ClobberExistingVars,
                    new EnvironmentValueProvider(), new DictionaryValueProvider(previousValueDictionary));

            var pairs = Parsers.ParseDotenvFile(contents, options.ClobberExistingVars, actualValueProvider);

            var unClobberedPairs = (options.ClobberExistingVars
                    ? pairs
                    : pairs.Where(p => !actualValueProvider.TryGetValue(p.Key, out _)))
                .ToArray();

            if (options.SetEnvVars)
                foreach (var pair in unClobberedPairs)
                    Environment.SetEnvironmentVariable(pair.Key, pair.Value);

            return unClobberedPairs.ToDotEnvDictionary(dictionaryOption);
        }

        public static string GetString(string key, string fallback = default(string)) =>
            Environment.GetEnvironmentVariable(key) ?? fallback;

        public static bool GetBool(string key, bool fallback = default(bool)) =>
            bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static int GetInt(string key, int fallback = default(int)) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static double GetDouble(string key, double fallback = default(double)) =>
            double.TryParse(Environment.GetEnvironmentVariable(key), NumberStyles.Any, CultureInfo.InvariantCulture,
                out var value)
                ? value
                : fallback;

        public static LoadOptions NoEnvVars() => LoadOptions.NoEnvVars();
        public static LoadOptions NoClobber() => LoadOptions.NoClobber();
        public static LoadOptions TraversePath() => LoadOptions.TraversePath();
    }
}
