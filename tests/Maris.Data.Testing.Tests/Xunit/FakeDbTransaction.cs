using System.Data;
using System.Data.Common;

namespace Maris.Data.Testing.Tests.Xunit;

/// <summary>
///  コミットやロールバックの状態を記録するテスト用のトランザクションです。
/// </summary>
internal sealed class FakeDbTransaction : DbTransaction
{
    private readonly FakeDbConnection connection;

    /// <summary>
    ///  <see cref="FakeDbTransaction"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="connection">トランザクションを開始した接続。</param>
    /// <param name="isolationLevel">分離レベル。</param>
    public FakeDbTransaction(FakeDbConnection connection, IsolationLevel isolationLevel)
    {
        this.connection = connection;
        this.IsolationLevel = isolationLevel;
    }

    /// <inheritdoc/>
    public override IsolationLevel IsolationLevel { get; }

    /// <summary>
    ///  コミットされたかどうかを取得します。
    /// </summary>
    public bool IsCommitted { get; private set; }

    /// <summary>
    ///  ロールバックされたかどうかを取得します。
    ///  一般的な ADO.NET プロバイダーと同様に、コミットせずに破棄した場合もロールバックされたものとします。
    /// </summary>
    public bool IsRolledBack { get; private set; }

    /// <summary>
    ///  トランザクションが完了しておらず、コマンドを実行できるかどうかを取得します。
    /// </summary>
    public bool IsActive => !this.IsCommitted && !this.IsRolledBack;

    /// <inheritdoc/>
    protected override DbConnection DbConnection => this.connection;

    /// <inheritdoc/>
    public override void Commit()
    {
        this.ThrowIfNotActive();
        this.IsCommitted = true;
    }

    /// <inheritdoc/>
    public override void Rollback()
    {
        this.ThrowIfNotActive();
        this.IsRolledBack = true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.IsActive)
        {
            this.IsRolledBack = true;
        }

        base.Dispose(disposing);
    }

    private void ThrowIfNotActive()
    {
        if (!this.IsActive)
        {
            throw new InvalidOperationException("トランザクションは既に完了しています。");
        }
    }
}
