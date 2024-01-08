using Xunit;

namespace DotNetEnv.Tests
{
    public class LoadOptionsTests
    {
        [Fact]
        public void StaticEnvTest()
        {
            LoadOptions options;

            options = DotNetEnv.Env.NoEnvVars();
            Assert.False(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = DotNetEnv.Env.NoClobber();
            Assert.True(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = DotNetEnv.Env.TraversePath();
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);
        }

        [Fact]
        public void StaticOptionsTest()
        {
            LoadOptions options;

            options = DotNetEnv.LoadOptions.NoEnvVars();
            Assert.False(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = DotNetEnv.LoadOptions.NoClobber();
            Assert.True(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = DotNetEnv.LoadOptions.TraversePath();
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);
        }

        [Fact]
        public void ConstructorTest()
        {
            LoadOptions options;

            options = new LoadOptions();
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new LoadOptions(setEnvVars: false);
            Assert.False(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new LoadOptions(clobberExistingVars: false);
            Assert.True(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new LoadOptions(onlyExactPath: false);
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);
        }

        [Fact]
        public void ObjectInitializerTest()
        {
            LoadOptions options;

            options = new LoadOptions() { SetEnvVars = false };
            Assert.False(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new LoadOptions() { ClobberExistingVars = false };
            Assert.True(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new LoadOptions() { OnlyExactPath = false };
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);
        }

        [Fact]
        public void InstanceTest()
        {
            LoadOptions options;

            options = new DotNetEnv.LoadOptions();
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new DotNetEnv.LoadOptions().NoEnvVars();
            Assert.False(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new DotNetEnv.LoadOptions().NoClobber();
            Assert.True(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = new DotNetEnv.LoadOptions().TraversePath();
            Assert.True(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);
        }

        [Fact]
        public void ComboTest()
        {
            LoadOptions options;

            options = DotNetEnv.Env.NoEnvVars().NoClobber().TraversePath();
            Assert.False(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);

            options = DotNetEnv.Env.NoClobber().TraversePath();
            Assert.True(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);

            options = DotNetEnv.Env.NoEnvVars().NoClobber();
            Assert.False(options.SetEnvVars);
            Assert.False(options.ClobberExistingVars);
            Assert.True(options.OnlyExactPath);

            options = DotNetEnv.Env.NoEnvVars().TraversePath();
            Assert.False(options.SetEnvVars);
            Assert.True(options.ClobberExistingVars);
            Assert.False(options.OnlyExactPath);
        }
    }
}
