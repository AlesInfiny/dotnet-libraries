<!-- textlint-disable ja-technical-writing/sentence-length -->
# Maris.ComponentModel.Annotations

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.ComponentModel.Annotations](https://img.shields.io/nuget/v/Maris.ComponentModel.Annotations?style=for-the-badge&logo=nuget)][NuGet Maris.ComponentModel.Annotations]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[日本語版](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.ComponentModel.Annotations/README.ja.md)

`Maris.ComponentModel.Annotations` provides additional functionality for `System.ComponentModel.Annotations`.

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

### Validate the order of dates

`DateComparisonAttribute` validates the chronological order between the value of a property and the value of another property of the same object.
It becomes available by referencing the `Maris.ComponentModel.DataAnnotations` namespace.

```csharp
using System.ComponentModel.DataAnnotations;
using Maris.ComponentModel.DataAnnotations;

public class Period
{
    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "End date")]
    [DateComparison(nameof(StartDate), DateComparisonOperator.OnOrAfter)]
    public DateTime? EndDate { get; set; }
}

var period = new Period
{
    StartDate = new DateTime(2026, 11, 12),
    EndDate = new DateTime(2026, 11, 11),
};
var results = new List<ValidationResult>();
Validator.TryValidateObject(period, new ValidationContext(period), results, validateAllProperties: true);
Console.WriteLine(results[0].ErrorMessage); // Output: End date must be on or after Start date.
```

Specify how to compare with the `DateComparisonOperator` enumeration.

- `After`: Validation succeeds when the validated value is after the other value.
- `OnOrAfter`: Validation succeeds when the validated value is the same as or after the other value.
- `Before`: Validation succeeds when the validated value is before the other value.
- `OnOrBefore`: Validation succeeds when the validated value is the same as or before the other value.

#### Supported types

Properties of `DateTime`, `DateTimeOffset`, `DateOnly`, their nullable types, and `string` are supported.
`DateOnly` is available only on .NET 10 or later.

Date values are compared as they are, without type conversion.
The validated value and the other value must be of the same type.
`Kind` of `DateTime` is not taken into account, so do not mix UTC and local time.

#### Comparing string values

`string` values are converted to a date type before comparison.
Set the destination type to `ConversionType` and the format of the string to `Format`.

```csharp
public class SearchCondition
{
    public DateOnly? StartDate { get; set; }

    [DateComparison(
        nameof(StartDate),
        DateComparisonOperator.OnOrAfter,
        ConversionType = typeof(DateOnly),
        Format = "yyyy/MM/dd")]
    public string? EndDate { get; set; }
}
```

- `ConversionType` accepts `typeof(DateTime)`, `typeof(DateTimeOffset)`, or `typeof(DateOnly)`.
  The default value is `typeof(DateTime)`.
- To compare strings that contain an offset, set `ConversionType` to `typeof(DateTimeOffset)`.
- When `Format` is set, only strings that exactly match the format are converted, independently of the culture.
  When it is not set, strings are converted according to the format of the current culture.

#### Values that pass validation

In the following cases, no comparison is performed and validation succeeds.

- Either the validated value or the other value is `null` or an empty string.
  Use [RequiredAttribute][RequiredAttribute Web] together to validate that a value is required.
- Either the validated value or the other value is a string that cannot be converted to the date type.
  `DateComparisonAttribute` does not validate the format of strings.
  Use validation attributes such as [RegularExpressionAttribute][RegularExpressionAttribute Web] together to validate the format.

#### Comparing with multiple properties

`DateComparisonAttribute` can be applied to the same property more than once.

```csharp
public class Schedule
{
    public DateTime? StartDate { get; set; }

    [DateComparison(nameof(StartDate), DateComparisonOperator.After)]
    [DateComparison(nameof(EndDate), DateComparisonOperator.Before)]
    public DateTime? CheckDate { get; set; }

    public DateTime? EndDate { get; set; }
}
```

#### Error messages

Default error messages are provided in Japanese and English.
The English message is used when the current UI culture is English; otherwise, the Japanese message is used.
You can change the error message with the `ErrorMessage` property, or with the `ErrorMessageResourceType` and `ErrorMessageResourceName` properties.
In the message, `{0}` is replaced with the display name of the validated property, and `{1}` with the display name of the other property.

#### When the configuration is incorrect

In the following cases, `InvalidOperationException` is thrown during validation.

- The other property does not exist, is not public, or is an indexer.
- The validated value or the other value is of an unsupported type.
- The types of the validated value and the other value do not match.
- `ConversionType` is set to an unsupported type.
- The operator is set to a value that is not defined in `DateComparisonOperator`.

## Dependencies

- [System.ComponentModel.Annotations][NuGet System.ComponentModel.Annotations]

  `System.ComponentModel.Annotations` that provides attributes for defining object metadata.

## License

[Apache License Version 2.0][Apache License v2]

[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
[NuGet Maris.ComponentModel.Annotations]:https://www.nuget.org/packages/Maris.ComponentModel.Annotations
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[DisplayAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.displayattribute
[RequiredAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.requiredattribute
[RegularExpressionAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.regularexpressionattribute
[NuGet System.ComponentModel.Annotations]:https://www.nuget.org/packages/System.ComponentModel.Annotations/
