using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotNetEnv
{
    public interface IValue
    {
        string GetValue (bool interpolationEnabled = true);
    }

    public class ValueInterpolated : IValue
    {
        private readonly string _id;
        private readonly string _raw;

        public ValueInterpolated (string id, string raw)
        {
            _id = id;
            _raw = raw;
        }

        public string GetValue(bool interpolationEnabled)
        {
            return interpolationEnabled
                ? Environment.GetEnvironmentVariable(_id) ?? string.Empty
                : _raw;
        }
    }

    public class ValueActual : IValue
    {
        private readonly string _value;

        public ValueActual (IEnumerable<string> strs)
            : this(string.Join(string.Empty, strs)) {}

        public ValueActual (string str)
        {
            _value = str;
        }

        public string GetValue(bool interpolationEnabled)
        {
            return _value;
        }
    }

    public class ValueCalculator
    {
        public string Value { get; private set; }
        public string RawValue { get; private set; }

        public ValueCalculator (IList<IValue> values)
        {
            RawValue = string.Concat(values.Select(val => val.GetValue(false)));
            
            // note that we do want this lookup / calculation / GetValue calls in the ctor
            // because it is the state of the world at the moment that this value is calculated
            Value = string.Join(string.Empty, values.Select(val => val.GetValue(true)));
        }
    }
}
