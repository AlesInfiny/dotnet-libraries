using System.Data.Common;
using System.Reflection;
using Maris.Data.Testing.Xunit;
using Xunit.v3;

namespace Maris.Data.Testing.Tests.Xunit;

[Collection(nameof(ExecuteSqlSettingsCollection))]
public sealed class ExecuteSqlAttributeTest : IDisposable
{
    private const string FirstSql = "INSERT INTO Items VALUES (1);";
    private const string SecondSql = "INSERT INTO Items VALUES (2);";
    private const string NestedSql = "DELETE FROM Items;";
    private const string OrdersDatabase = "Orders";

    private static readonly MethodInfo MethodUnderTest = typeof(ExecuteSqlAttributeTest).GetMethod(nameof(Dispose))!;

    private readonly Func<DbConnection>? originalConnectionFactory = ExecuteSqlSettings.ConnectionFactory;
    private readonly string originalTestDataDirectory = ExecuteSqlSettings.TestDataDirectory;
    private readonly int? originalCommandTimeout = ExecuteSqlSettings.CommandTimeout;
    private readonly IXunitTest test = Mock.Of<IXunitTest>();
    private readonly List<FakeDbConnection> connections = [];
    private readonly List<FakeDbConnection> ordersConnections = [];
    private readonly List<string> addedDatabaseNames = [];

    public ExecuteSqlAttributeTest()
    {
        ExecuteSqlSettings.TestDataDirectory = ExecuteSqlSettings.DefaultTestDataDirectory;
        ExecuteSqlSettings.CommandTimeout = null;
        ExecuteSqlSettings.ConnectionFactory = () =>
        {
            var connection = new FakeDbConnection();
            this.connections.Add(connection);
            return connection;
        };
    }

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

    [Fact]
    public void Constructor_SetsPhaseAndSqlFiles()
    {
        // Act
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.AfterTest, "First.sql", "Second.sql");

