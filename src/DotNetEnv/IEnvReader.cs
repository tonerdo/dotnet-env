using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static System.Environment;

namespace DotNetEnv
{
    /// <summary>
    /// Defines methods for read the contents of an environment variable.
    /// </summary>
    public interface IEnvReader
    {
        /// <summary>
        /// Gets the value of an environment variable in string format.
        /// </summary>
        /// <param name="variable">The variable of the value to get.</param>
        /// <value>The value of the variable in string format.</value>
        /// <exception cref="EnvVariableNotFoundException">variable is not found in the current process.</exception>
        string this[string variable] { get; }

        /// <summary>
        /// Gets the value of an environment variable in string format.
        /// </summary>
        /// <param name="variable">The variable of the value to get.</param>
        /// <returns>The value of the variable in string format.</returns>
        /// <exception cref="EnvVariableNotFoundException">variable is not found in the current process.</exception>
        string GetStringValue(string variable);

        /// <summary>
        /// Gets the value of an environment variable in boolean format.
        /// </summary>
        /// <param name="variable">The variable of the value to get.</param>
        /// <returns>The value of the variable in boolean format.</returns>
        /// <exception cref="EnvVariableNotFoundException">variable is not found in the current process.</exception>
        bool GetBoolValue(string variable);

        /// <summary>
        /// Gets the value of an environment variable integer format.
        /// </summary>
        /// <param name="variable">The variable of the value to get.</param>
        /// <returns>The value of the variable in integer format.</returns>
        /// <exception cref="EnvVariableNotFoundException">variable is not found in the current process.</exception>
        int GetIntValue(string variable);

        /// <summary>
        /// Gets the value of an environment variable in double format.
        /// </summary>
        /// <param name="variable">The variable of the value to get.</param>
        /// <returns>The value of the variable in double format.</returns>
        /// <exception cref="EnvVariableNotFoundException">variable is not found in the current process.</exception>
        double GetDoubleValue(string variable);

        /// <summary>
        /// Try to retrieve the value of an environment variable in string format.
        /// </summary>
        /// <param name="variable">The variable of the value to try retrieve.</param>
        /// <param name="value">The string value retrieved or null.</param>
        /// <returns>true if the environment variable exists in the current process, otherwise false.</returns>
        bool TryGetStringValue(string variable, out string value);

        /// <summary>
        /// Try to retrieve the value of an environment variable in boolean format.
        /// </summary>
        /// <param name="variable">The variable of the value to try retrieve.</param>
        /// <param name="value">The boolean value retrieved or false.</param>
        /// <returns>true if the environment variable exists in the current process, otherwise false.</returns>
        bool TryGetBoolValue(string variable, out bool value);

        /// <summary>
        /// Try to retrieve the value of an environment variable in integer format.
        /// </summary>
        /// <param name="variable">The variable of the value to try retrieve.</param>
        /// <param name="value">The integer value retrieved or zero.</param>
        /// <returns>true if the environment variable exists in the current process, otherwise false.</returns>
        bool TryGetIntValue(string variable, out int value);

        /// <summary>
        /// Try to retrieve the value of an environment variable in double format.
        /// </summary>
        /// <param name="variable">The variable of the value to try retrieve.</param>
        /// <param name="value">The double value retrieved or zero.</param>
        /// <returns>true if the environment variable exists in the current process, otherwise false.</returns>
        bool TryGetDoubleValue(string variable, out double value);
    }

    /// <inheritdoc cref="IEnvReader" />
    public class EnvReader : IEnvReader
    {
        private const string EnvVariableNotFoundMessage = "The value could not be retrieved because it does not exist in the current process.";
        private static EnvReader instance;

        /// <summary>
        /// Gets an instance of type EnvReader.
        /// </summary>
        public static EnvReader Instance
        {
            get
            {
                if (instance == null)
                    instance = new EnvReader();

                return instance;
            }
        }

        public string this[string variable] => GetStringValue(variable);

        public bool GetBoolValue(string variable)
        {
            if (TryGetBoolValue(variable, out bool value))
                return value;

            throw new EnvVariableNotFoundException(EnvVariableNotFoundMessage, nameof(variable));
        }

        public double GetDoubleValue(string variable)
        {
            if (TryGetDoubleValue(variable, out double value))
                return value;

            throw new EnvVariableNotFoundException(EnvVariableNotFoundMessage, nameof(variable));
        }

        public int GetIntValue(string variable)
        {
            if (TryGetIntValue(variable, out int value))
                return value;

            throw new EnvVariableNotFoundException(EnvVariableNotFoundMessage, nameof(variable));
        }

        public string GetStringValue(string variable)
        {
            if (TryGetStringValue(variable, out string value))
                return value;

            throw new EnvVariableNotFoundException(EnvVariableNotFoundMessage, nameof(variable));
        }

        public bool TryGetBoolValue(string variable, out bool value)
        {
            var retrievedValue = GetEnvironmentVariable(variable);
            if (retrievedValue == null)
            {
                value = false;
                return false;
            }
            value = bool.Parse(retrievedValue);
            return true;
        }

        public bool TryGetDoubleValue(string variable, out double value)
        {
            var retrievedValue = GetEnvironmentVariable(variable);
            if (retrievedValue == null)
            {
                value = 0.0;
                return false;
            }
            value = double.Parse(retrievedValue, CultureInfo.InvariantCulture);
            return true;
        }

        public bool TryGetIntValue(string variable, out int value)
        {
            var retrievedValue = GetEnvironmentVariable(variable);
            if (retrievedValue == null)
            {
                value = 0;
                return false;
            }
            value = int.Parse(retrievedValue);
            return true;
        }

        public bool TryGetStringValue(string variable, out string value)
        {
            var retrievedValue = GetEnvironmentVariable(variable);
            if (retrievedValue == null)
            {
                value = null;
                return false;
            }
            value = retrievedValue;
            return true;
        }
    }

    /// <summary>
    /// The exception that is thrown when the environment variable is not found in the current process.
    /// </summary>
    public class EnvVariableNotFoundException : ArgumentException
    {
        public EnvVariableNotFoundException(string message, string paramName) : base(message, paramName)
        {

        }
    }
}
