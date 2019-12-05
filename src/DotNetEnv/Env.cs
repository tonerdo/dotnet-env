﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DotNetEnv
{
    public class Env
    {
        public const string DEFAULT_ENVFILENAME = ".env";

        private static LoadOptions DEFAULT_OPTIONS = new LoadOptions();

        public static void Load(string[] lines, LoadOptions options = null)
        {
            if (options == null) options = DEFAULT_OPTIONS;

            Vars envFile = Parser.Parse(
                lines,
                options.TrimWhitespace,
                options.IsEmbeddedHashComment,
                options.UnescapeQuotedValues,
                options.ParseVariables
            );
            LoadVars.SetEnvironmentVariables(envFile, options.ClobberExistingVars);
        }

        public static void Load(string path, LoadOptions options = null)
        {
            if (!File.Exists(path)) return;
            Load(File.ReadAllLines(path), options);
        }

        public static void Load(Stream file, LoadOptions options = null)
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

        public static void Load(LoadOptions options = null) =>
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
            public bool TrimWhitespace { get; }
            public bool IsEmbeddedHashComment { get; }
            public bool UnescapeQuotedValues { get; }
            public bool ClobberExistingVars { get; }
            public bool ParseVariables { get; }

            public LoadOptions(
                bool trimWhitespace          = true,
                bool isEmbeddedHashComment   = true,
                bool unescapeQuotedValues    = true,
                bool clobberExistingVars     = true,
                bool parseVariables          = true
            )
            {
                TrimWhitespace = trimWhitespace;
                IsEmbeddedHashComment = isEmbeddedHashComment;
                UnescapeQuotedValues = unescapeQuotedValues;
                ClobberExistingVars = clobberExistingVars;
                ParseVariables = parseVariables;
            }
        }
    }
}
