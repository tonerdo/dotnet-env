using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public const string DEFAULT_ENVFILENAME = ".env";

        public static void Load(string[] lines, LoadOptions options)
        {
            Vars envFile = Parser.Parse(
                lines,
                options.TrimWhitespace,
                options.IsEmbeddedHashComment,
                options.UnescapeQuotedValues
            );
            LoadVars.SetEnvironmentVariables(envFile, options.ClobberExistingVars);
        }

        public static void Load(string path, LoadOptions options)
        {
            if (!options.RequireEnvFile && !File.Exists(path)) return;
            Load(File.ReadAllLines(path), options);
        }

        public static void Load(Stream file, LoadOptions options)
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
            Load(lines.ToArray(), options);
        }

        public static void Load(LoadOptions options)
        => Load(Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_ENVFILENAME), options);

        public static string GetString(string key, string fallback = default(string)) =>
            Environment.GetEnvironmentVariable(key) ?? fallback;

        public static bool GetBool(string key, bool fallback = default(bool)) =>
            bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static int GetInt(string key, int fallback = default(int)) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

        public static double GetDouble(string key, double fallback = default(double)) =>
            double.TryParse(Environment.GetEnvironmentVariable(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : fallback;

        #region "Obsolete parameters list"

        [Obsolete("list of flag arguments is deprecated, use the options object")]
        public static void Load(
            string[] lines,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        => Load(
            lines,
            new LoadOptions(
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues,
                clobberExistingVars
            )
        );
        
        [Obsolete("list of flag arguments is deprecated, use the options object")]
        public static void Load(
            string path,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        => Load(
            path,
            new LoadOptions(
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues,
                clobberExistingVars
            )
        );

        [Obsolete("list of flag arguments is deprecated, use the options object")]
        public static void Load(
            Stream file,
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        => Load(
            file,
            new LoadOptions(
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues,
                clobberExistingVars
            )
        );

        [Obsolete("list of flag arguments is deprecated, use the options object")]
        public static void Load(
            bool trimWhitespace = true,
            bool isEmbeddedHashComment = true,
            bool unescapeQuotedValues = true,
            bool clobberExistingVars = true
        )
        => Load(
            new LoadOptions(
                trimWhitespace,
                isEmbeddedHashComment,
                unescapeQuotedValues,
                clobberExistingVars
            )
        );

        #endregion

        public class LoadOptions
        {
            public bool TrimWhitespace { get; }
            public bool IsEmbeddedHashComment { get; }
            public bool UnescapeQuotedValues { get; }
            public bool ClobberExistingVars { get; }
            public bool RequireEnvFile { get; }

            public LoadOptions(
                bool trimWhitespace = true,
                bool isEmbeddedHashComment = true,
                bool unescapeQuotedValues = true,
                bool clobberExistingVars = true,
                bool requireEnvFile = true
            )
            {
                TrimWhitespace = trimWhitespace;
                IsEmbeddedHashComment = isEmbeddedHashComment;
                UnescapeQuotedValues = unescapeQuotedValues;
                ClobberExistingVars = clobberExistingVars;
                RequireEnvFile = requireEnvFile;
            }
        }
    }
}
