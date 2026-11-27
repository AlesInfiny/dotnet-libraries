using System.Data.Common;
using Maris.Data.Testing.Xunit;

namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  <see cref="ExecuteSqlAttribute"/> を付与したテストを xUnit のパイプラインで実行し、
///  SQL ファイルが実行されるタイミングを確認します。
/// </summary>
/// <remarks>
///  実行するテストは <see cref="Scenarios"/> に定義します。
///  <see cref="Scenarios"/> は internal のため、このテストプロジェクトの実行時には検出されず、
///  <see cref="XunitPipeline"/> から明示的に指定した場合だけ実行されます。
/// </remarks>
public sealed class ExecuteSqlAttributeIntegrationTest : IDisposable
{
    private const string InsertSql = "INSERT INTO Items VALUES (1);";
    private const string CleanupSql = "DELETE FROM Items;";
    private const string OrdersInsertSql = "INSERT INTO Orders VALUES (1);";
    private const string OrdersCleanupSql = "DELETE FROM Orders;";

    private readonly Func<DbConnection>? originalConnectionFactory = ExecuteSqlSettings.ConnectionFactory;
    private readonly SqlLog log = new();
    private readonly SqlLog ordersLog = new();

    public ExecuteSqlAttributeIntegrationTest()
    {
        ExecuteSqlSettings.ConnectionFactory = () => new RecordingDbConnection(this.log);
        ExecuteSqlSettings.AddDatabase(
            Scenarios.OrdersDatabase,
            () => new RecordingDbConnection(this.ordersLog),
            options => options.TestDataDirectory = "TestData/Orders");
        Scenarios.TestMethodExecuted = false;
    }

    public void Dispose()
    {
        ExecuteSqlSettings.ConnectionFactory = this.originalConnectionFactory;
        ExecuteSqlSettings.RemoveDatabase(Scenarios.OrdersDatabase);
    }

    [Fact]
    public async Task TestMethodPasses_ExecutesBeforeTestAndAfterTest()
    {
        // Act
        var results = await XunitPipeline.RunAsync(typeof(Scenarios.TestMethodPasses));

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(XunitTestOutcome.Passed, result.Outcome);
        Assert.True(Scenarios.TestMethodExecuted);
        Assert.Equal([InsertSql, CleanupSql], this.log.CommittedSqlLines);
    }

    [Fact]
    public async Task TestMethodFails_ExecutesAfterTest()
    {
        // Act
        var results = await XunitPipeline.RunAsync(typeof(Scenarios.TestMethodFails));

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(XunitTestOutcome.Failed, result.Outcome);
        Assert.Contains(Scenarios.TestMethodFailureMessage, Assert.Single(result.Messages));
        Assert.Equal([InsertSql, CleanupSql], this.log.CommittedSqlLines);
    }

    [Theory]
    [InlineData(typeof(Scenarios.BeforeTestFailsWithAfterTestDeclaredFirst))]
    [InlineData(typeof(Scenarios.BeforeTestFailsWithAfterTestDeclaredLast))]
    public async Task BeforeTestFails_DoesNotExecuteTestMethodAndAfterTest(Type scenario)
    {
        // Act
        var results = await XunitPipeline.RunAsync(scenario);

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(XunitTestOutcome.Failed, result.Outcome);
        Assert.False(Scenarios.TestMethodExecuted);

        // xUnit は Before で例外が発生すると、属性の記述順に関係なく、どの属性の After も呼び出さない。
        // そのため AfterTest の SQL ファイルは実行されず、失敗より前に別の属性でコミットしたデータが残る。
        Assert.Equal([InsertSql], this.log.CommittedSqlLines);
    }

    [Fact]
    public async Task BeforeTestFails_SqlFilesInSameAttribute_RollsBackAllSqlFiles()
    {
        // Act
        var results = await XunitPipeline.RunAsync(typeof(Scenarios.BeforeTestFailsInSameAttribute));

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(XunitTestOutcome.Failed, result.Outcome);
        Assert.False(Scenarios.TestMethodExecuted);

        // 1 つの属性で指定した SQL ファイルは 1 つのトランザクションで実行するため、まとめてロールバックされる。
        Assert.Empty(this.log.CommittedSqlLines);
    }

