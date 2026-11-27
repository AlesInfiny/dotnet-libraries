using System.Data;
using System.Data.Common;

namespace Maris.Data.Testing.Tests.Xunit;

/// <summary>
///  実行された操作を記録するテスト用のデータベース接続です。
/// </summary>
internal sealed class FakeDbConnection : DbConnection
{
    private ConnectionState state = ConnectionState.Closed;

    /// <summary>
    ///  <see cref="DbCommand.ExecuteNonQuery"/> の実行時に例外を発生させる CommandText を取得または設定します。
    /// </summary>
    public string? FailingCommandText { get; set; }

    /// <summary>
    ///  実行された CommandText を実行順に取得します。
    /// </summary>
    public List<string> ExecutedCommandTexts { get; } = [];

    /// <summary>
    ///  実行されたコマンドの CommandTimeout を実行順に取得します。
    /// </summary>
    public List<int> ExecutedCommandTimeouts { get; } = [];

    /// <summary>
    ///  作成されたトランザクションを取得します。
    /// </summary>
    public List<FakeDbTransaction> Transactions { get; } = [];

    /// <summary>
    ///  <see cref="Open"/> が呼び出された回数を取得します。
    /// </summary>
    public int OpenCount { get; private set; }

    /// <summary>
    ///  接続が破棄されたかどうかを取得します。
    /// </summary>
    public bool IsDisposed { get; private set; }

#pragma warning disable CS8765 // ターゲットフレームワークによって基底クラスの null 許容注釈が異なるため。
    /// <inheritdoc/>
    public override string ConnectionString { get; set; } = string.Empty;
#pragma warning restore CS8765

    /// <inheritdoc/>
    public override string Database => "FakeDatabase";

    /// <inheritdoc/>
    public override string DataSource => "FakeDataSource";

    /// <inheritdoc/>
    public override string ServerVersion => "1.0";

    /// <inheritdoc/>
    public override ConnectionState State => this.state;

    /// <inheritdoc/>
    public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Close() => this.state = ConnectionState.Closed;

    /// <inheritdoc/>
    public override void Open()
    {
        this.OpenCount++;
        this.state = ConnectionState.Open;
    }

    /// <summary>
    ///  コマンドを実行したものとして記録します。
    /// </summary>
    /// <param name="command">実行するコマンド。</param>
    /// <returns>影響を受けた行数。</returns>
    internal int Execute(FakeDbCommand command)
    {
        if (this.state != ConnectionState.Open)
        {
            throw new InvalidOperationException("接続がオープンされていません。");
        }

        if (command.CommandText == this.FailingCommandText)
        {
            throw new FakeDbException("SQL の実行に失敗しました。");
        }

        this.ExecutedCommandTexts.Add(command.CommandText);
        this.ExecutedCommandTimeouts.Add(command.CommandTimeout);
        return 1;
    }

    /// <inheritdoc/>
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        var transaction = new FakeDbTransaction(this, isolationLevel);
        this.Transactions.Add(transaction);
        return transaction;
    }

    /// <inheritdoc/>
    protected override DbCommand CreateDbCommand() => new FakeDbCommand(this);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.IsDisposed = true;
        this.state = ConnectionState.Closed;
        base.Dispose(disposing);
    }
}
