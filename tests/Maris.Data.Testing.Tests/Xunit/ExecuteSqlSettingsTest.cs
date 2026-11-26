using System.Data.Common;
using Maris.Data.Testing.Xunit;

namespace Maris.Data.Testing.Tests.Xunit;

[Collection(nameof(ExecuteSqlSettingsCollection))]
public sealed class ExecuteSqlSettingsTest : IDisposable
{
    private readonly Func<DbConnection>? originalConnectionFactory = ExecuteSqlSettings.ConnectionFactory;
    private readonly string originalTestDataDirectory = ExecuteSqlSettings.TestDataDirectory;

    public void Dispose()
    {
        ExecuteSqlSettings.ConnectionFactory = this.originalConnectionFactory;
        ExecuteSqlSettings.TestDataDirectory = this.originalTestDataDirectory;
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
