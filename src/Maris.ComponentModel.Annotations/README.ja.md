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

## 主な依存ライブラリ

- [System.ComponentModel.Annotations][NuGet System.ComponentModel.Annotations]

  オブジェクトのメタデータを定義するための属性を提供するパッケージです。

## ライセンス

[Apache License Version 2.0][Apache License v2]

[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
[NuGet Maris.ComponentModel.Annotations]:https://www.nuget.org/packages/Maris.ComponentModel.Annotations
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[DisplayAttribute Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.componentmodel.dataannotations.displayattribute
[NuGet System.ComponentModel.Annotations]:https://www.nuget.org/packages/System.ComponentModel.Annotations/
