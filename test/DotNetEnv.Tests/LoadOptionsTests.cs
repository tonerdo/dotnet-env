using DotNetEnv.Tests.XUnit;
using Xunit;

namespace DotNetEnv.Tests
{
    public class LoadOptionsTests
    {
        public static readonly IndexedTheoryData<(LoadOptions OptionsUnderTest,
                bool ExpectedSetEnvVars,
                bool ExpectedClobberExistingVars,
                bool ExpectedOnlyExactPath)>
            LoadOptionTestCombinations = new ()
            {
                (Env.NoEnvVars(), false, true, true),
                (Env.NoClobber(), true, false, true),
                (Env.TraversePath(), true, true, false),
                (LoadOptions.NoEnvVars(), false, true, true),
                (LoadOptions.NoClobber(), true, false, true),
                (LoadOptions.TraversePath(), true, true, false),
                (new LoadOptions(), true, true, true),
                (new LoadOptions().NoEnvVars(), false, true, true),
                (new LoadOptions().NoClobber(), true, false, true),
                (new LoadOptions().TraversePath(), true, true, false),
                (Env.NoEnvVars().NoClobber().TraversePath(), false, false, false),
                (Env.NoClobber().TraversePath(), true, false, false),
                (Env.NoEnvVars().NoClobber(), false, false, true),
                (Env.NoEnvVars().TraversePath(), false, true, false),
            };

        [Theory]
        [MemberData(nameof(LoadOptionTestCombinations))]
        public void LoadOptionsShouldHaveCorrectPropertiesSet(string _,
            (LoadOptions OptionsUnderTest,
                bool ExpectedSetEnvVars,
                bool ExpectedClobberExistingVars,
                bool ExpectedOnlyExactPath) testData)
        {
            var (optionsUnderTest,
                    expectedSetEnvVars,
                    expectedClobberExistingVars,
                    expectedOnlyExactPath) = testData;

            Assert.Equal(expectedSetEnvVars, optionsUnderTest.SetEnvVars);
            Assert.Equal(expectedClobberExistingVars, optionsUnderTest.ClobberExistingVars);
            Assert.Equal(expectedOnlyExactPath, optionsUnderTest.OnlyExactPath);
        }
    }
}
