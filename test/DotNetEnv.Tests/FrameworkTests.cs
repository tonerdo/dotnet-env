using System;
using System.Text;
using DotNetEnv.Tests.Helper;
using Xunit;

namespace DotNetEnv.Tests;

public class FrameworkTests
{
    [Fact]
    public void CheckUnicodeFunctionality()
    {
        Assert.Equal("®", Encoding.Unicode.GetString(new byte[] { 0xae, 0x00 }));
        Assert.Equal(UnicodeChars.Rocket, Encoding.UTF32.GetString(new byte[] { 0x80, 0xf6, 0x01, 0x00 }));
        Assert.Equal(UnicodeChars.Rocket, Encoding.UTF32.GetString(new byte[] { 0x80, 0xf6, 0x1, 0x0 }));
    }

    [Fact]
    public void CheckEnvironmentVariableFunctionality()
    {
        Environment.SetEnvironmentVariable("EV_DNE", null);
        Assert.Null(Environment.GetEnvironmentVariable("EV_DNE"));

        // Note that dotnet returns null if the env var is empty -- even if it was set to empty!
        Environment.SetEnvironmentVariable("EV_DNE", "");
        Assert.Null(Environment.GetEnvironmentVariable("EV_DNE"));
    }
}
