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
            IEnumerable<KeyValuePair<string, string>> values;
            if (this.paths == null)
            {
                values = Env.Load(options: this.options);
            }
            else
            {
                if (this.paths.Length == 1)
                {
                    values = Env.Load(this.paths[0], this.options);
                }
                else
                {
                    values = Env.LoadMulti(this.paths, this.options);
                }
            }

            // Since the Load method does not take care of clobberring, We have to check it here!
            var dictionaryOption = options.ClobberExistingVars ? CreateDictionaryOption.TakeLast : CreateDictionaryOption.TakeFirst;
            var dotEnvDictionary = values.ToDotEnvDictionary(dictionaryOption);

            if (!options.ClobberExistingVars)
                foreach (string key in Environment.GetEnvironmentVariables().Keys)
                    dotEnvDictionary.Remove(key);

            foreach (var value in dotEnvDictionary)
                Data[NormalizeKey(value.Key)] = value.Value;
        }

        private static string NormalizeKey(string key)
            => key.Replace("__", ConfigurationPath.KeyDelimiter);
    }
}
