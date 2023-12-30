using Microsoft.Extensions.Configuration;

namespace DotNetEnv.Configuration
{
    public class EnvConfigurationSource : IConfigurationSource
    {
        private readonly string[] paths;

        private readonly LoadOptions options;

        public EnvConfigurationSource(
            string[] paths,
            LoadOptions options)
        {
            this.paths = paths;
            this.options = options;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EnvConfigurationProvider(this.paths, this.options);
        }
    }
}
