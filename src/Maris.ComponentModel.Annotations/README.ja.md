# Maris.ComponentModel.Annotations

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.ComponentModel.Annotations](https://img.shields.io/nuget/v/Maris.ComponentModel.Annotations?style=for-the-badge&logo=nuget)][NuGet Maris.ComponentModel.Annotations]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[English version](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.ComponentModel.Annotations/README.md)

`Maris.ComponentModel.Annotations` は `System.ComponentModel.Annotations` に対する追加機能を提供します。

## インストール方法

パッケージマネージャーコンソールまたはコマンドプロンプトで以下のコマンドを実行してインストールしてください。

- パッケージマネージャーコンソール

```winbatch
Install-Package Maris.ComponentModel.Annotations
```

- コマンドプロンプト

```bash
dotnet add package Maris.ComponentModel.Annotations
```

## 使用方法

### 列挙型の表示名取得

列挙型の各項目に対して設定した表示名を取得できます。
列挙型の各値に対する表示名は、 [`DisplayAttribute`][DisplayAttribute Web] を用いて設定します。
リソースファイルからも、表示名を取得できます。

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

表示名の取得は `GetDisplayName()` 拡張メソッドを利用します。
`Maris.ComponentModel` 名前空間を参照することで、 `GetDisplayName()` メソッドが利用できます。

### 日付の前後関係の検証

`DateComparisonAttribute` を用いて、プロパティの値と、同じオブジェクトの別のプロパティの値について、日付の前後関係を検証できます。
`Maris.ComponentModel.DataAnnotations` 名前空間を参照することで利用できます。

```csharp
using System.ComponentModel.DataAnnotations;
using Maris.ComponentModel.DataAnnotations;

public class Period
{
    [Display(Name = "開始日")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "終了日")]
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
Console.WriteLine(results[0].ErrorMessage); // Output: 終了日 は 開始日 以降にしてください。
```

比較方法は `DateComparisonOperator` 列挙型で指定します。
この列挙型には、以下の値があります。

- `After` ：検証対象の値が、比較先の値より後である場合に検証成功とします。
- `OnOrAfter` ：検証対象の値が、比較先の値と同じか、比較先の値より後である場合に検証成功とします。
- `Before` ：検証対象の値が、比較先の値より前である場合に検証成功とします。
- `OnOrBefore` ：検証対象の値が、比較先の値と同じか、比較先の値より前である場合に検証成功とします。

#### 対応する型

`DateTime` 、 `DateTimeOffset` 、 `DateOnly` とそれぞれの null 許容型、および `string` のプロパティに対応します。
`DateOnly` は .NET 10 以降でのみ利用できます。

日付型の値は、型を変換せずそのまま比較します。
検証対象の値と比較先の値は、同じ型である必要があります。
`DateTime` の `Kind` は考慮しないため、 UTC とローカル時刻が混在しないようにしてください。

#### string 型の値の比較

`string` の値は、日付型へ変換してから比較します。
変換先の型は `ConversionType` に、文字列のフォーマットは `Format` に設定します。

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

- `ConversionType` には `typeof(DateTime)` 、 `typeof(DateTimeOffset)` 、 `typeof(DateOnly)` のいずれかを設定できます。
  既定値は `typeof(DateTime)` です。
- オフセットを含む文字列を比較する場合は、 `ConversionType` に `typeof(DateTimeOffset)` を設定してください。
- `Format` を設定した場合、そのフォーマットに厳密に一致する文字列のみを、カルチャに依存せず変換します。
  設定しない場合、現在のカルチャのフォーマットに従って変換します。

#### 検証成功となる値

以下の場合は比較せず、検証成功とします。

- 検証対象の値、比較先の値のいずれかが `null` または空の文字列である場合。
  値が必須であることの検証には [`RequiredAttribute`][RequiredAttribute Web] を併用してください。
- 検証対象の値、比較先の値のいずれかが、日付型へ変換できない文字列である場合。
  `DateComparisonAttribute` は文字列のフォーマットを検証しません。
  フォーマットの検証には [`RegularExpressionAttribute`][RegularExpressionAttribute Web] などの検証属性を併用してください。

#### 複数のプロパティとの比較

`DateComparisonAttribute` は、同じプロパティに複数付与できます。

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

#### エラーメッセージ

既定のエラーメッセージは、日本語と英語を提供します。
現在の UI カルチャが英語の場合は英語、それ以外の場合は日本語のメッセージを使用します。
`ErrorMessage` プロパティ、または `ErrorMessageResourceType` プロパティと `ErrorMessageResourceName` プロパティを用いて、エラーメッセージを変更できます。
メッセージの `{0}` は検証対象のプロパティの表示名、 `{1}` は比較先のプロパティの表示名に置き換わります。

#### 設定に誤りがある場合

以下の場合は、検証時に `InvalidOperationException` をスローします。

- 比較先のプロパティが存在しない、公開されていない、またはインデクサーです。
- 検証対象の値または比較先の値が、対応していない型です。
- 検証対象の値と比較先の値の型が一致しません。
- `ConversionType` に対応していない型を設定しています。
- 比較方法に `DateComparisonOperator` で定義されていない値を設定しています。

## 主な依存ライブラリ

- [System.ComponentModel.Annotations][NuGet System.ComponentModel.Annotations]

  オブジェクトのメタデータを定義するための属性を提供するパッケージです。

## ライセンス

[Apache License Version 2.0][Apache License v2]

[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
[NuGet Maris.ComponentModel.Annotations]:https://www.nuget.org/packages/Maris.ComponentModel.Annotations
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[DisplayAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.displayattribute
[RequiredAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.requiredattribute
[RegularExpressionAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.regularexpressionattribute
[NuGet System.ComponentModel.Annotations]:https://www.nuget.org/packages/System.ComponentModel.Annotations/
