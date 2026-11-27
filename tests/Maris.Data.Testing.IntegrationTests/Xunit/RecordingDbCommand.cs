using System.Data;
using System.Data.Common;

namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  実行した SQL をトランザクションに記録するテスト用のコマンドです。
/// </summary>
internal sealed class RecordingDbCommand : DbCommand
{
    private readonly RecordingDbConnection connection;

    /// <summary>
    ///  <see cref="RecordingDbCommand"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="connection">コマンドを作成した接続。</param>
    public RecordingDbCommand(RecordingDbConnection connection)
    {
        this.connection = connection;
    }

#pragma warning disable CS8765 // ターゲットフレームワークによって基底クラスの null 許容注釈が異なるため。
    /// <inheritdoc/>
    public override string CommandText { get; set; } = string.Empty;
#pragma warning restore CS8765

    /// <inheritdoc/>
    public override int CommandTimeout { get; set; }

    /// <inheritdoc/>
    public override CommandType CommandType { get; set; } = CommandType.Text;

    /// <inheritdoc/>
    public override bool DesignTimeVisible { get; set; }

    /// <inheritdoc/>
    public override UpdateRowSource UpdatedRowSource { get; set; }

    /// <inheritdoc/>
    protected override DbConnection? DbConnection
    {
        get => this.connection;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override DbParameterCollection DbParameterCollection => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override DbTransaction? DbTransaction { get; set; }

    /// <inheritdoc/>
    public override void Cancel() => throw new NotSupportedException();

    /// <inheritdoc/>
    public override int ExecuteNonQuery()
    {
        if (this.DbTransaction is not RecordingDbTransaction transaction)
        {
            throw new InvalidOperationException("トランザクションが設定されていません。");
        }

        var sql = this.CommandText.Trim();
        if (sql.Contains(RecordingDbConnection.FailingKeyword))
        {
            throw new InvalidOperationException($"SQL の実行に失敗しました: {sql}");
        }

        transaction.Add(sql);
        return 1;
    }

    /// <inheritdoc/>
    public override object? ExecuteScalar() => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Prepare() => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override DbParameter CreateDbParameter() => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => throw new NotSupportedException();
}
