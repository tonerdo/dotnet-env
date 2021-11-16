using System;
using Xunit;

namespace DotNetEnv.Tests
{
    public class EnvReaderTests
    {
        private const string VariableNotFound = nameof(VariableNotFound);

        [Fact]
        public void GetStringValueTest()
        {
            var reader = new EnvReader();
            var key = "A_STRING";
            var value = "This is a string";

            Environment.SetEnvironmentVariable(key, value);

            Assert.Equal(value, reader.GetStringValue(key));
            Assert.Equal(value, reader[key]);
            Assert.Throws<EnvVariableNotFoundException>(
                () => reader.GetStringValue(VariableNotFound)
            );
            Assert.Throws<EnvVariableNotFoundException>(
                () => reader[VariableNotFound]
            );
        }

        [Fact]
        public void GetBoolValueTest()
        {
            var reader = new EnvReader();
            var key1 = "TRUE_VALUE";
            var value1 = "true";
            var key2 = "FALSE_VALUE";
            var value2 = "false";

            Environment.SetEnvironmentVariable(key1, value1);
            Environment.SetEnvironmentVariable(key2, value2);

            Assert.True(reader.GetBoolValue(key1));
            Assert.False(reader.GetBoolValue(key2));
            Assert.Throws<EnvVariableNotFoundException>(
                () => reader.GetBoolValue(VariableNotFound)
            );
        }

        [Fact]
        public void GetIntValueTest()
        {
            var reader = new EnvReader();
            var key1 = "ONE_STRING";
            var value1 = "1";

            Environment.SetEnvironmentVariable(key1, value1);

            Assert.Equal(1, reader.GetIntValue(key1));
            Assert.Throws<EnvVariableNotFoundException>(
                () => reader.GetIntValue(VariableNotFound)
            );
        }

        [Fact]
        public void GetDoubleValueTest()
        {
            var reader = new EnvReader();
            var key1 = "ONE_POINT_TWO_STRING";
            var value1 = "1.2";
            var key2 = "ONE_POINT_TWO_STRING_WITH_COMMA";
            var value2 = "1,2";

            Environment.SetEnvironmentVariable(key1, value1);
            Environment.SetEnvironmentVariable(key2, value2);

            Assert.Equal(1.2, reader.GetDoubleValue(key1));
            Assert.Equal(12D, reader.GetDoubleValue(key2));
            Assert.Throws<EnvVariableNotFoundException>(
                () => reader.GetDoubleValue(VariableNotFound)
            );
        }

        [Fact]
        public void TryGetStringValueTest()
        {
            var reader = new EnvReader();
            var key = "A_STRING";
            var value = "This is a string";

            Environment.SetEnvironmentVariable(key, value);

            reader.TryGetStringValue(key, out string result);
            Assert.Equal(value, result);
            Assert.False(reader.TryGetStringValue(VariableNotFound, out _));
        }

        [Fact]
        public void TryGetBoolValueTest()
        {
            var reader = new EnvReader();
            var key1 = "TRUE_VALUE";
            var value1 = "true";
            var key2 = "FALSE_VALUE";
            var value2 = "false";

            Environment.SetEnvironmentVariable(key1, value1);
            Environment.SetEnvironmentVariable(key2, value2);

            reader.TryGetBoolValue(key1, out bool result);
            Assert.True(result);
            reader.TryGetBoolValue(key2, out result);
            Assert.False(result);
            Assert.False(reader.TryGetBoolValue(VariableNotFound, out _));
        }

        [Fact]
        public void TryGetIntValueTest()
        {
            var reader = new EnvReader();
            var key1 = "ONE_STRING";
            var value1 = "1";

            Environment.SetEnvironmentVariable(key1, value1);

            reader.TryGetIntValue(key1, out int result);
            Assert.Equal(1, result);
            Assert.False(reader.TryGetIntValue(VariableNotFound, out _));
        }

        [Fact]
        public void TryGetDoubleValueTest()
        {
            var reader = new EnvReader();
            var key1 = "ONE_POINT_TWO_STRING";
            var value1 = "1.2";
            var key2 = "ONE_POINT_TWO_STRING_WITH_COMMA";
            var value2 = "1,2";

            Environment.SetEnvironmentVariable(key1, value1);
            Environment.SetEnvironmentVariable(key2, value2);

            reader.TryGetDoubleValue(key1, out double result);
            Assert.Equal(1.2, result);
            reader.TryGetDoubleValue(key2, out result);
            Assert.Equal(12D, result);
            Assert.False(reader.TryGetDoubleValue(VariableNotFound, out _));
        }
    }
}
