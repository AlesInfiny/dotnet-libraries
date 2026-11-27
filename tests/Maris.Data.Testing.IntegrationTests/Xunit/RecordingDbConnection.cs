using System.Data;
using System.Data.Common;

namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  コミットされた SQL を <see cref="SqlLog"/> に記録するテスト用のデータベース接続です。
///  <see cref="FailingKeyword"/> を含む SQL は実行に失敗します。
/// </summary>
internal sealed class RecordingDbConnection : DbConnection
{
    /// <summary>
    ///  この文字列を含む SQL の実行時に例外を発生させます。
    /// </summary>
    public const string FailingKeyword = "FAIL";

    private readonly SqlLog log;
    private ConnectionState state = ConnectionState.Closed;

    /// <summary>
    ///  <see cref="RecordingDbConnection"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="log">コミットされた SQL を記録するログ。</param>
    public RecordingDbConnection(SqlLog log)
    {
        this.log = log;
    }

#pragma warning disable CS8765 // ターゲットフレームワークによって基底クラスの null 許容注釈が異なるため。
    /// <inheritdoc/>
    public override string ConnectionString { get; set; } = string.Empty;
#pragma warning restore CS8765

    /// <inheritdoc/>
    public override string Database => "RecordingDatabase";

    /// <inheritdoc/>
    public override string DataSource => "RecordingDataSource";

    /// <inheritdoc/>
    public override string ServerVersion => "1.0";

    /// <inheritdoc/>
    public override ConnectionState State => this.state;

    /// <inheritdoc/>
    public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Close() => this.state = ConnectionState.Closed;

    /// <inheritdoc/>
    public override void Open() => this.state = ConnectionState.Open;

    /// <inheritdoc/>
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        => new RecordingDbTransaction(this, this.log, isolationLevel);

    /// <inheritdoc/>
    protected override DbCommand CreateDbCommand() => new RecordingDbCommand(this);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.state = ConnectionState.Closed;
        base.Dispose(disposing);
    }
}
