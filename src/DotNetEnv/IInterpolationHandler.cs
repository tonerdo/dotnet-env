using System;

namespace DotNetEnv
{
    static class LookupHelper
    {
        public static string GetEnvironmentVariable(string key)
        {
            var val = Environment.GetEnvironmentVariable(key);
            if (val == null && Env.FakeEnvVars.TryGetValue(key, out var fakeVal))
            {
                return fakeVal;
            }

            return val;
        }
    }

    public interface IInterpolationHandler
    {
        string Handle(string key);
    }

    public class DirectSubstitutionInterpolationHandler : IInterpolationHandler
    {
        public string Handle(string key)
        {
            return LookupHelper.GetEnvironmentVariable(key) ?? string.Empty;
        }
    }

    public class DefaultInterpolationHandler : IInterpolationHandler
    {
        readonly string _defaultValue;

        public DefaultInterpolationHandler(string defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public string Handle(string key)
        {
            var val = LookupHelper.GetEnvironmentVariable(key);
            return val ?? _defaultValue;
        }
    }

    public class RequiredInterpolationHandler : IInterpolationHandler
    {
        public string Handle(string key)
        {
            var val = LookupHelper.GetEnvironmentVariable(key);
            return val ?? throw new Exception($"Required environment variable '{key}' is not set.");
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
            var val = LookupHelper.GetEnvironmentVariable(key);
            return val != null
                ? _replacementValue
                : string.Empty;
        }
    }
}
