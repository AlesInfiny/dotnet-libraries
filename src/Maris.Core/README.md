<!-- textlint-disable ja-technical-writing/sentence-length -->
# Maris.Core

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.Core](https://img.shields.io/nuget/v/Maris.Core?style=for-the-badge&logo=nuget)][NuGet Maris.Core]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[日本語版](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.Core/README.ja.md)

Provides core libraries for .NET business application development.

## Install

Run the following command in Package Manager Console or Command Prompt to install `Maris.Core`.

- Package Manager Console

```winbatch
Install-Package Maris.Core
```

- Command Prompt

```bash
dotnet add package Maris.Core
```

## Usage

### Handling Business Errors

You can handle errors that occur in business logic in a structured way.
Define error information with the `BusinessError` class and throw it using the `BusinessException` exception class.

```csharp
using Maris.Core;

// Create error messages
var errorMessage1 = new ErrorMessage("Product code {0} does not exist.", "P001");
var errorMessage2 = new ErrorMessage("Insufficient stock.");

// Create business error
var businessError = new BusinessError("ERR001", errorMessage1, errorMessage2);

// Throw business exception
throw new BusinessException(businessError);
```

You can also handle multiple business errors together.

```csharp
using Maris.Core;

// Create multiple business errors
var error1 = new BusinessError("ERR001", new ErrorMessage("Input error."));
var error2 = new BusinessError("ERR002", new ErrorMessage("Validation error."));

// Throw multiple errors together
var exception = new BusinessException(error1, error2);
throw exception;

// Retrieve error information
foreach (var error in exception.GetErrors())
{
    Console.WriteLine($"{error.ErrorMessage}: {error.ErrorMessage}");
}
```

## Dependencies

- [System.Text.Json][NuGet System.Text.Json] (.NET Framework 4.7.2 only)

  A package that provides JSON serialization and deserialization.

## License

[Apache License Version 2.0][Apache License v2]

[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
[NuGet Maris.Core]:https://www.nuget.org/packages/Maris.Core
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[NuGet System.Text.Json]:https://www.nuget.org/packages/System.Text.Json/
