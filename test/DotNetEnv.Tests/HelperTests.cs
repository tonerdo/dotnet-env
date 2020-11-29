using System;
using Xunit;

namespace DotNetEnv.Tests
{
    public class HelperTests
    {
        private const string VariableNotPresentKey = nameof(VariableNotPresentKey);

        [Fact]
        public void GetStringTest()
        {
            var key = "A_STRING";
            var value = "This is a string";

            Environment.SetEnvironmentVariable(key, value);

            Assert.Equal(value, Env.GetString(key));
            Assert.Equal(default(string), Env.GetString(VariableNotPresentKey));
            Assert.Equal("none", Env.GetString(VariableNotPresentKey, "none"));
        }

        [Fact]
        public void GetBoolTest()
        {
            var key1 = "TRUE_VALUE";
            var value1 = "true";
            var key2 = "FALSE_VALUE";
            var value2 = "false";

            Environment.SetEnvironmentVariable(key1, value1);
            Environment.SetEnvironmentVariable(key2, value2);

            Assert.True(Env.GetBool(key1));
            Assert.False(Env.GetBool(key2));
            Assert.False(Env.GetBool(VariableNotPresentKey));
            Assert.True(Env.GetBool(VariableNotPresentKey, true));
        }

        [Fact]
        public void GetIntTest()
        {
            var key1 = "ONE_STRING";
            var value1 = "1";

            Environment.SetEnvironmentVariable(key1, value1);

            Assert.Equal(1, Env.GetInt(key1));
            Assert.Equal(0, Env.GetInt(VariableNotPresentKey));
            Assert.Equal(-1, Env.GetInt(VariableNotPresentKey, -1));
        }

        [Fact]
        public void GetDoubleTest()
        {
            var key1 = "ONE_POINT_TWO_STRING";
            var value1 = "1.2";
            var key2 = "ONE_POINT_TWO_STRING_WITH_COMMA";
            var value2 = "1,2";

            Environment.SetEnvironmentVariable(key1, value1);
            Environment.SetEnvironmentVariable(key2, value2);

            Assert.Equal(1.2, Env.GetDouble(key1));
            Assert.Equal(12D, Env.GetDouble(key2));
            Assert.Equal(0, Env.GetDouble(VariableNotPresentKey));
            Assert.Equal(-1.2, Env.GetDouble(VariableNotPresentKey, -1.2));
        }
    }
}
