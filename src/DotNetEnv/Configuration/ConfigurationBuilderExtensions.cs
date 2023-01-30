using Microsoft.Extensions.Configuration;

namespace DotNetEnv.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDotNetEnv(
            this IConfigurationBuilder builder,
            string path = null,
            LoadOptions options = null)
        {

            builder.Add(new EnvConfigurationSource(path == null ? null : new[] { path }, options));
            return builder;
        }

        public static IConfigurationBuilder AddDotNetEnvMulti(
            this IConfigurationBuilder builder,
            string[] paths,
            LoadOptions options = null)
        {
            builder.Add(new EnvConfigurationSource(paths, options));
            return builder;
        }
    }
}
