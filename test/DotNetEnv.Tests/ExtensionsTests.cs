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

    /// <summary>
    /// Data: _, dictionaryOption, input, expectedValue
    /// </summary>
    public static readonly TheoryData<
            string, CreateDictionaryOption, KeyValuePair<string, string>[], KeyValuePair<string, string>[]>
        ToDotEnvDictionaryTestData =
            new IndexedTheoryData<
                CreateDictionaryOption, KeyValuePair<string, string>[], KeyValuePair<string, string>[]>
            {
                { CreateDictionaryOption.Throw, KvpSetNoDupe, KvpSetNoDupe },
                { CreateDictionaryOption.TakeFirst, KvpSetWithDupe, new[] { FirstValuePair } },
                { CreateDictionaryOption.TakeFirst, KvpSetNoDupe, KvpSetNoDupe },
                { CreateDictionaryOption.TakeLast, KvpSetWithDupe, new[] { FirstValuePairDupe } },
                { CreateDictionaryOption.TakeLast, KvpSetNoDupe, KvpSetNoDupe },
            };

    [Theory]
    [MemberData(nameof(ToDotEnvDictionaryTestData))]
    public void ToDotEnvDictionaryWithKvpSetNoDupeShouldContainValues(string _,
        CreateDictionaryOption dictionaryOption,
        KeyValuePair<string, string>[] input,
        KeyValuePair<string, string>[] expected)
    {
        var dotEnvDictionary = input.ToDotEnvDictionary(dictionaryOption);

        foreach (var (key, value) in expected)
            Assert.Equal(value, dotEnvDictionary[key]);
    }

    [Theory]
    [MemberData(nameof(ToDotEnvDictionaryTestData))]
    public void ToDotEnvDictionaryWithKvpSetNoDupeShouldHaveCorrectNumberOfEntries(string _,
        CreateDictionaryOption dictionaryOption,
        KeyValuePair<string, string>[] input,
        KeyValuePair<string, string>[] expectedValues)
    {
        var dotEnvDictionary = input.ToDotEnvDictionary(dictionaryOption);

        Assert.Equal(expectedValues.Length, dotEnvDictionary.Count);
    }

    [Fact]
    public void ToDotEnvDictionaryWithThrowOptionShouldThrowOnDupes() =>
        Assert.Throws<ArgumentException>(() => KvpSetWithDupe.ToDotEnvDictionary(CreateDictionaryOption.Throw));
}
