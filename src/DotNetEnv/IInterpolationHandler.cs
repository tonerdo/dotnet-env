using System;

namespace DotNetEnv
{
    public interface IInterpolationHandler
    {
        string Handle(string key);
    }

    public class DirectSubstitutionInterpolationHandler : IInterpolationHandler
    {
        public string Handle(string key)
        {
            return Parsers.CurrentValueProvider.TryGetValue(key, out var value) ? value : string.Empty;
        }
    }

    public class DefaultInterpolationHandler : IInterpolationHandler
    {
        private readonly string _defaultValue;

        public DefaultInterpolationHandler(string defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public string Handle(string key)
        {
            return Parsers.CurrentValueProvider.TryGetValue(key, out var value) ? value : _defaultValue;
        }
    }

    public class RequiredInterpolationHandler : IInterpolationHandler
    {
        public string Handle(string key)
        {
            return Parsers.CurrentValueProvider.TryGetValue(key, out var value)
                ? value
                : throw new Exception($"Required environment variable '{key}' is not set.");
        }
    }

    public class ReplacementInterpolationHandler : IInterpolationHandler
    {
        private readonly string _replacementValue;

        public ReplacementInterpolationHandler(string replacementValue)
        {
            _replacementValue = replacementValue;
        }

        public string Handle(string key)
        {
            return Parsers.CurrentValueProvider.TryGetValue(key, out _)
                ? _replacementValue
                : string.Empty;
        }
    }
}
