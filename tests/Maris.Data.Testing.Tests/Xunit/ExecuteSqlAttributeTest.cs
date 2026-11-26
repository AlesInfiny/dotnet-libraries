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

    private static readonly MethodInfo MethodUnderTest = typeof(ExecuteSqlAttributeTest).GetMethod(nameof(Dispose))!;

    private readonly Func<DbConnection>? originalConnectionFactory = ExecuteSqlSettings.ConnectionFactory;
    private readonly string originalTestDataDirectory = ExecuteSqlSettings.TestDataDirectory;
    private readonly IXunitTest test = Mock.Of<IXunitTest>();
    private readonly List<FakeDbConnection> connections = [];

    public ExecuteSqlAttributeTest()
    {
        ExecuteSqlSettings.TestDataDirectory = ExecuteSqlSettings.DefaultTestDataDirectory;
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
    }

    [Fact]
    public void Constructor_SetsPhaseAndSqlFiles()
    {
        // Act
        var attribute = new ExecuteSqlAttribute(SqlExecutionPhase.AfterTest, "First.sql", "Second.sql");

        // Assert
        Assert.Equal(SqlExecutionPhase.AfterTest, attribute.Phase);
        Assert.Equal(["First.sql", "Second.sql"], attribute.SqlFiles);
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

    private static string NormalizeNewLine(string sql) => sql.Replace("\r\n", "\n").TrimEnd('\n');
}
