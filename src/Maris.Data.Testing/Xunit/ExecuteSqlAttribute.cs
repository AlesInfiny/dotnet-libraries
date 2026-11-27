using System.Reflection;
using Xunit.v3;

namespace Maris.Data.Testing.Xunit;

/// <summary>
///  テストメソッドの実行前または実行後に、指定した SQL ファイルを実行します。
/// </summary>
/// <remarks>
///  <see cref="Database"/> を指定しない場合、データベース接続は <see cref="ExecuteSqlSettings.ConnectionFactory"/> から取得します。
///  <see cref="Database"/> を指定した場合は、
///  <see cref="ExecuteSqlSettings.AddDatabase(string, Func{System.Data.Common.DbConnection}, Action{ExecuteSqlDatabaseOptions}?)"/>
///  で登録した、同じ名前のデータベースの設定を使用します。
///  SQL ファイルのパスは、 SQL ファイルの基準フォルダー（既定は <see cref="ExecuteSqlSettings.TestDataDirectory"/> ）からの相対パスで指定します。
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ExecuteSqlAttribute : BeforeAfterTestAttribute
{
    /// <summary>
    ///  <see cref="ExecuteSqlAttribute"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="phase">SQL ファイルを実行するタイミング。</param>
    /// <param name="sqlFiles">指定した順に実行する SQL ファイルの、 SQL ファイルの基準フォルダーからの相対パス。</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlFiles"/> が <see langword="null"/> です。</exception>
    public ExecuteSqlAttribute(SqlExecutionPhase phase, params string[] sqlFiles)
    {
        this.Phase = phase;
        this.SqlFiles = sqlFiles ?? throw new ArgumentNullException(nameof(sqlFiles));
    }

    /// <summary>
    ///  SQL ファイルを実行するタイミングを取得します。
    /// </summary>
    public SqlExecutionPhase Phase { get; }

    /// <summary>
    ///  実行する SQL ファイルの、 SQL ファイルの基準フォルダーからの相対パスを取得します。
    /// </summary>
    public IReadOnlyList<string> SqlFiles { get; }

    /// <summary>
    ///  SQL ファイルを実行するデータベースの名前を取得または設定します。
    ///  既定値は <see langword="null"/> （既定のデータベース）です。
    /// </summary>
    /// <remarks>
    ///  名前は
    ///  <see cref="ExecuteSqlSettings.AddDatabase(string, Func{System.Data.Common.DbConnection}, Action{ExecuteSqlDatabaseOptions}?)"/>
    ///  で登録したものを指定します。大文字と小文字を区別します。
    ///  登録されていない名前を指定した場合、 SQL ファイルの実行時に例外が発生し、テストは失敗します。
    /// </remarks>
    public string? Database { get; set; }

    /// <inheritdoc/>
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (this.Phase == SqlExecutionPhase.BeforeTest)
        {
            this.Execute();
        }
    }

    /// <inheritdoc/>
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (this.Phase == SqlExecutionPhase.AfterTest)
        {
            this.Execute();
        }
    }

    private static string ReadSqlFile(string testDataDirectory, string sqlFile)
    {
        if (Path.IsPathRooted(sqlFile))
        {
            throw new ArgumentException($"SQL ファイル パスは相対パスである必要があります: '{sqlFile}'", nameof(sqlFile));
        }

        var path = Path.GetFullPath(Path.Combine(testDataDirectory, sqlFile));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"SQL ファイル '{sqlFile}' が見つかりません。", path);
        }

        return File.ReadAllText(path);
    }

    /// <summary>
    ///  <see cref="SqlFiles"/> を指定した順に、 1 つのトランザクション内で実行します。
    ///  いずれかの SQL ファイルの実行に失敗した場合、トランザクションはロールバックされます。
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///  <see cref="Database"/> で指定したデータベースが登録されていないか、
    ///  データベース接続を作成する処理が設定されていないか、 <see langword="null"/> を返しました。
    /// </exception>
    /// <exception cref="FileNotFoundException">SQL ファイルが見つかりません。</exception>
    private void Execute()
    {
        if (this.SqlFiles.Count == 0)
        {
            return;
        }

        var database = ExecuteSqlSettings.GetDatabase(this.Database);

        // データベースに接続する前にすべての SQL ファイルを読み込み、ファイルの不備を早期に検出する。
        var sqlTexts = this.SqlFiles
            .Select(sqlFile => ReadSqlFile(database.TestDataDirectory, sqlFile))
            .Where(sql => !string.IsNullOrWhiteSpace(sql))
            .ToList();

        if (sqlTexts.Count == 0)
        {
            return;
        }

        using var connection = database.ConnectionFactory()
            ?? throw new InvalidOperationException(this.Database is null
                ? $"{nameof(ExecuteSqlSettings)}.{nameof(ExecuteSqlSettings.ConnectionFactory)} が null を返しました。"
                : $"名前 '{this.Database}' のデータベースの接続を作成する処理が null を返しました。");
        connection.Open();

        // コミット前に例外が発生した場合、トランザクションは Dispose 時にロールバックされる。
        using var transaction = connection.BeginTransaction();
        foreach (var sql in sqlTexts)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            if (database.CommandTimeout.HasValue)
            {
                command.CommandTimeout = database.CommandTimeout.Value;
            }

            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
