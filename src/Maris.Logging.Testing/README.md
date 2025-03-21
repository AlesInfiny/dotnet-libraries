# Maris.Logging.Testing

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.Logging.Testing](https://img.shields.io/nuget/v/Maris.Logging.Testing?style=for-the-badge&logo=nuget)][NuGet Maris.Logging.Testing]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[`Microsoft.Extensions.Logging.ILogger`][ILogger Web] または [`Microsoft.Extensions.Logging.ILogger<TCategoryName>`][ILogger-T Web] の xUnit テストで利用可能なロガーを提供します。
このロガーを使用すると、テスト対象クラスで出力したログを、 Visual Studio のテストエクスプローラー上で確認できるようになります。
また [`Microsoft.Extensions.Logging.Testing.FakeLogger`][FakeLogger Web] と連携して、テストコード内でログ出力内容を検証する機能を提供します。

## インストール方法

パッケージマネージャーコンソールまたはコマンドプロンプトで以下のコマンドを実行してインストールしてください。

- パッケージマネージャーコンソール

```winbatch
Install-Package Maris.Logging.Testing
```

- コマンドプロンプト

```bash
dotnet add package Maris.Logging.Testing
```

## 使用方法

以下のような `ILogger<TCategoryName>` のオブジェクトを必要とするクラスに対して、 xUnit のテストコードを記述する場合を考えます。

```csharp title="TestTarget.cs"
using Microsoft.Extensions.Logging;

namespace Maris;

public class TestTarget
{
    private readonly ILogger<TestTarget> Logger;

    public TestTarget(ILogger<TestTarget> logger)
        => this.Logger = logger;
    
    // 省略
}
```

テストコードでテスト用のロガーを利用するためには、以下の手順で実装します。

1. [xUnit v3 のプロジェクトテンプレートをインストール][Install xUnit template] して、 `dotnet new xunit3` コマンドで xUnit v3 テストプロジェクトを作成します。
1. xUnit のテストプロジェクトに `Maris.Logging.Testing` のパッケージをインストールします。
1. テストクラスのコンストラクターを定義し、 `Xunit.ITestOutputHelper` インターフェースのオブジェクトを引数に取ります。
1. コンストラクターで `Maris.Logging.Testing.Xunit.TestLoggerManager` のオブジェクトを生成し、フィールドに保存します。
   `TestLoggerManager` のコンストラクターには `ITestOutputHelper` のオブジェクトを渡します。
1. テストメソッド内で `TestLoggerManager` の `CreateLogger` メソッドを呼び出して、テスト対象クラスに渡す `ILogger` または `ILogger<TCategoryName>` のオブジェクトを生成します。

```csharp title="TestClass1.cs"
using Maris.Logging.Testing.Xunit;
using Xunit;

namespace Maris.Tests;

public class TestClass1
{
    private readonly TestLoggerManager loggerManager;

    public TestClass1(ITestOutputHelper testOutputHelper)
        => this.loggerManager = new TestLoggerManager(testOutputHelper);

    [Fact]
    public void TestMethod()
    {
        // ILogger<T> のオブジェクトを生成可能。
        var logger = this.loggerManager.CreateLogger<TestTarget>();
        var target = new TestTarget(logger);

        // Do something...

        // TestLoggerManagerのLogCollectorプロパティはFakeLogger.Collectorを公開します
        Assert.Equal(1, this.loggerManager.LogCollector.Count);  
        Assert.Equal("expectedCategory", this.loggerManager.LogCollector.LatestRecord.Category);
        Assert.Equal("expectedMessage", this.loggerManager.LogCollector.LatestRecord.Message);
    }
}
```

[`Microsoft.Extensions.Hosting.IHost`][IHost Web] のテストを行う際は、 `AddTestLogging` メソッドを利用してテスト用のロガーを DI コンテナーに登録できます。
`AddTestLogging` メソッドは `Microsoft.Extensions.DependencyInjection.TestLoggerServiceCollectionExtensions` クラスに定義されています。
xUnit のテストコード例は以下のとおりです。

```csharp title="TestClass2.cs"
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Maris.Logging.Testing.Xunit;
using Xunit;

namespace Maris.Tests;

public class TestClass2
{
    private readonly TestLoggerManager loggerManager;

    public TestClass2(ITestOutputHelper testOutputHelper)
        => this.loggerManager = new TestLoggerManager(testOutputHelper);

    [Fact]
    public async Task TestMethod()
    {
        using var app = this.CreateHost();
        var cancellationToken = TestContext.Current.CancellationToken;
        await app.RunAsync(cancellationToken);
        
        // Do something...
    }

    private IHost CreateHost()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices((context, services) =>
        {
            // テスト用の Logger を登録
            services.AddTestLogging(this.loggerManager);

            // Do something...            
        });

        return builder.Build();
    }
}
```

テストエクスプローラーではテスト実行中に出力されたログが以下のように表示されます。

![test-explorer-log][Test explorer log image]

詳細は [ASP.NET Core での統合テスト][ASP.NET Core integration test] を参照してください。

## 主な依存ライブラリ

- [xunit.v3.extensibility.core][NuGet xUnit v3]

  xUnit v3 の拡張機能を開発するためのパッケージです。
  このライブラリは xUnit v3 で開発されたテストプロジェクトでのみ利用できます。

- [Microsoft.Extensions.Diagnostics.Testing][NuGet Diagnostics.Testing]

  ログ出力のテストを行うためのライブラリです。

## ライセンス

[Apache License Version 2.0][Apache License v2]

[IHost Web]:https://learn.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.ihost
[ILogger Web]:https://learn.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.logging.ilogger
[ILogger-T Web]:https://learn.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.logging.ilogger-1
[FakeLogger Web]:https://learn.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.logging.testing.fakelogger
[Install xUnit template]:https://xunit.net/docs/getting-started/v3/cmdline#install-the-net-sdk-templates
[Test explorer log image]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/images/test-explorer-log.png
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[NuGet Maris.Logging.Testing]:https://www.nuget.org/packages/Maris.Logging.Testing
[NuGet xUnit v3]:https://www.nuget.org/packages/xunit.v3.extensibility.core/
[NuGet Diagnostics.Testing]:https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.Testing
[ASP.NET Core integration test]:https://learn.microsoft.com/ja-jp/aspnet/core/test/integration-tests
[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
