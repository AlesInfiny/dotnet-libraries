# Maris.Core

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.Core](https://img.shields.io/nuget/v/Maris.Core?style=for-the-badge&logo=nuget)][NuGet Maris.Core]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[English version](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.Core/README.md)

.NET ビジネスアプリケーション開発のためのコアライブラリを提供します。

## インストール方法

パッケージマネージャーコンソールまたはコマンドプロンプトで以下のコマンドを実行してインストールしてください。

- パッケージマネージャーコンソール

```winbatch
Install-Package Maris.Core
```

- コマンドプロンプト

```bash
dotnet add package Maris.Core
```

## 使用方法

### 業務エラーの処理

業務ロジックで発生するエラーを構造化して扱うことができます。
`BusinessError` クラスでエラー情報を定義し、`BusinessException` 例外クラスでスローします。

```csharp
using Maris.Core;

// エラーメッセージの作成
var errorMessage1 = new ErrorMessage("商品コード {0} は存在しません。", "P001");
var errorMessage2 = new ErrorMessage("在庫が不足しています。");

// 業務エラーの作成
var businessError = new BusinessError("ERR001", errorMessage1, errorMessage2);

// 業務例外のスロー
throw new BusinessException(businessError);
```

複数の業務エラーをまとめて扱うこともできます。

```csharp
using Maris.Core;

// 複数の業務エラーを作成
var error1 = new BusinessError("ERR001", new ErrorMessage("入力エラーです。"));
var error2 = new BusinessError("ERR002", new ErrorMessage("検証エラーです。"));

// 複数のエラーをまとめてスロー
var exception = new BusinessException(error1, error2);
throw exception;

// エラー情報の取得
foreach (var error in exception.GetErrors())
{
    Console.WriteLine($"{error.ExceptionId}: {error.ErrorMessage}");
}
```

## 主な依存ライブラリ

- [System.Text.Json][NuGet System.Text.Json] (.NET Framework 4.7.2 のみ)

  JSON のシリアル化・デシリアル化を提供するパッケージです。

## ライセンス

[Apache License Version 2.0][Apache License v2]

[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
[NuGet Maris.Core]:https://www.nuget.org/packages/Maris.Core
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[NuGet System.Text.Json]:https://www.nuget.org/packages/System.Text.Json/
