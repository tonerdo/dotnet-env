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
        private readonly IInterpolationHandler _interpolationHandler;

        public ValueInterpolated (string id, IInterpolationHandler interpolationHandler)
        {
            _id = id;
            _interpolationHandler = interpolationHandler;
        }

        public string GetValue ()
        {
            return _interpolationHandler.Handle(_id);
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

        public string GetValue ()
        {
            return _value;
        }
    }

    public class ValueCalculator
    {
        public string Value { get; private set; }

        public ValueCalculator (IEnumerable<IValue> values)
        {
            // note that we do want this lookup / calculation / GetValue calls in the ctor
            // because it is the state of the world at the moment that this value is calculated
            Value = string.Join(string.Empty, values.Select(val => val.GetValue()));
        }
    }
}
