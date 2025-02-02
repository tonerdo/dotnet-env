using System;
using System.Collections.Generic;
using DotNetEnv.Extensions;
using DotNetEnv.Tests.XUnit;
using Xunit;

namespace DotNetEnv.Tests;

public class ExtensionsTests
{
    private static readonly KeyValuePair<string, string> FirstValuePair = new("key", "value");
    private static readonly KeyValuePair<string, string> FirstValuePairDupe = new("key", "dupe");
    private static readonly KeyValuePair<string, string> SecondValuePair = new("key2", "value2");

    private static readonly KeyValuePair<string, string>[] KvpSetNoDupe = { FirstValuePair, SecondValuePair };
    private static readonly KeyValuePair<string, string>[] KvpSetWithDupe = { FirstValuePair, FirstValuePairDupe };

    public static readonly TheoryData<
            string, KeyValuePair<string, string>[], CreateDictionaryOption, KeyValuePair<string, string>[]>
        ToDotEnvDictionaryTestData =
            new IndexedTheoryData<
                KeyValuePair<string, string>[], CreateDictionaryOption, KeyValuePair<string, string>[]>
            {
                { KvpSetNoDupe, CreateDictionaryOption.Throw, KvpSetNoDupe },
                { KvpSetWithDupe, CreateDictionaryOption.TakeFirst, new[] { FirstValuePair } },
                { KvpSetNoDupe, CreateDictionaryOption.TakeFirst, KvpSetNoDupe },
                { KvpSetWithDupe, CreateDictionaryOption.TakeLast, new[] { FirstValuePairDupe } },
                { KvpSetNoDupe, CreateDictionaryOption.TakeLast, KvpSetNoDupe },
            };

    [Theory]
    [MemberData(nameof(ToDotEnvDictionaryTestData))]
    public void ToDotEnvDictionaryWithKvpSetNoDupeShouldContainValues(string _,
        KeyValuePair<string, string>[] input,
        CreateDictionaryOption dictionaryOption,
        KeyValuePair<string, string>[] expectedValues)
    {
        var dotEnvDictionary = input.ToDotEnvDictionary(dictionaryOption);

        foreach (var expectedValue in expectedValues)
            Assert.Equal(expectedValue.Value, dotEnvDictionary[expectedValue.Key]);
    }

    [Theory]
    [MemberData(nameof(ToDotEnvDictionaryTestData))]
    public void ToDotEnvDictionaryWithKvpSetNoDupeShouldHaveCorrectNumberOfEntries(string _,
        KeyValuePair<string, string>[] input,
        CreateDictionaryOption dictionaryOption,
        KeyValuePair<string, string>[] expectedValues)
    {
        var dotEnvDictionary = input.ToDotEnvDictionary(dictionaryOption);

        Assert.Equal(expectedValues.Length, dotEnvDictionary.Count);
    }

    [Fact]
    public void ToDotEnvDictionaryWithThrowOptionShouldThrowOnDupes() =>
        Assert.Throws<ArgumentException>(() => KvpSetWithDupe.ToDotEnvDictionary(CreateDictionaryOption.Throw));
}
