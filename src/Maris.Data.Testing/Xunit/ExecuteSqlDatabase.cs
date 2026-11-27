using System.Data.Common;

namespace Maris.Data.Testing.Xunit;

/// <summary>
///  <see cref="ExecuteSqlAttribute"/> が SQL ファイルを実行するデータベースの、解決済みの設定です。
/// </summary>
internal sealed class ExecuteSqlDatabase
{
    /// <summary>
    ///  <see cref="ExecuteSqlDatabase"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="connectionFactory">未オープンのデータベース接続を作成する処理。</param>
    /// <param name="testDataDirectory">SQL ファイルの相対パスの基準となるフォルダー。</param>
    /// <param name="commandTimeout">コマンドのタイムアウト時間（秒）。 <see langword="null"/> の場合はプロバイダーの既定値。</param>
    public ExecuteSqlDatabase(Func<DbConnection> connectionFactory, string testDataDirectory, int? commandTimeout)
    {
        this.ConnectionFactory = connectionFactory;
        this.TestDataDirectory = testDataDirectory;
        this.CommandTimeout = commandTimeout;
    }

    /// <summary>
    ///  未オープンのデータベース接続を作成する処理を取得します。
    /// </summary>
    public Func<DbConnection> ConnectionFactory { get; }

    /// <summary>
    ///  SQL ファイルの相対パスの基準となるフォルダーを取得します。
    /// </summary>
    public string TestDataDirectory { get; }

    /// <summary>
    ///  コマンドのタイムアウト時間（秒）を取得します。
    ///  <see langword="null"/> の場合はプロバイダーの既定値を使用します。
    /// </summary>
    public int? CommandTimeout { get; }
}
