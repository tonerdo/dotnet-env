[![Windows build status](https://ci.appveyor.com/api/projects/status/github/tonerdo/dotnet-env?branch=master&svg=true)](https://ci.appveyor.com/project/tonerdo/dotnet-env)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet version](https://badge.fury.io/nu/DotNetEnv.svg)](https://www.nuget.org/packages/DotNetEnv)
# dotnet-env

A .NET library to load environment variables from .env files. Supports .NET Core and .NET Framework

## Installation

Available on [NuGet](https://www.nuget.org/packages/DotNetEnv/)

Visual Studio:

```powershell
PM> Install-Package DotNetEnv
```

.NET Core CLI:

```bash
dotnet add package DotNetEnv
```

## Usage

### Load env file

`Load()` will automatically look for a `.env` file in the current directory
```csharp
DotNetEnv.Env.Load();
```

Or you can specify the path to the `.env` file
```csharp
DotNetEnv.Env.Load("./path/to/.env");
```

It's also possible to load the (text) file as a `Stream`

```csharp
using (var stream = File.OpenRead("./path/to/.env"))
{
    DotNetEnv.Env.Load(stream);
}
```

### Accessing environment variables

The variables in the `.env` can then be accessed through the `System.Environment` class

```csharp
System.Environment.GetEnvironmentVariable("IP");
```

Or through one of the helper methods:

```csharp
DotNetEnv.Env.GetString("A_STRING");
DotNetEnv.Env.GetBool("A_BOOL");
DotNetEnv.Env.GetInt("AN_INT");
DotNetEnv.Env.GetDouble("A_DOUBLE");
```

The helper methods also have an optional second argument which specifies what value to return if the variable is not found:

```csharp
DotNetEnv.Env.GetString("THIS_DOES_NOT_EXIST", "Variable not found");
```

### Additional arguments

You can also pass a `LoadOptions` object arg to all `DotNetEnv.Env.Load` variants to affect the Load/Parse behavior:

```csharp
new DotNetEnv.Env.LoadOptions(
    trimWhitespace: false,
    isEmbeddedHashComment: false,
    unescapeQuotedValues: false,
    clobberExistingVars: false,
    parseVariables: false
)
```

All parameters default to true, which means:

1. `trimWhitespace`, first arg: `true` in order to trim
 leading and trailing whitespace from keys and values such that

```env
  KEY  =  value
```

Would then be available as
```csharp
"value" == System.Environment.GetEnvironmentVariable("KEY")
null == System.Environment.GetEnvironmentVariable("  KEY  ")
```

`false` would mean:
```csharp
"  value" == System.Environment.GetEnvironmentVariable("  KEY  ")
null == System.Environment.GetEnvironmentVariable("KEY")
```

2. `isEmbeddedHashComment`, second arg: `true` in order to allow inline comments

```env
KEY=value  # comment
```

Would then be available as
```csharp
"value" == System.Environment.GetEnvironmentVariable("KEY")
```

`false` would mean:
```csharp
"value  # comment" == System.Environment.GetEnvironmentVariable("KEY")
```

Which is most useful when you want to do something like:
```env
KEY=value#moreValue#otherValue#etc
```

3. `unescapeQuotedValues`, third arg: `true` in order to unescape/parse
 quoted (single or double) values as being strings with escaped chars
 such as newline ("\n"), but also handles unicode chars
 (e.g. "\u00ae" and "\U0001F680") -- note that you can always include
 unescaped unicode chars anyway (e.g. "日本") if your .env is in UTF-8.
 Also note that there is no need to escape quotes inside.

```env
KEY="quoted\n\tvalue"
```

Would then be available as
```csharp
"quoted
    value" == System.Environment.GetEnvironmentVariable("KEY")
```

`false` would mean:
```csharp
"\"quoted\\n\\tvalue\"" == System.Environment.GetEnvironmentVariable("KEY")
```

4. `clobberExistingVars`, fourth arg: `false` to avoid overwriting existing environment variables

```env
KEY=value
```

```csharp
System.Environment.SetEnvironmentVariable("KEY", "really important value, don't overwrite");
DotNetEnv.Env.Load(
    new DotNetEnv.Env.LoadOptions(
        clobberExistingVars: false
    )
)
"really important value, don't overwrite" == System.Environment.GetEnvironmentVariable("KEY")  // not "value" from the .env file
```

5. `parseVariables`, fifth arg: `true` to parse existing environment variables

```env
FIRST_KEY=value1
SECOND_KEY=value2and$FIRST_KEY
THIRD_KEY=$EXISTING_ENVIRONMENT_VARIABLE;andvalue3
```

Would then be available as
```csharp
"value1" == System.Environment.GetEnvironmentVariable("FIRST_KEY")
"value2andvalue1" == System.Environment.GetEnvironmentVariable("SECOND_KEY")
"value;andvalue3" == System.Environment.GetEnvironmentVariable("THIRD_KEY") //EXISTING_ENVIRONMENT_VARIABLE already set to "value"
```

## A Note about Production and the Purpose of this library

You should not be using a .env file in production.  The purpose of this library is to enable easy local development.

Your dev team should have a .env with localdev testing credentials/etc stored in some secure storage -- 1pass/lastpass or s3 bucket or something like that.

Then every developer gets a copy of that file as part of onboarding that they save into their project dir that uses DotNetEnv to get env vars for configuration.

When the application is deployed into production, actual env vars should be used, not a static .env file!

This does mean that env vars, and thus this library, are only useful for load time configuration -- not anything that changes during the lifetime of an application's run.
(You should load env var values during startup or on first access and not look them up more than once during the application's lifetime.)

Admittedly, this is best practices advice, and if you want to use .env files in production, that's up to you.  But at least I have told you so. :)

## Issue Reporting

If you have found a bug or if you have a feature request, please report them at this repository issues section.

## Contributing

Run `dotnet test test/DotNetEnv.Tests` to run all tests.

`src/DotNetEnvEnv/Env.cs` is the entry point for all behavior.

Open a PR on Github if you have some changes, or an issue if you want to discuss some proposed changes before creating a PR for them.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
