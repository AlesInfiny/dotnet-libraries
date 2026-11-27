using System.Data.Common;

namespace Maris.Data.Testing.Xunit;

/// <summary>
///  <see cref="ExecuteSqlAttribute"/> の動作を設定します。
/// </summary>
/// <remarks>
///  テストの実行前（アセンブリフィクスチャーのコンストラクターなど）に一度だけ設定してください。
/// </remarks>
public static class ExecuteSqlSettings
{
    /// <summary>
    ///  SQL ファイルの基準フォルダーの既定値（出力フォルダー直下の TestData フォルダー）です。
    /// </summary>
    public static readonly string DefaultTestDataDirectory = Path.Combine(AppContext.BaseDirectory, "TestData");

    private static string testDataDirectory = DefaultTestDataDirectory;

    private static int? commandTimeout;

    /// <summary>
    ///  SQL ファイルの実行に使用する、未オープンのデータベース接続を作成する処理を取得または設定します。
    /// </summary>
    /// <remarks>
    ///  作成された接続は <see cref="ExecuteSqlAttribute"/> がオープンし、使用後に破棄します。
    /// </remarks>
    public static Func<DbConnection>? ConnectionFactory { get; set; }

    /// <summary>
    ///  SQL ファイルを実行するコマンドのタイムアウト時間（秒）を取得または設定します。
    ///  既定値は <see langword="null"/> です。
    /// </summary>
    /// <remarks>
    ///  <see langword="null"/> の場合は、 ADO.NET プロバイダーの既定値（ SqlClient の場合は 30 秒）を使用します。
    ///  0 を設定した場合の動作はプロバイダーに依存します（ SqlClient の場合は無制限）。
    ///  大量のテストデータを投入する SQL ファイルを実行する場合は、必要に応じて大きな値を設定してください。
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">設定する値が負の値です。</exception>
    public static int? CommandTimeout
    {
        get
        {
            return commandTimeout;
        }

        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "コマンドのタイムアウト時間には 0 以上の値を指定してください。");
            }

            commandTimeout = value;
        }
    }

    /// <summary>
    ///  SQL ファイルの相対パスの基準となるフォルダーを取得または設定します。
    ///  既定値は <see cref="DefaultTestDataDirectory"/> です。
    /// </summary>
    /// <remarks>
    ///  相対パスを設定した場合は、カレントディレクトリではなく出力フォルダー（ <see cref="AppContext.BaseDirectory"/> ）を基準に解決します。
    ///  取得される値は、常に解決後の絶対パスです。
    /// </remarks>
    /// <exception cref="ArgumentException">設定する値が <see langword="null"/> 、空白、またはパスとして無効です。</exception>
    public static string TestDataDirectory
    {
        get
        {
            return testDataDirectory;
        }

        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("TestData フォルダーのパスを指定してください。", nameof(value));
            }

            // 相対パスはテストランナーによって変わるカレントディレクトリではなく、出力フォルダーを基準に解決する。
            // value が絶対パスの場合、Path.Combine は value をそのまま返す。
            testDataDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, value));
        }
    }
}
