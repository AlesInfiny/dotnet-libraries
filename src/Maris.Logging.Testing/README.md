<!-- textlint-disable ja-technical-writing/sentence-length -->
# Maris.Logging.Testing

[![Apache License v2](https://img.shields.io/github/license/AlesInfiny/dotnet-libraries?style=for-the-badge&color=purple)][Apache License v2]
[![Maris.Logging.Testing](https://img.shields.io/nuget/v/Maris.Logging.Testing?style=for-the-badge&logo=nuget)][NuGet Maris.Logging.Testing]
[![Release date](https://img.shields.io/github/release-date/AlesInfiny/dotnet-libraries?style=for-the-badge&logo=github)][GitHub Release]

[日本語版](https://github.com/AlesInfiny/dotnet-libraries/blob/main/src/Maris.Logging.Testing/README.ja.md)

This library provides [`Microsoft.Extensions.Logging.ILogger`][ILogger Web] and [`Microsoft.Extensions.Logging.ILogger<TCategoryName>`][ILogger-T Web] loggers which can be used in xUnit tests.
You can check the log output from test classes on Visual Studio Test Explorer.
This library also provides the functionality to verify the log output within the test code by integrating [`Microsoft.Extensions.Logging.Testing.FakeLogger`][FakeLogger Web].

## Install

Run the following command in Package Manager Console or Command Prompt to install `Maris.Logging.Testing`.

- Package Manager Console

```winbatch
Install-Package Maris.Logging.Testing
```

- Command Prompt

```bash
dotnet add package Maris.Logging.Testing
```

## Usage

Suppose you have a class that requires an `ILogger<TCategoryName>` object as shown below, and need to write xUnit test code for this class.

```csharp title="TestTarget.cs"
using Microsoft.Extensions.Logging;

namespace Maris;

public class TestTarget
{
    private readonly ILogger<TestTarget> Logger;

    public TestTarget(ILogger<TestTarget> logger)
        => this.Logger = logger;
    
    // ...
}
```

Follow these steps to use a test logger in the test code.

1. [Install template for xUnit v3][Install xUnit template], and run `dotnet new xunit3` to create a unit test project.
1. Install `Maris.Logging.Testing` in the unit test project.
1. In the test class, define a constructor with a `Xunit.ITestOutputHelper` parameter.
1. Create a `Maris.Logging.Testing.Xunit.TestLoggerManager` instance in the constructor, and store it in a field. Pass an `ITestOutputHelper` instance to the constructor of `TestLoggerManager`.
1. Call `CreateLogger` method of `TestLoggerManager` in the test method to create an `ILogger` or `ILogger<TCategoryName>` instance which can be passed to the test class.

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
        // Creates ILogger<T> instance
        var logger = this.loggerManager.CreateLogger<TestTarget>();
        var target = new TestTarget(logger);

        // Do something...

        // TestLoggerManager.LogCollector exposes FakeLogger.Collector
        Assert.Equal(1, this.loggerManager.LogCollector.Count);  
        Assert.Equal("expectedCategory", this.loggerManager.LogCollector.LatestRecord.Category);
        Assert.Equal("expectedMessage", this.loggerManager.LogCollector.LatestRecord.Message);
    }
}
```

When testing [`Microsoft.Extensions.Hosting.IHost`][IHost Web], `AddTestLogging` method can be used to register a test logger in the DI container.
`AddTestLogging` method is defined in `Microsoft.Extensions.DependencyInjection.TestLoggerServiceCollectionExtensions` class.
Here is an example of xUnit test code.

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
            // Register a test logger
            services.AddTestLogging(this.loggerManager);

            // Do something...            
        });

        return builder.Build();
    }
}
```

The log output during the test execution is displayed in Test Explorer as follows.

![test-explorer-log][Test explorer log image]

For more details, see [Integration tests in ASP.NET Core][ASP.NET Core integration test].

## Dependencies

- [xunit.v3.extensibility.core][NuGet xUnit v3]

  The package used for developing xUnit v3 extensions.
  `Maris.Logging.Testing` only supports the unit test project with xUnit v3.

- [Microsoft.Extensions.Diagnostics.Testing][NuGet Diagnostics.Testing]

  The library for testing log output.

## License

[Apache License Version 2.0][Apache License v2]

[IHost Web]:https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost
[ILogger Web]:https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger
[ILogger-T Web]:https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger-1
[FakeLogger Web]:https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.testing.fakelogger
[Install xUnit template]:https://xunit.net/docs/getting-started/v3/cmdline#install-the-net-sdk-templates
[Test explorer log image]:https://raw.githubusercontent.com/AlesInfiny/dotnet-libraries/refs/heads/main/images/test-explorer-log.en.png
[GitHub Release]:https://github.com/AlesInfiny/dotnet-libraries/releases
[NuGet Maris.Logging.Testing]:https://www.nuget.org/packages/Maris.Logging.Testing
[NuGet xUnit v3]:https://www.nuget.org/packages/xunit.v3.extensibility.core/
[NuGet Diagnostics.Testing]:https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.Testing
[ASP.NET Core integration test]:https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
[Apache License v2]:https://github.com/AlesInfiny/dotnet-libraries/blob/main/LICENSE
