using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetEnv
{
    public interface IValue
    {
        string GetValue ();
    }

    public class ValueInterpolated : IValue
    {
        private readonly string _id;

        public ValueInterpolated (string id)
        {
            _id = id;
        }

        public string GetValue ()
        {
            return Environment.GetEnvironmentVariable(_id) ?? string.Empty;
        }
    }

    public class ValueActual : IValue
    {
        private readonly string _value;

        public ValueActual (IEnumerable<string> strs)
        {
            _value = string.Join(string.Empty, strs);
        }

        public string GetValue ()
        {
            return _value;
        }
    }

    public class ValueCalculator
    {
        public readonly string Value;

        public ValueCalculator (IEnumerable<IValue> values)
        {
            // note that we do want this lookup / calculation / GetValue calls in the ctor
            // because it is the state of the world at the moment that this value is calculated
            Value = string.Join(string.Empty, values.Select(val => val.GetValue()));
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
