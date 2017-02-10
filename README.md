# dotnet-env [![NuGet version](https://badge.fury.io/nu/DotNetEnv.svg)](https://www.nuget.org/packages/DotNetEnv)

A library to load .env file into Environment variable

## How to use
Add as a dependency to your project.json
```json
"dependencies": {
    "DotNetEnv": "1.0.0"
}
```
Restore dependencies to install
```bash
dotnet restore
```

## Usage

Will automatically look for a `.env` file in the current directory
```csharp
DotNetEnv.Env.Load();
```

You can also specify the path to the `.env` file
```csharp
DotNetEnv.Env.Load("./path/to/.env");
```

The variables in the `.env` can then be accessed through the `System.Environment` class
```csharp
System.Environment.GetEnvironmentVariable("IP")
```

## Issue Reporting

If you have found a bug or if you have a feature request, please report them at this repository issues section.

## Author

[Toni Solarin-Sodara](https://github.com/tsolarin)

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
