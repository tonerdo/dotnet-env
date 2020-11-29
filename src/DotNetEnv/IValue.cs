using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetEnv
{
    public interface IValue
    {
        string GetValue ();
    }

    public interface IValueFactory
    {
        IValue GetInterpolated (string id);
        IValue GetActual (IEnumerable<string> strs);
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

        public ValueActual (string str)
        {
            _value = str;
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
    }

    public class InterpolatedValueFactory : IValueFactory
    {
        public IValue GetInterpolated (string str)
        {
            return new ValueInterpolated(str);
        }

        public IValue GetActual (IEnumerable<string> strs)
        {
            return new ValueActual(strs);
        }
    }

    public class ActualValueFactory : IValueFactory
    {
        public IValue GetInterpolated (string str)
        {
            return new ValueActual(str);
        }

        public IValue GetActual (IEnumerable<string> strs)
        {
            return new ValueActual(strs);
        }
    }
}
