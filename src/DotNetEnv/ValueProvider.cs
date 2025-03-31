using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetEnv
{
    internal interface IValueProvider
    {
        string GetValue(string key);
        bool TryGetValue(string key, out string value);
    }

    internal abstract class ValueProvider : IValueProvider
    {
        public virtual string GetValue(string key) => TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException();

        public abstract bool TryGetValue(string key, out string value);
        
        public static readonly IValueProvider Empty = new EmptyValueProvider();
    }

    internal class EmptyValueProvider : IValueProvider
    {
        public string GetValue(string key) => null;

        public bool TryGetValue(string key, out string value)
        {
            value = null;
            return false;
        }
    }

    internal class EnvironmentValueProvider : ValueProvider
    {
        public override bool TryGetValue(string key, out string value)
        {
            value = Environment.GetEnvironmentVariable(key);
            return value != null;
        }
    }

    internal class DictionaryValueProvider : ValueProvider
    {
        private readonly IDictionary<string, string> _keyValuePairs;

        public DictionaryValueProvider(IDictionary<string, string> dictionary)
            => _keyValuePairs = dictionary;

        public override bool TryGetValue(string key, out string value) => _keyValuePairs.TryGetValue(key, out value);
    }

    internal class KeyValuePairValueProvider : ValueProvider
    {
        private readonly bool _clobberExisting;
        private readonly IList<KeyValuePair<string, string>> _keyValuePairs;

        public KeyValuePairValueProvider(bool clobberExisting, IList<KeyValuePair<string, string>> keyValuePairs)
        {
            _clobberExisting = clobberExisting;
            _keyValuePairs = keyValuePairs;
        }

        public override bool TryGetValue(string key, out string value)
        {
            value = (_clobberExisting ? _keyValuePairs.Reverse() : _keyValuePairs)
                .FirstOrDefault(pair => pair.Key == key).Value;
            return value != null;
        }
    }

    internal class ChainedValueProvider : ValueProvider
    {
        private readonly bool _clobberExisting;
        private readonly IValueProvider[] _providers;

        public ChainedValueProvider(bool clobberExisting, params IValueProvider[] providers)
        {
            _clobberExisting = clobberExisting;
            _providers = providers;
        }

        public override bool TryGetValue(string key, out string value)
        {
            foreach (var provider in _clobberExisting ? _providers.Reverse() : _providers)
                if (provider.TryGetValue(key, out value))
                    return true;

            value = null;
            return false;
        }
    }
}
