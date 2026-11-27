using Maris.Data.Testing.Xunit;

namespace Maris.Data.Testing.Tests.Xunit;

public sealed class ExecuteSqlDatabaseOptionsTest
{
    [Fact]
    public void Constructor_DefaultValues_AreNull()
    {
        // Act
        var options = new ExecuteSqlDatabaseOptions();

        // Assert
        Assert.Null(options.TestDataDirectory);
        Assert.Null(options.CommandTimeout);
    }

    [Theory]
    [InlineData("OrdersTestData")]
    [InlineData("TestData/Orders")]
    [InlineData("../OrdersTestData")]
    public void TestDataDirectory_RelativePath_ResolvesFromBaseDirectory(string directory)
    {
        // Arrange
        var options = new ExecuteSqlDatabaseOptions();
        var expected = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, directory));

        // Act
        options.TestDataDirectory = directory;

        // Assert
        Assert.Equal(expected, options.TestDataDirectory);
    }

    [Fact]
    public void TestDataDirectory_AbsolutePath_ReturnsSetValue()
    {
        // Arrange
        var options = new ExecuteSqlDatabaseOptions();
        var directory = Path.Combine(AppContext.BaseDirectory, "OrdersTestData");

        // Act
        options.TestDataDirectory = directory;

        // Assert
        Assert.Equal(directory, options.TestDataDirectory);
    }

    [Fact]
    public void TestDataDirectory_Null_ReturnsNull()
    {
        // Arrange
        var options = new ExecuteSqlDatabaseOptions { TestDataDirectory = "OrdersTestData" };

        // Act
        options.TestDataDirectory = null;

        // Assert
        Assert.Null(options.TestDataDirectory);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TestDataDirectory_EmptyOrWhiteSpace_ThrowsArgumentException(string directory)
    {
        // Arrange
        var options = new ExecuteSqlDatabaseOptions();

        // Act
        var action = () => options.TestDataDirectory = directory;

        // Assert
        Assert.Throws<ArgumentException>("value", action);
        Assert.Null(options.TestDataDirectory);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(600)]
    public void CommandTimeout_ValidValue_ReturnsSetValue(int? commandTimeout)
    {
        // Arrange
        var options = new ExecuteSqlDatabaseOptions();

        // Act
        options.CommandTimeout = commandTimeout;

        // Assert
        Assert.Equal(commandTimeout, options.CommandTimeout);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void CommandTimeout_NegativeValue_ThrowsArgumentOutOfRangeException(int commandTimeout)
    {
        // Arrange
        var options = new ExecuteSqlDatabaseOptions { CommandTimeout = 60 };

        // Act
        Action action = () => options.CommandTimeout = commandTimeout;

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>("value", action);
        Assert.Equal(60, options.CommandTimeout);
    }
}
