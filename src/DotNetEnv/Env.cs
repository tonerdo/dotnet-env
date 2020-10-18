using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public const string DEFAULT_ENVFILENAME = ".env";

        private static LoadOptions DEFAULT_OPTIONS = new LoadOptions();

        private static void Load(Dictionary<string, string> envFile, LoadOptions options = null)
        {
            if (envFile == null) return; // since we don't require the target file to exist, just do nothing here
            if (options == null) throw new ArgumentNullException(nameof(options)); // internal call, all callers should be populating this

            LoadVars.SetEnvironmentVariables(envFile, options.ClobberExistingVars);
        }

        public static void Load(string[] lines, LoadOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            var envFile = Parse(lines, options);
            Load(envFile, options);
        }

        public static void Load(string path, LoadOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            var envFile = Parse(path, options);
            Load(envFile, options);
        }

        public static void Load(Stream file, LoadOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            var envFile = Parse(file, options);
            Load(envFile, options);
        }

        public static void Load(LoadOptions options = null) =>
            Load(Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_ENVFILENAME), options);

        public static Dictionary<string, string> Parse(string[] lines, ParseOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            var envFile = Parser.Parse(

























                lines,
                options.TrimWhitespace,
                options.IsEmbeddedHashComment,
                options.UnescapeQuotedValues,
                options.ParseVariables
            );
            return envFile;
        }

        public static Dictionary<string, string> Parse(string path, ParseOptions options = null)
        {
            if (!File.Exists(path)) return null;
            return Parse(File.ReadAllLines(path), options);
        }

        public static Dictionary<string, string> Parse(Stream file, ParseOptions options = null)
        {
            var lines = new List<string>();
            var currentLine = "";
            using (var reader = new StreamReader(file))
            {
                while (currentLine != null)
                {
                    currentLine = reader.ReadLine();
                    if (currentLine != null) lines.Add(currentLine);
                }
            }
            return Parse(lines.ToArray(), options);
        }

        public static Dictionary<string, string> Parse(ParseOptions options = null) =>
            Parse(Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_ENVFILENAME), options);

        public static string GetString(string key, string fallback = default(string)) =>
            Environment.GetEnvironmentVariable(key) ?? fallback;

        public static bool GetBool(string key, bool fallback = default(bool)) =>
            bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static int GetInt(string key, int fallback = default(int)) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static double GetDouble(string key, double fallback = default(double)) =>
            double.TryParse(Environment.GetEnvironmentVariable(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : fallback;

        public class ParseOptions
        {
            public bool TrimWhitespace { get; }
            public bool IsEmbeddedHashComment { get; }
            public bool UnescapeQuotedValues { get; }
            public bool ParseVariables { get; }

            public ParseOptions(
                bool trimWhitespace = true,
                bool isEmbeddedHashComment = true,
                bool unescapeQuotedValues = true,
                bool parseVariables = true
            )
            {
                TrimWhitespace = trimWhitespace;
                IsEmbeddedHashComment = isEmbeddedHashComment;
                UnescapeQuotedValues = unescapeQuotedValues;
                ParseVariables = parseVariables;
            }
        }

        public class LoadOptions : ParseOptions
        {
            public bool ClobberExistingVars { get; }

            public LoadOptions(
                bool trimWhitespace = true,
                bool isEmbeddedHashComment = true,
                bool unescapeQuotedValues = true,
                bool clobberExistingVars = true,
                bool parseVariables = true
            ) : base(trimWhitespace, isEmbeddedHashComment, unescapeQuotedValues, parseVariables)
            {
                ClobberExistingVars = clobberExistingVars;
            }
        }
    }
}