    [Fact]
    public async Task MultipleDatabases_ExecutesSqlFilesOnEachDatabase()
    {
        // Act
        var results = await XunitPipeline.RunAsync(typeof(Scenarios.MultipleDatabases));

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(XunitTestOutcome.Passed, result.Outcome);
        Assert.True(Scenarios.TestMethodExecuted);

        // 名前付きのデータベースの SQL ファイルは、そのデータベースに設定した TestDataDirectory から読み込まれる。
        Assert.Equal([InsertSql, CleanupSql], this.log.CommittedSqlLines);
        Assert.Equal([OrdersInsertSql, OrdersCleanupSql], this.ordersLog.CommittedSqlLines);
    }

    [Fact]
    public async Task MultipleDatabases_BeforeTestFails_DoesNotExecuteAfterTestOnAnyDatabase()
    {
        // Act
        var results = await XunitPipeline.RunAsync(typeof(Scenarios.MultipleDatabasesBeforeTestFails));

        // Assert
        var result = Assert.Single(results);
        Assert.Equal(XunitTestOutcome.Failed, result.Outcome);
        Assert.False(Scenarios.TestMethodExecuted);

        // データベースをまたいだトランザクションはないため、別のデータベースでコミットしたデータはロールバックされない。
        // また、どのデータベースの AfterTest の SQL ファイルも実行されない。
        // 属性どうしの実行順は保証されないため、既定のデータベースへの投入が失敗より前に実行されたかどうかは検証しない。
        Assert.DoesNotContain(CleanupSql, this.log.CommittedSqlLines);
        Assert.Empty(this.ordersLog.CommittedSqlLines);
    }

    /// <summary>
    ///  <see cref="XunitPipeline"/> で実行するテストです。
    ///  internal のため、このテストプロジェクトの実行時には検出されません。
    /// </summary>
    internal static class Scenarios
    {
        public const string TestMethodFailureMessage = "テストメソッドが失敗しました。";

        public const string OrdersDatabase = "Orders";

        /// <summary>
        ///  テストメソッドが実行されたかどうかを取得または設定します。
        /// </summary>
        public static bool TestMethodExecuted { get; set; }

        public sealed class TestMethodPasses
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql")]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            public void Test()
            {
                TestMethodExecuted = true;
            }
        }

        public sealed class TestMethodFails
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql")]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            public void Test()
            {
                TestMethodExecuted = true;
                Assert.Fail(TestMethodFailureMessage);
            }
        }

        public sealed class BeforeTestFailsWithAfterTestDeclaredFirst
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql")]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Fail.sql")]
            public void Test()
            {
                TestMethodExecuted = true;
            }
        }

        public sealed class BeforeTestFailsWithAfterTestDeclaredLast
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql")]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Fail.sql")]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            public void Test()
            {
                TestMethodExecuted = true;
            }
        }

        public sealed class BeforeTestFailsInSameAttribute
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql", "Fail.sql")]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            public void Test()
            {
                TestMethodExecuted = true;
            }
        }

        public sealed class MultipleDatabases
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql")]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql", Database = OrdersDatabase)]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql", Database = OrdersDatabase)]
            public void Test()
            {
                TestMethodExecuted = true;
            }
        }

        public sealed class MultipleDatabasesBeforeTestFails
        {
            [Fact]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql")]
            [ExecuteSql(SqlExecutionPhase.BeforeTest, "Insert.sql", "Fail.sql", Database = OrdersDatabase)]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql")]
            [ExecuteSql(SqlExecutionPhase.AfterTest, "Cleanup.sql", Database = OrdersDatabase)]
            public void Test()
            {
                TestMethodExecuted = true;
            }
        }
    }
}
