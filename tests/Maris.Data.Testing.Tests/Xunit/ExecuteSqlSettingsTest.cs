using System.Data.Common;
using Maris.Data.Testing.Xunit;

namespace Maris.Data.Testing.Tests.Xunit;

[Collection(nameof(ExecuteSqlSettingsCollection))]
public sealed class ExecuteSqlSettingsTest : IDisposable
{
    private readonly Func<DbConnection>? originalConnectionFactory = ExecuteSqlSettings.ConnectionFactory;
    private readonly string originalTestDataDirectory = ExecuteSqlSettings.TestDataDirectory;
    private readonly int? originalCommandTimeout = ExecuteSqlSettings.CommandTimeout;
    private readonly List<string> addedDatabaseNames = [];

    public void Dispose()
    {
        ExecuteSqlSettings.ConnectionFactory = this.originalConnectionFactory;
        ExecuteSqlSettings.TestDataDirectory = this.originalTestDataDirectory;
        ExecuteSqlSettings.CommandTimeout = this.originalCommandTimeout;
        foreach (var name in this.addedDatabaseNames)
        {
            ExecuteSqlSettings.RemoveDatabase(name);
        }
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
    public void TestDataDirectory_NotSet_ReturnsDefaultTestDataDirectory()
    {
        // ExecuteSqlSettings を変更するテストは同じコレクションで直列に実行され、終了時に設定を元に戻す。
        // そのため、ここで取得する値は静的フィールドの初期値になる。

        // Act
        var actual = ExecuteSqlSettings.TestDataDirectory;

        // Assert
        Assert.Equal(ExecuteSqlSettings.DefaultTestDataDirectory, actual);
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddDatabase_NullOrWhiteSpaceName_ThrowsArgumentException(string? name)
    {
        // Act
        var action = () => ExecuteSqlSettings.AddDatabase(name!, () => new FakeDbConnection());

        // Assert
        Assert.Throws<ArgumentException>("name", action);
    }

    [Fact]
    public void AddDatabase_ConnectionFactoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var action = () => ExecuteSqlSettings.AddDatabase("Orders", null!);

        // Assert
        Assert.Throws<ArgumentNullException>("connectionFactory", action);
    }

    [Fact]
    public void AddDatabase_DuplicateName_ThrowsArgumentExceptionAndKeepsRegisteredDatabase()
    {
        // Arrange
        Func<DbConnection> factory = () => new FakeDbConnection();
        this.AddDatabase("Orders", factory);

        // Act
        var action = () => ExecuteSqlSettings.AddDatabase("Orders", () => new FakeDbConnection());

        // Assert
        var exception = Assert.Throws<ArgumentException>("name", action);
        Assert.Contains("Orders", exception.Message);
        Assert.Same(factory, ExecuteSqlSettings.GetDatabase("Orders").ConnectionFactory);
    }

    [Fact]
    public void AddDatabase_NamesDifferOnlyInCase_RegistersAsDifferentDatabases()
    {
        // Arrange
        Func<DbConnection> upperFactory = () => new FakeDbConnection();
        Func<DbConnection> lowerFactory = () => new FakeDbConnection();

        // Act
        this.AddDatabase("Orders", upperFactory);
        this.AddDatabase("orders", lowerFactory);

        // Assert
        Assert.Same(upperFactory, ExecuteSqlSettings.GetDatabase("Orders").ConnectionFactory);
        Assert.Same(lowerFactory, ExecuteSqlSettings.GetDatabase("orders").ConnectionFactory);
    }

    [Fact]
    public void AddDatabase_ExceptionInConfigure_DoesNotRegisterDatabase()
    {
        // Act
        var action = () => this.AddDatabase("Orders", () => new FakeDbConnection(), options => options.CommandTimeout = -1);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
        Assert.False(ExecuteSqlSettings.RemoveDatabase("Orders"));
    }

    [Fact]
    public void RemoveDatabase_RegisteredName_RemovesDatabaseAndReturnsTrue()
    {
        // Arrange
        this.AddDatabase("Orders", () => new FakeDbConnection());

        // Act
        var actual = ExecuteSqlSettings.RemoveDatabase("Orders");

        // Assert
        Assert.True(actual);
        Assert.Throws<InvalidOperationException>(() => ExecuteSqlSettings.GetDatabase("Orders"));
    }

    [Fact]
    public void RemoveDatabase_NotRegisteredName_ReturnsFalse()
    {
        // Act
        var actual = ExecuteSqlSettings.RemoveDatabase("NotRegistered");

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void RemoveDatabase_NameIsNull_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => ExecuteSqlSettings.RemoveDatabase(null!);

        // Assert
        Assert.Throws<ArgumentNullException>("name", action);
    }

    [Fact]
    public void GetDatabase_NameIsNull_ReturnsDefaultDatabaseSettings()
    {
        // Arrange
        Func<DbConnection> factory = () => new FakeDbConnection();
        ExecuteSqlSettings.ConnectionFactory = factory;
        ExecuteSqlSettings.TestDataDirectory = "DefaultTestData";
        ExecuteSqlSettings.CommandTimeout = 60;

        // Act
        var actual = ExecuteSqlSettings.GetDatabase(null);

        // Assert
        Assert.Same(factory, actual.ConnectionFactory);
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "DefaultTestData"), actual.TestDataDirectory);
        Assert.Equal(60, actual.CommandTimeout);
    }

    [Fact]
    public void GetDatabase_NameIsNullAndConnectionFactoryIsNotSet_ThrowsInvalidOperationException()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = null;

        // Act
        var action = () => ExecuteSqlSettings.GetDatabase(null);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(nameof(ExecuteSqlSettings.ConnectionFactory), exception.Message);
    }

    [Fact]
    public void GetDatabase_NamedDatabaseWithoutOptions_UsesCurrentGlobalSettings()
    {
        // Arrange
        Func<DbConnection> factory = () => new FakeDbConnection();
        this.AddDatabase("Orders", factory);

        // 登録後に変更した全体の設定も反映されることを確認する。
        ExecuteSqlSettings.TestDataDirectory = "DefaultTestData";
        ExecuteSqlSettings.CommandTimeout = 60;

        // Act
        var actual = ExecuteSqlSettings.GetDatabase("Orders");

        // Assert
        Assert.Same(factory, actual.ConnectionFactory);
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "DefaultTestData"), actual.TestDataDirectory);
        Assert.Equal(60, actual.CommandTimeout);
    }

    [Fact]
    public void GetDatabase_NamedDatabaseWithOptions_UsesDatabaseSettings()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = null;
        ExecuteSqlSettings.TestDataDirectory = "DefaultTestData";
        ExecuteSqlSettings.CommandTimeout = 60;
        this.AddDatabase("Orders", () => new FakeDbConnection(), options =>
        {
            options.TestDataDirectory = "OrdersTestData";
            options.CommandTimeout = 0;
        });

        // Act
        var actual = ExecuteSqlSettings.GetDatabase("Orders");

        // Assert
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "OrdersTestData"), actual.TestDataDirectory);
        Assert.Equal(0, actual.CommandTimeout);
    }

    [Fact]
    public void GetDatabase_OptionsChangedAfterAddDatabase_IsNotAffected()
    {
        // Arrange
        ExecuteSqlDatabaseOptions? capturedOptions = null;
        this.AddDatabase("Orders", () => new FakeDbConnection(), options =>
        {
            options.CommandTimeout = 120;
            capturedOptions = options;
        });
        capturedOptions!.CommandTimeout = 300;

        // Act
        var actual = ExecuteSqlSettings.GetDatabase("Orders");

        // Assert
        Assert.Equal(120, actual.CommandTimeout);
    }

    [Fact]
    public void GetDatabase_NotRegisteredName_ThrowsInvalidOperationExceptionWithRegisteredNames()
    {
        // Arrange
        this.AddDatabase("Orders", () => new FakeDbConnection());
        this.AddDatabase("Inventory", () => new FakeDbConnection());

        // Act
        var action = () => ExecuteSqlSettings.GetDatabase("Order");

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("'Order'", exception.Message);
        Assert.Contains("'Inventory', 'Orders'", exception.Message);
    }

    private void AddDatabase(string name, Func<DbConnection> connectionFactory, Action<ExecuteSqlDatabaseOptions>? configure = null)
    {
        this.addedDatabaseNames.Add(name);
        ExecuteSqlSettings.AddDatabase(name, connectionFactory, configure);
    }
}