        // Assert
        Assert.Equal(SqlExecutionPhase.AfterTest, attribute.Phase);
        Assert.Equal(["First.sql", "Second.sql"], attribute.SqlFiles);
        Assert.Null(attribute.Database);
    }

    [Fact]
    public void Constructor_SqlFilesIsNull_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, null!);

        // Assert
        Assert.Throws<ArgumentNullException>("sqlFiles", action);
    }

    [Fact]
    public void Before_BeforeTestPhase_ExecutesSqlFilesInOrderAndCommits()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "Second.sql", "First.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal(1, connection.OpenCount);
        Assert.Equal([SecondSql, FirstSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
        var transaction = Assert.Single(connection.Transactions);
        Assert.True(transaction.IsCommitted);
        Assert.True(connection.IsDisposed);
    }

    [Fact]
    public void Before_AfterTestPhase_DoesNotExecuteSql()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.AfterTest, "First.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        Assert.Empty(this.connections);
    }

    [Fact]
    public void After_AfterTestPhase_ExecutesSqlFilesAndCommits()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.AfterTest, "First.sql");

        // Act
        attribute.After(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal([FirstSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
        Assert.True(Assert.Single(connection.Transactions).IsCommitted);
        Assert.True(connection.IsDisposed);
    }

    [Fact]
    public void After_BeforeTestPhase_DoesNotExecuteSql()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql");

        // Act
        attribute.After(MethodUnderTest, this.test);

        // Assert
        Assert.Empty(this.connections);
    }

    [Fact]
    public void Before_SqlFileInSubdirectory_ExecutesSqlFile()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, Path.Combine("Sub", "Nested.sql"));

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal([NestedSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
    }

    [Fact]
    public void Before_TestDataDirectoryChanged_ReadsSqlFileFromChangedDirectory()
    {
        // Arrange
        ExecuteSqlSettings.TestDataDirectory = Path.Combine(ExecuteSqlSettings.DefaultTestDataDirectory, "Sub");
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "Nested.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal([NestedSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
    }

    [Fact]
    public void Before_CommandTimeoutIsNull_UsesProviderDefault()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql", "Second.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal([FakeDbCommand.DefaultCommandTimeout, FakeDbCommand.DefaultCommandTimeout], connection.ExecutedCommandTimeouts);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(600)]
    public void Before_CommandTimeoutSet_AppliesToAllCommands(int commandTimeout)
    {
        // Arrange
        ExecuteSqlSettings.CommandTimeout = commandTimeout;
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql", "Second.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal([commandTimeout, commandTimeout], connection.ExecutedCommandTimeouts);
    }

    [Fact]
    public void Before_SqlCommandFails_RollsBackAndRethrows()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = () =>
        {
            var connection = new FakeDbConnection { FailingCommandText = File.ReadAllText(Path.Combine(ExecuteSqlSettings.DefaultTestDataDirectory, "Second.sql")) };
            this.connections.Add(connection);
            return connection;
        };
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql", "Second.sql");

        // Act
        var action = () => attribute.Before(MethodUnderTest, this.test);

        // Assert
        Assert.Throws<FakeDbException>(action);
        var connection = Assert.Single(this.connections);
        var transaction = Assert.Single(connection.Transactions);
        Assert.False(transaction.IsCommitted);
        Assert.True(transaction.IsRolledBack);
        Assert.True(connection.IsDisposed);
    }

    [Fact]
    public void Before_SqlFileNotFound_ThrowsFileNotFoundExceptionWithoutConnecting()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql", "NotFound.sql");

        // Act
        var action = () => attribute.Before(MethodUnderTest, this.test);

        // Assert
        var exception = Assert.Throws<FileNotFoundException>(action);
        Assert.Equal(Path.Combine(ExecuteSqlSettings.DefaultTestDataDirectory, "NotFound.sql"), exception.FileName);
        Assert.Empty(this.connections);
    }

    [Fact]
    public void Before_WhiteSpaceOnlySqlFile_SkipsSqlFile()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "Whitespace.sql", "First.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.connections);
        Assert.Equal([FirstSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
    }

    [Fact]
    public void Before_AllSqlFilesAreWhiteSpaceOnly_DoesNotConnect()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "Whitespace.sql");

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        Assert.Empty(this.connections);
    }

    [Fact]
    public void Before_NoSqlFiles_DoesNotConnect()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = null;
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest);

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        Assert.Empty(this.connections);
    }

    [Fact]
    public void Before_ConnectionFactoryIsNotSet_ThrowsInvalidOperationException()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = null;
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql");

        // Act
        var action = () => attribute.Before(MethodUnderTest, this.test);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(nameof(ExecuteSqlSettings.ConnectionFactory), exception.Message);
    }

    [Fact]
    public void Before_ConnectionFactoryReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = () => null!;
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql");

        // Act
        var action = () => attribute.Before(MethodUnderTest, this.test);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(nameof(ExecuteSqlSettings.ConnectionFactory), exception.Message);
    }

    [Fact]
    public void Before_DatabaseSpecified_ExecutesSqlFilesOnNamedDatabase()
    {
        // Arrange
        this.AddOrdersDatabase();
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql") { Database = OrdersDatabase };

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        Assert.Empty(this.connections);
        var connection = Assert.Single(this.ordersConnections);
        Assert.Equal([FirstSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
        Assert.True(Assert.Single(connection.Transactions).IsCommitted);
        Assert.True(connection.IsDisposed);
    }

    [Fact]
    public void After_DatabaseSpecified_ExecutesSqlFilesOnNamedDatabase()
    {
        // Arrange
        this.AddOrdersDatabase();
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.AfterTest, "First.sql") { Database = OrdersDatabase };

        // Act
        attribute.After(MethodUnderTest, this.test);

        // Assert
        Assert.Empty(this.connections);
        var connection = Assert.Single(this.ordersConnections);
        Assert.Equal([FirstSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
    }

    [Fact]
    public void Before_DatabaseSpecifiedAndConnectionFactoryIsNotSet_ExecutesSqlFilesOnNamedDatabase()
    {
        // Arrange
        ExecuteSqlSettings.ConnectionFactory = null;
        this.AddOrdersDatabase();
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql") { Database = OrdersDatabase };

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        Assert.Single(this.ordersConnections);
    }

    [Fact]
    public void Before_DatabaseWithoutOptions_UsesGlobalTestDataDirectoryAndCommandTimeout()
    {
        // Arrange
        ExecuteSqlSettings.TestDataDirectory = Path.Combine(ExecuteSqlSettings.DefaultTestDataDirectory, "Sub");
        ExecuteSqlSettings.CommandTimeout = 60;
        this.AddOrdersDatabase();
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "Nested.sql") { Database = OrdersDatabase };

        // Act
        attribute.Before(MethodUnderTest, this.test);

        // Assert
        var connection = Assert.Single(this.ordersConnections);
        Assert.Equal([NestedSql], connection.ExecutedCommandTexts.Select(NormalizeNewLine));
        Assert.Equal([60], connection.ExecutedCommandTimeouts);
    }

    [Fact]
    public void Before_DatabaseWithOptions_UsesDatabaseTestDataDirectoryAndCommandTimeout()
    {
        // Arrange
        ExecuteSqlSettings.CommandTimeout = 60;
        this.AddOrdersDatabase(options =>
        {
            options.TestDataDirectory = Path.Combine(ExecuteSqlSettings.DefaultTestDataDirectory, "Sub");
            options.CommandTimeout = 600;
        });
        var namedAttribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "Nested.sql") { Database = OrdersDatabase };
        var defaultAttribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql");

        // Act
        namedAttribute.Before(MethodUnderTest, this.test);
        defaultAttribute.Before(MethodUnderTest, this.test);

        // Assert
        var ordersConnection = Assert.Single(this.ordersConnections);
        Assert.Equal([NestedSql], ordersConnection.ExecutedCommandTexts.Select(NormalizeNewLine));
        Assert.Equal([600], ordersConnection.ExecutedCommandTimeouts);

        // データベースごとの設定は、既定のデータベースに影響しない。
        var defaultConnection = Assert.Single(this.connections);
        Assert.Equal([FirstSql], defaultConnection.ExecutedCommandTexts.Select(NormalizeNewLine));
        Assert.Equal([60], defaultConnection.ExecutedCommandTimeouts);
    }

    [Fact]
    public void Before_DatabaseNotRegistered_ThrowsInvalidOperationExceptionWithoutConnecting()
    {
        // Arrange
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql") { Database = OrdersDatabase };

        // Act
        var action = () => attribute.Before(MethodUnderTest, this.test);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains($"'{OrdersDatabase}'", exception.Message);
        Assert.Empty(this.connections);
    }

    [Fact]
    public void Before_NamedConnectionFactoryReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        this.addedDatabaseNames.Add(OrdersDatabase);
        ExecuteSqlSettings.AddDatabase(OrdersDatabase, () => null!);
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.BeforeTest, "First.sql") { Database = OrdersDatabase };

        // Act
        var action = () => attribute.Before(MethodUnderTest, this.test);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains($"'{OrdersDatabase}'", exception.Message);
    }

    private static string NormalizeNewLine(string sql) => sql.Replace("\r\n", "\n").TrimEnd('\n');

    private void AddOrdersDatabase(Action<ExecuteSqlDatabaseOptions>? configure = null)
    {
        this.addedDatabaseNames.Add(OrdersDatabase);
        ExecuteSqlSettings.AddDatabase(
            OrdersDatabase,
            () =>
            {
                var connection = new FakeDbConnection();
                this.ordersConnections.Add(connection);
                return connection;
            },
            configure);
    }
}
