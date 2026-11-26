<!-- textlint-disable ja-technical-writing/sentence-length -->
# Maris.Data.Testing

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.Data.Testing](https://img.shields.io/nuget/v/Maris.Data.Testing?style=for-the-badge&logo=nuget)][NuGet Maris.Data.Testing]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[日本語版](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.Data.Testing/README.ja.md)

This library provides an xUnit attribute that executes SQL files before or after each test method, in order to set up and clean up test data.
It depends only on [ADO.NET `DbConnection`][DbConnection Web], so it can be used with any database product that has an ADO.NET provider.

## Install

Run the following command in Package Manager Console or Command Prompt to install `Maris.Data.Testing`.

- Package Manager Console

```winbatch
Install-Package Maris.Data.Testing
```

- Command Prompt

```bash
dotnet add package Maris.Data.Testing
```

## Usage

1. [Install template for xUnit v3][Install xUnit template], and run `dotnet new xunit3` to create a unit test project.
1. Install `Maris.Data.Testing` and the ADO.NET provider of your database (e.g. `Microsoft.Data.SqlClient`) in the unit test project.
1. Put the SQL files in the `TestData` folder of the test project, and copy them to the output directory.

    ```xml title="*.csproj"
    <ItemGroup>
      <Content Include="TestData\**\*.sql" CopyToOutputDirectory="PreserveNewest" />
    </ItemGroup>
    ```

1. Before any test runs, set `Maris.Data.Testing.Xunit.ExecuteSqlSettings.ConnectionFactory` to a function that creates a new, unopened `DbConnection`.
   An [assembly fixture][xUnit assembly fixture] is a good place to do this.

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

1. Add `Maris.Data.Testing.Xunit.ExecuteSqlAttribute` to the test method or the test class.
   Specify the timing (`SqlExecutionPhase.BeforeTest` or `SqlExecutionPhase.AfterTest`) and the paths of the SQL files relative to the `TestData` folder.

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
            // Test data inserted by Products/Insert.sql is available here.
            // Products/Delete.sql is executed after the test, even if the test fails.
        }
    }
    ```

## Behavior

- The SQL files specified in one attribute are executed in the specified order within one transaction.
  If any of them fails, the transaction is rolled back and the test fails.
- The content of each SQL file is executed as a single command.
  Database-specific batch separators such as `GO` in SQL Server are not supported. Split such SQL into separate files.
- Files containing only white space are skipped.
- The base folder of the SQL files can be changed by `ExecuteSqlSettings.TestDataDirectory`.
  The default is the `TestData` folder in the output directory.

## Notes

xUnit runs test classes in different test collections in parallel by default.
If the tests in different classes manipulate the same tables, put those classes in the same [test collection][xUnit collection], or disable parallelization.

## Dependencies

- [xunit.v3.extensibility.core][NuGet xUnit v3]

  The package used for developing xUnit v3 extensions.
  `Maris.Data.Testing` only supports the unit test project with xUnit v3.

## License

[Apache License Version 2.0][Apache License v2]

[DbConnection Web]:https://learn.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection
[Install xUnit template]:https://xunit.net/docs/getting-started/v3/cmdline#install-the-net-sdk-templates
[xUnit assembly fixture]:https://xunit.net/docs/shared-context#assembly-fixture
[xUnit collection]:https://xunit.net/docs/running-tests-in-parallel
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[NuGet Maris.Data.Testing]:https://www.nuget.org/packages/Maris.Data.Testing
[NuGet xUnit v3]:https://www.nuget.org/packages/xunit.v3.extensibility.core/
[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
