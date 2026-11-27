namespace Maris.Data.Testing.Xunit;

/// <summary>
///  <see cref="ExecuteSqlSettings.AddDatabase(string, Func{System.Data.Common.DbConnection}, Action{ExecuteSqlDatabaseOptions}?)"/>
///  で登録する名前付きのデータベースごとの設定です。
/// </summary>
/// <remarks>
///  設定しなかった項目には、 <see cref="ExecuteSqlSettings"/> の値を使用します。
/// </remarks>
public sealed class ExecuteSqlDatabaseOptions
{
    private string? testDataDirectory;
    private int? commandTimeout;

    /// <summary>
    ///  このデータベースで実行する SQL ファイルの、相対パスの基準となるフォルダーを取得または設定します。
    ///  既定値は <see langword="null"/> です。
    /// </summary>
    /// <remarks>
    ///  <see langword="null"/> の場合は、 <see cref="ExecuteSqlSettings.TestDataDirectory"/> を使用します。
    ///  相対パスを設定した場合は、カレントディレクトリではなく出力フォルダー（ <see cref="AppContext.BaseDirectory"/> ）を基準に解決します。
    ///  取得される値は、解決後の絶対パスです。
    /// </remarks>
    /// <exception cref="ArgumentException">設定する値が空白、またはパスとして無効です。</exception>
    public string? TestDataDirectory
    {
        get
        {
            return this.testDataDirectory;
        }

        set
        {
            this.testDataDirectory = value is null ? null : ExecuteSqlSettings.ResolveTestDataDirectory(value);
        }
    }

    /// <summary>
    ///  このデータベースで SQL ファイルを実行するコマンドのタイムアウト時間（秒）を取得または設定します。
    ///  既定値は <see langword="null"/> です。
    /// </summary>
    /// <remarks>
    ///  <see langword="null"/> の場合は、 <see cref="ExecuteSqlSettings.CommandTimeout"/> を使用します。
    ///  0 を設定した場合の動作はプロバイダーに依存します（ SqlClient の場合は無制限）。
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">設定する値が負の値です。</exception>
    public int? CommandTimeout
    {
        get
        {
            return this.commandTimeout;
        }

        set
        {
            ExecuteSqlSettings.ValidateCommandTimeout(value);
            this.commandTimeout = value;
        }
    }
}
