[![Windows build status](https://ci.appveyor.com/api/projects/status/github/rogusdev/dotnet-env?branch=master&svg=true)](https://ci.appveyor.com/project/rogusdev/dotnet-env)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet version](https://badge.fury.io/nu/DotNetEnv.svg)](https://www.nuget.org/packages/DotNetEnv)
# dotnet-env

A .NET library to load environment variables from .env files. Supports .NET Core and .NET Framework (4.6+).

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

`Load()` will automatically look for a `.env` file in the current directory by default,
  or any higher parent/ancestor directory if given the option flag via TraversePath()
```csharp
DotNetEnv.Env.Load();
DotNetEnv.Env.TraversePath().Load();
```

Or you can specify the path directly to the `.env` file,
 and as above, with `TraversePath()`, it will start looking there
 and then look in higher dirs from there if not found.
```csharp
DotNetEnv.Env.Load("./path/to/.env");
```

It's also possible to load the (text) file as a `Stream` or `string` or multiple files in sequence

```csharp
using (var stream = File.OpenRead("./path/to/.env"))
{
    DotNetEnv.Env.Load(stream);
}

DotNetEnv.Env.LoadContents("OK=GOOD\nTEST=\"more stuff\"");

// will use values in later files over values in earlier files
// NOTE: NoClobber will reverse this, it will use the first value encountered!
DotNetEnv.Env.LoadMulti(new[] {
    ".env",
    ".env2",
});
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
new DotNetEnv.LoadOptions(
    setEnvVars: true,
    clobberExistingVars: true,
    onlyExactPath: true
)
```

However the recommended approach is with a fluent syntax for turning flags off such as:

```csharp
DotNetEnv.Env.NoEnvVars().NoClobber().TraversePath().Load();
```

All parameters default to true, which means:

1. `setEnvVars`, first arg: `true` in order to actually update env vars.
 Setting it `false` allows consumers of this library to process the .env file
 but use it for things other than updating env vars, as a generic configuration file.
 The Load methods all return an `IEnumerable<KeyValuePair<string,string>>` for this, but
 there is an extension method `ToDotEnvDictionary()` to get a dict with the last value for each key.

```env
KEY=value
```

```csharp
var kvps = DotNetEnv.Env.Load(
    options: new DotNetEnv.Env.LoadOptions(
        setEnvVars: false
    )
)

// or the recommended, cleaner (fluent) approach:
var dict = DotNetEnv.Env.NoEnvVars().Load().ToDotEnvDictionary();

// not "value" from the .env file
null == System.Environment.GetEnvironmentVariable("KEY")
"KEY" == kvps.First().Key
"value" == kvps.First().Value
```

With `CreateDictionaryOption` you can change behavior of `ToDotEnvDictionary` to take either the First value or to throw on duplicates. With the `TakeFirst` options you can simulate `NoClobber`-behavior.

2. `clobberExistingVars`, second arg: `true` to always set env vars,
 `false` would leave existing env vars alone.

```env
KEY=value
```

```csharp
System.Environment.SetEnvironmentVariable("KEY", "really important value, don't overwrite");
DotNetEnv.Env.Load(
    options: new DotNetEnv.Env.LoadOptions(
        clobberExistingVars: false
    )
)

// or the recommended, cleaner (fluent) approach:
DotNetEnv.Env.NoClobber().Load();

// not "value" from the .env file
"really important value, don't overwrite" == System.Environment.GetEnvironmentVariable("KEY")
```

3. `exactPathOnly`, third arg: `true` to require .env to be
 in the current directory if not specified, or to match the exact path passed in,
 `false` would traverse the parent directories above the current or given path
 to find the nearest `.env` file or whatever name was passed in.
 This option only applies to Env.Load that takes a string path.

See `DotNetEnvTraverse.Tests` for examples.

```csharp
// the recommended, cleaner (fluent) approach:
DotNetEnv.Env.TraversePath().Load();
```

### Using .NET Configuration provider

Integrating with the usual ConfigurationBuilder used in .NET is simple!

```csharp
var configuration = new ConfigurationBuilder()
    .AddDotNetEnv(".env", LoadOptions.TraversePath()) // Simply add the DotNetEnv configuration source!
    .Build();
```

The configuration provider will map `__` as `:` to allow sections!

## .env file structure

All lines must be valid assignments or empty lines (with optional comments).

A minimal valid assignment looks like:
```sh
KEY=value
```

There can optionally be one of a few export or equivalent keywords at the beginning
 and there can be a comment at the end, values can be quoted to include whitespace,
 and interpolated references can be included (unquoted values as well as double quoted,
 with optional braces in both cases -- but often more useful in unquoted), like:
```sh
export KEY="extra $ENVVAR value" # comment
set KEY2=extra${ENVVAR}value # comment
```

The options for the export keyword are:

    export  # bash
    set     # windows cmd
    SET     # windows cmd
    set -x  # fish

This allows the `.env` file itself to be `source`-d like `. .env`
 to load the env vars into a terminal session directly.

The options for quoting values are:

1. `""` double: can have everything: interpolated variables, plus whitespace, escaped chars, and byte code chars
1. `''` single: can have whitespace, but no interpolation, no escaped chars, no byte code chars -- notably not even escaped single quotes inside -- single quoted values are for when you want truly raw values
1. unquoted: can have interpolated variables, but only inline whitespace, and no quote chars, no escaped chars, nor byte code chars

As these are the options bash recognizes. However, while bash does have
 special meaning for each of these, in this library, they are all the same,
 other than that you do not need to escape single quote chars inside
 a double quoted value, nor double quotes inside single quotes.

As a special note: if a value is unquoted, it can still include a `#` char,
 which might look like it is starting a comment, like:
```sh
KEY=value#notcomment #actualcomment
```

This is how bash works as well:
```sh
export TEST=value#notcomment #actualcomment
env | grep TEST
# TEST=value#notcomment
```

However, unlike bash, a `#` directly after the `=` will be recognized as a comment:
```sh
KEY=#yesacomment
```

This is because whitespaces between `=` and the value are allowed by this library, which is not allowed in bash. This prevents confusion between `KEY=#comment` and `KEY= #comment`, which is expected to give the same result when leading whitespaces before the value are allowed.

Also unlike bash, inline whitespace is allowed so you can do:
```
KEY=value#notcomment more	words here # yes comment

"value#notcomment more	words here" == System.Environment.GetEnvironmentVariable("KEY")
```

You can also declare unicode chars as byte codes in double quoted values:

    UTF8 btes: "\xF0\x9F\x9A\x80" # rocket 🚀
    UTF16 bytes: "\uae" # registered ®
    UTF32 bytes: "\U1F680" # rocket 🚀

Capitalization on the hex chars is irrelevant, and leading zeroes are optional.

And standard escaped chars like `\t`, `\\``, `\n`, etc are also recognized
 -- though quoted strings can also be multi line, e.g.:

```sh
KEY="value
and more"
OTHER='#not_comment
line2'
```

Loaded gives:
```csharp
"value\nand more" == System.Environment.GetEnvironmentVariable("KEY")
"#not_comment\nline2" == System.Environment.GetEnvironmentVariable("OTHER")
```

You can also include whitespace before and after the equals sign in assignments,
 between the name/identifier, and the value, quoted or unquoted.
 Note that the pre/trailing and post/leading whitespace will be ignored.
 If you want leading whitepace on your values, quote them with whitespace.
```sh
WHITE_BOTH = value
WHITE_QUOTED=" value "
```

Loaded gives:
```csharp
"value" == System.Environment.GetEnvironmentVariable("WHITE_BOTH")
" value " == System.Environment.GetEnvironmentVariable("WHITE_QUOTED")
```

Note that bash env vars do not allow white space pre or post equals,
 so this is a convenience feature that will break sourcing .env files.
 But then, not all of this is 100% compatible anyway, and that's ok.

Note that other .env parsing libraries also might have slightly different rules
 -- no consistent rules have arisen industry wide yet.

## A Note about Production and the Purpose of This Library

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

Run `dotnet test` to run all tests.

Or some more specific test examples:

    dotnet test --filter "FullyQualifiedName~DotNetEnv.Tests.EnvTests.BadSyntaxTest"
    dotnet test --filter "FullyQualifiedName~DotNetEnv.Tests.ParserTests.ParseAssignment"

`src/DotNetEnvEnv/Env.cs` is the entry point for all behavior.

`src/DotNetEnvEnv/Parsers.cs` defines all the [Sprache](https://github.com/sprache/Sprache) parsers.

The `DotNetEnvTraverse.Tests` project tests loading `.env` files in parent (or higher) directories from the executable.

Open a PR on Github if you have some changes, or an issue if you want to discuss some proposed changes before creating a PR for them.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
