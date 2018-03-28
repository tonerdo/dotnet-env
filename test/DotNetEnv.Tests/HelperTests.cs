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
            
            Assert.Equal(Env.GetString(key), value);
            Assert.Equal(Env.GetString(VariableNotPresentKey), default(string));
            Assert.Equal(Env.GetString(VariableNotPresentKey, "none"), "none");
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
            
            Assert.Equal(Env.GetBool(key1), true);
            Assert.Equal(Env.GetBool(key2), false);
            Assert.Equal(Env.GetBool(VariableNotPresentKey), false);
            Assert.Equal(Env.GetBool(VariableNotPresentKey, true), true);
        }
        
        [Fact]
        public void GetIntTest()
        {
            var key1 = "ONE_STRING";
            var value1 = "1";
            
            Environment.SetEnvironmentVariable(key1, value1);
            
            Assert.Equal(Env.GetInt(key1), 1);
            Assert.Equal(Env.GetInt(VariableNotPresentKey), 0);
            Assert.Equal(Env.GetInt(VariableNotPresentKey, -1), -1);
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
            
            Assert.Equal(Env.GetDouble(key1), 1.2);
            Assert.Equal(Env.GetDouble(key2), 12D);
            Assert.Equal(Env.GetDouble(VariableNotPresentKey), 0);
            Assert.Equal(Env.GetDouble(VariableNotPresentKey, -1.2), -1.2);
        }
    }
}
