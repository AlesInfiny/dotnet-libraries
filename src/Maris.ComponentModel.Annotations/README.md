<!-- textlint-disable ja-technical-writing/sentence-length -->
# Maris.ComponentModel.Annotations

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.ComponentModel.Annotations](https://img.shields.io/nuget/v/Maris.ComponentModel.Annotations?style=for-the-badge&logo=nuget)][NuGet Maris.ComponentModel.Annotations]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[日本語版](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.ComponentModel.Annotations/README.ja.md)

Provides common libraries useful for .NET enterprise system development.

## Install

Run the following command in Package Manager Console or Command Prompt to install `Maris.ComponentModel.Annotations`.

- Package Manager Console

```winbatch
Install-Package Maris.ComponentModel.Annotations
```

- Command Prompt

```bash
dotnet add package Maris.ComponentModel.Annotations
```

## Usage

### Retrieve display name of enumeration

You can retrieve the display name configured for each item of an enumeration.
Display names for each enumeration value are set using [DisplayAttribute][DisplayAttribute Web].
It is also possible to define display names in resource files.

```csharp
using Maris.ComponentModel;

public enum Status
{
    [Display(Name = "Preparation is ready.")]
    Ready = 1,

    [Display(
        ResourceType = typeof(EnumExtensionsTestResources),
        Name = nameof(EnumExtensionsTestResources.InProgress))]
    InProgress = 2,

    Done = 3,
}

Console.WriteLine(Status.Ready.GetDisplayName());      // Output: Preparation is ready.
Console.WriteLine(Status.InProgress.GetDisplayName()); // Output: Work is in progress.
Console.WriteLine(Status.Done.GetDisplayName());       // Output: Done
```

Display names are retrieved using the `GetDisplayName()` extension method.
By referencing the `Maris.ComponentModel` namespace, the `GetDisplayName()` method becomes available.

## Dependencies

- [System.ComponentModel.Annotations][NuGet System.ComponentModel.Annotations]

  `System.ComponentModel.Annotations` that provides attributes for defining object metadata.

## License

[Apache License Version 2.0][Apache License v2]

[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
[NuGet Maris.ComponentModel.Annotations]:https://www.nuget.org/packages/Maris.ComponentModel.Annotations
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[DisplayAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.displayattribute
[NuGet System.ComponentModel.Annotations]:https://www.nuget.org/packages/System.ComponentModel.Annotations/
