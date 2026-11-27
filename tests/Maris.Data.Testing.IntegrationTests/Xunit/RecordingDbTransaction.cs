using System.Data;
using System.Data.Common;

namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  コミットした時点で、実行された SQL を <see cref="SqlLog"/> に記録するテスト用のトランザクションです。
///  コミットせずに破棄した場合、実行された SQL は記録しません（ロールバック）。
/// </summary>
internal sealed class RecordingDbTransaction : DbTransaction
{
    private readonly RecordingDbConnection connection;
    private readonly SqlLog log;
    private readonly List<string> pendingSqls = [];

    /// <summary>
    ///  <see cref="RecordingDbTransaction"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="connection">トランザクションを開始した接続。</param>
    /// <param name="log">コミットされた SQL を記録するログ。</param>
    /// <param name="isolationLevel">分離レベル。</param>
    public RecordingDbTransaction(RecordingDbConnection connection, SqlLog log, IsolationLevel isolationLevel)
    {
        this.connection = connection;
        this.log = log;
        this.IsolationLevel = isolationLevel;
    }

    /// <inheritdoc/>
    public override IsolationLevel IsolationLevel { get; }

    /// <inheritdoc/>
    protected override DbConnection DbConnection => this.connection;

    /// <inheritdoc/>
    public override void Commit()
    {
        this.log.AddRange(this.pendingSqls);
        this.pendingSqls.Clear();
    }

    /// <inheritdoc/>
    public override void Rollback() => this.pendingSqls.Clear();

    /// <summary>
    ///  実行された SQL を、コミット待ちとして追加します。
    /// </summary>
    /// <param name="sql">実行された SQL 。</param>
    internal void Add(string sql) => this.pendingSqls.Add(sql);
}
