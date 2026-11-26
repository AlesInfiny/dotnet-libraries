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

    /// <summary>
    ///  SQL ファイルの実行に使用する、未オープンのデータベース接続を作成する処理を取得または設定します。
    /// </summary>
    /// <remarks>
    ///  作成された接続は <see cref="ExecuteSqlAttribute"/> がオープンし、使用後に破棄します。
    /// </remarks>
    public static Func<DbConnection>? ConnectionFactory { get; set; }

    /// <summary>
    ///  SQL ファイルの相対パスの基準となるフォルダーを取得または設定します。
    ///  既定値は <see cref="DefaultTestDataDirectory"/> です。
    /// </summary>
    /// <exception cref="ArgumentException">設定する値が <see langword="null"/> または空白です。</exception>
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

            testDataDirectory = value;
        }
    }
}
