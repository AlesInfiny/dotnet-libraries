# Maris.Data.Testing

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.Data.Testing](https://img.shields.io/nuget/v/Maris.Data.Testing?style=for-the-badge&logo=nuget)][NuGet Maris.Data.Testing]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[English version](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.Data.Testing/README.md)

テストメソッドの実行前または実行後に SQL ファイルを実行し、テストデータの投入やクリーンアップを行う xUnit の属性を提供します。
ADO.NET の [`DbConnection`][DbConnection Web] のみに依存するため、 ADO.NET プロバイダーがあれば任意のデータベース製品で利用できます。

## インストール方法

パッケージマネージャーコンソールまたはコマンドプロンプトで以下のコマンドを実行してインストールしてください。

- パッケージマネージャーコンソール

```winbatch
Install-Package Maris.Data.Testing
```

- コマンドプロンプト

```bash
dotnet add package Maris.Data.Testing
```

## 使用方法

1. [xUnit v3 のテンプレートをインストール][Install xUnit template] し、 `dotnet new xunit3` でテストプロジェクトを作成します。
1. テストプロジェクトに `Maris.Data.Testing` と、使用するデータベースの ADO.NET プロバイダー（ `Microsoft.Data.SqlClient` など）をインストールします。
1. SQL ファイルをテストプロジェクトの `TestData` フォルダーに配置し、出力フォルダーにコピーされるよう設定します。

    ```xml title="*.csproj"
    <ItemGroup>
      <Content Include="TestData\**\*.sql" CopyToOutputDirectory="PreserveNewest" />
    </ItemGroup>
    ```

1. テストの実行前に、 `Maris.Data.Testing.Xunit.ExecuteSqlSettings.ConnectionFactory` へ、オープンしていない新しい `DbConnection` を作成する処理を設定します。
   [アセンブリフィクスチャー][xUnit assembly fixture] で設定すると、テストの実行前に一度だけ設定できます。

    ```csharp title="DatabaseFixture.cs"
    using Maris.Data.Testing.Xunit;
    using Microsoft.Data.SqlClient;
    using Xunit;

    [assembly: AssemblyFixture(typeof(Maris.Tests.DatabaseFixture))]

    namespace Maris.Tests;

    public class DatabaseFixture
    {
        public DatabaseFixture()
        {
            var connectionString = "...";
            ExecuteSqlSettings.ConnectionFactory = () => new SqlConnection(connectionString);
        }
    }
    ```

1. テストメソッドまたはテストクラスに `Maris.Data.Testing.Xunit.ExecuteSqlAttribute` を付与します。
   実行タイミング（ `SqlExecutionPhase.BeforeTest` または `SqlExecutionPhase.AfterTest` ）と、 `TestData` フォルダーからの相対パスで SQL ファイルを指定します。

    ```csharp title="TestClass1.cs"
    using Maris.Data.Testing.Xunit;
    using Xunit;

    namespace Maris.Tests;

    public class TestClass1
    {
        [Fact]
        [ExecuteSql(SqlExecutionPhase.BeforeTest, "Products/Insert.sql")]
        [ExecuteSql(SqlExecutionPhase.AfterTest, "Products/Delete.sql")]
        public void TestMethod()
        {
            // Products/Insert.sql で投入したデータを使用してテストできます。
            // Products/Delete.sql はテストメソッドが失敗した場合も実行されます。
        }
    }
    ```

## 動作仕様

- 1 つの属性で指定した SQL ファイルは、指定した順に 1 つのトランザクション内で実行します。
  いずれかの実行に失敗した場合はロールバックし、テストは失敗します。
- `SqlExecutionPhase.AfterTest` を指定した SQL ファイルは、テストメソッドが失敗した場合も実行します。
  ただし、 `SqlExecutionPhase.BeforeTest` を指定した SQL ファイルの実行に失敗した場合は、属性の記述順に関係なく実行しません。
  xUnit は、いずれかの属性の `Before` で例外が発生すると、テストメソッドもすべての属性の `After` も呼び出さないためです。
  このとき、失敗より前に別の属性で実行した SQL ファイルはコミット済みのため、投入したデータが残ります。

    ```csharp
    [ExecuteSql(SqlExecutionPhase.BeforeTest, "A.sql")]       // 成功してコミット済み
    [ExecuteSql(SqlExecutionPhase.BeforeTest, "B.sql")]       // 失敗してロールバック
    [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]  // 実行されず、 A.sql で投入したデータが残る
    ```

  `SqlExecutionPhase.BeforeTest` の SQL ファイルが失敗した場合にデータを残さないためには、複数の SQL ファイルを 1 つの属性にまとめて指定してください。
  1 つの属性で指定した SQL ファイルは 1 つのトランザクション内で実行するため、いずれかが失敗するとすべてロールバックされます。

    ```csharp
    [ExecuteSql(SqlExecutionPhase.BeforeTest, "A.sql", "B.sql")]  // B.sql が失敗すると A.sql もロールバック
    [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
    ```
- 各 SQL ファイルの内容は、 1 つのコマンドとして実行します。
  SQL Server の `GO` のような、データベース固有のバッチ区切りには対応していません。必要に応じてファイルを分割してください。
- 空白のみのファイルは実行しません。
- SQL ファイルの基準フォルダーは `ExecuteSqlSettings.TestDataDirectory` で変更できます。
  既定値は出力フォルダー直下の `TestData` フォルダーです。
  相対パスを設定した場合は、カレントディレクトリではなく出力フォルダーを基準に解決します。
- SQL ファイルを実行するコマンドのタイムアウト時間（秒）は `ExecuteSqlSettings.CommandTimeout` で変更できます。
  既定値は `null` で、 ADO.NET プロバイダーの既定値（ SqlClient の場合は 30 秒）を使用します。
  大量のテストデータを投入する場合は、必要に応じて大きな値を設定してください。

## 注意事項

xUnit は既定で、異なるテストコレクションに属するテストクラスを並列に実行します。
複数のテストクラスが同じテーブルを操作する場合は、それらのクラスを同じ [テストコレクション][xUnit collection] にまとめるか、並列実行を無効にしてください。

## 依存ライブラリ

- [xunit.v3.extensibility.core][NuGet xUnit v3]

  xUnit v3 の拡張機能を開発するためのパッケージです。
  `Maris.Data.Testing` は xUnit v3 を使用した単体テストプロジェクトでのみ利用できます。

## ライセンス

[Apache License Version 2.0][Apache License v2]

[DbConnection Web]:https://learn.microsoft.com/ja-jp/dotnet/api/system.data.common.dbconnection
[Install xUnit template]:https://xunit.net/docs/getting-started/v3/cmdline#install-the-net-sdk-templates
[xUnit assembly fixture]:https://xunit.net/docs/shared-context#assembly-fixture
[xUnit collection]:https://xunit.net/docs/running-tests-in-parallel
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[NuGet Maris.Data.Testing]:https://www.nuget.org/packages/Maris.Data.Testing
[NuGet xUnit v3]:https://www.nuget.org/packages/xunit.v3.extensibility.core/
[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
