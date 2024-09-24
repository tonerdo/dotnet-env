using System;
using System.Collections.Generic;
using DotNetEnv.Extensions;
using Microsoft.Extensions.Configuration;

namespace DotNetEnv.Configuration
{
    public class EnvConfigurationProvider : ConfigurationProvider
    {
        private readonly string[] paths;

        private readonly LoadOptions options;

        public EnvConfigurationProvider(
            string[] paths,
            LoadOptions options)
        {
            this.paths = paths;
            this.options = options ?? LoadOptions.DEFAULT;
        }

        public override void Load()
        {
            var values = paths == null
                ? Env.Load(options: options)
                : Env.LoadMulti(paths, options);

            foreach (var value in values)
                Data[NormalizeKey(value.Key)] = value.Value;
        }

        private static string NormalizeKey(string key)
            => key.Replace("__", ConfigurationPath.KeyDelimiter);
    }
}
