using System;
using System.Collections.Generic;
using DotNetEnv.Extensions;
using Xunit;

namespace DotNetEnv.Tests;

public class ExtensionsTests
{
    [Fact]
    public void ToDotEnvDictionaryTest()
    {
        var kvpSetNoDupe = new List<KeyValuePair<string, string>>()
        {
            new("key", "value"),
            new("key2", "value2"),
        };

        var kvpSetWithDupe = new List<KeyValuePair<string, string>>()
        {
            new("key", "value"),
            new("key", "value2"),
        };

        Assert.Throws<ArgumentException>(() => kvpSetWithDupe.ToDotEnvDictionary(CreateDictionaryOption.Throw));
        var noDupeAndThrowOption = kvpSetNoDupe.ToDotEnvDictionary(CreateDictionaryOption.Throw);
        Assert.Equal("value", noDupeAndThrowOption["key"]);
        Assert.Equal("value2", noDupeAndThrowOption["key2"]);

        var withDupeAndTakeFirstOption = kvpSetWithDupe.ToDotEnvDictionary(CreateDictionaryOption.TakeFirst);
        Assert.Equal("value", withDupeAndTakeFirstOption["key"]);
        var noDupeAndTakeFirstOption = kvpSetNoDupe.ToDotEnvDictionary(CreateDictionaryOption.TakeFirst);
        Assert.Equal("value", noDupeAndTakeFirstOption["key"]);
        Assert.Equal("value2", noDupeAndTakeFirstOption["key2"]);

        var withDupeAndTakeLastOption = kvpSetWithDupe.ToDotEnvDictionary();
        Assert.Equal("value2", withDupeAndTakeLastOption["key"]);
        var noDupeAndTakeLastOption = kvpSetNoDupe.ToDotEnvDictionary();
        Assert.Equal("value", noDupeAndTakeLastOption["key"]);
        Assert.Equal("value2", noDupeAndTakeLastOption["key2"]);
    }
}
