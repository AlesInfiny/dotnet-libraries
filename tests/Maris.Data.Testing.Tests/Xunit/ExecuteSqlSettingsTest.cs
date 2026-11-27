using System.Data.Common;
using Maris.Data.Testing.Xunit;

namespace Maris.Data.Testing.Tests.Xunit;

[Collection(nameof(ExecuteSqlSettingsCollection))]
public sealed class ExecuteSqlSettingsTest : IDisposable
{
    private readonly Func<DbConnection>? originalConnectionFactory = ExecuteSqlSettings.ConnectionFactory;
    private readonly string originalTestDataDirectory = ExecuteSqlSettings.TestDataDirectory;
    private readonly int? originalCommandTimeout = ExecuteSqlSettings.CommandTimeout;

    public void Dispose()
    {
        ExecuteSqlSettings.ConnectionFactory = this.originalConnectionFactory;
        ExecuteSqlSettings.TestDataDirectory = this.originalTestDataDirectory;
        ExecuteSqlSettings.CommandTimeout = this.originalCommandTimeout;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(600)]
    public void CommandTimeout_ValidValue_ReturnsSetValue(int? commandTimeout)
    {
        // Act
        ExecuteSqlSettings.CommandTimeout = commandTimeout;

        // Assert
        Assert.Equal(commandTimeout, ExecuteSqlSettings.CommandTimeout);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void CommandTimeout_NegativeValue_ThrowsArgumentOutOfRangeException(int commandTimeout)
    {
        // Arrange
        ExecuteSqlSettings.CommandTimeout = 60;

        // Act
        Action action = () => ExecuteSqlSettings.CommandTimeout = commandTimeout;

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>("value", action);
        Assert.Equal(60, ExecuteSqlSettings.CommandTimeout);
    }

    [Fact]
    public void DefaultTestDataDirectory_IsTestDataFolderUnderBaseDirectory()
    {
        // Arrange
        var expected = Path.Combine(AppContext.BaseDirectory, "TestData");

        // Act
        var actual = ExecuteSqlSettings.DefaultTestDataDirectory;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestDataDirectory_ValidValue_ReturnsSetValue()
    {
        // Arrange
        var directory = Path.Combine(AppContext.BaseDirectory, "OtherTestData");

        // Act
        ExecuteSqlSettings.TestDataDirectory = directory;

        // Assert
        Assert.Equal(directory, ExecuteSqlSettings.TestDataDirectory);
    }

    [Theory]
    [InlineData("OtherTestData")]
    [InlineData("TestData/Sub")]
    [InlineData("../OtherTestData")]
    public void TestDataDirectory_RelativePath_ResolvesFromBaseDirectory(string directory)
    {
        // Arrange
        var expected = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, directory));

        // Act
        ExecuteSqlSettings.TestDataDirectory = directory;

        // Assert
        Assert.Equal(expected, ExecuteSqlSettings.TestDataDirectory);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TestDataDirectory_NullOrWhiteSpace_ThrowsArgumentException(string? directory)
    {
        // Arrange
        var original = ExecuteSqlSettings.TestDataDirectory;

        // Act
        var action = () => ExecuteSqlSettings.TestDataDirectory = directory!;

        // Assert
        Assert.Throws<ArgumentException>("value", action);
        Assert.Equal(original, ExecuteSqlSettings.TestDataDirectory);
    }

    [Fact]
    public void ConnectionFactory_SetValue_ReturnsSetValue()
    {
        // Arrange
        Func<DbConnection> factory = () => new FakeDbConnection();

        // Act
        ExecuteSqlSettings.ConnectionFactory = factory;

        // Assert
        Assert.Same(factory, ExecuteSqlSettings.ConnectionFactory);
    }
}
