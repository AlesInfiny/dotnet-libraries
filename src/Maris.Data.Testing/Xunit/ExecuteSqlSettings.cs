using System.Collections.Concurrent;
using System.Data.Common;

namespace Maris.Data.Testing.Xunit;

/// <summary>
///  <see cref="ExecuteSqlAttribute"/> の動作を設定します。
/// </summary>
/// <remarks>
///  テストの実行前（アセンブリフィクスチャーのコンストラクターなど）に一度だけ設定してください。
///  <see cref="ExecuteSqlAttribute.Database"/> を指定しない属性は、 <see cref="ConnectionFactory"/> で作成した接続（既定のデータベース）に対して SQL ファイルを実行します。
///  複数のデータベースを使用する場合は、 <see cref="AddDatabase(string, Func{DbConnection}, Action{ExecuteSqlDatabaseOptions}?)"/> で名前付きのデータベースを登録してください。
/// </remarks>
public static class ExecuteSqlSettings
{
    /// <summary>
    ///  SQL ファイルの基準フォルダーの既定値（出力フォルダー直下の TestData フォルダー）です。
    /// </summary>
    public static readonly string DefaultTestDataDirectory = Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly ConcurrentDictionary<string, NamedDatabase> Databases = new(StringComparer.Ordinal);

    private static string testDataDirectory = DefaultTestDataDirectory;

    private static int? commandTimeout;

    /// <summary>
    ///  既定のデータベースに対して SQL ファイルを実行するための、未オープンのデータベース接続を作成する処理を取得または設定します。
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
    ///  名前付きのデータベースでは、 <see cref="ExecuteSqlDatabaseOptions.CommandTimeout"/> を設定しなかった場合にこの値を使用します。
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
            ValidateCommandTimeout(value);
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
    ///  名前付きのデータベースでは、 <see cref="ExecuteSqlDatabaseOptions.TestDataDirectory"/> を設定しなかった場合にこの値を使用します。
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
            testDataDirectory = ResolveTestDataDirectory(value);
        }
    }

    /// <summary>
    ///  名前付きのデータベースを登録します。
    ///  登録したデータベースに対して SQL ファイルを実行するには、 <see cref="ExecuteSqlAttribute.Database"/> に名前を指定します。
    /// </summary>
    /// <param name="name">データベースの名前。大文字と小文字を区別します。</param>
    /// <param name="connectionFactory">未オープンのデータベース接続を作成する処理。</param>
    /// <param name="configure">データベースごとの設定を行う処理。設定しなかった項目には、 <see cref="ExecuteSqlSettings"/> の値を使用します。</param>
    /// <exception cref="ArgumentException">
    ///  <paramref name="name"/> が <see langword="null"/> または空白です。
    ///  または、同じ名前のデータベースがすでに登録されています。
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> が <see langword="null"/> です。</exception>
    public static void AddDatabase(string name, Func<DbConnection> connectionFactory, Action<ExecuteSqlDatabaseOptions>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("データベースの名前を指定してください。", nameof(name));
        }

        if (connectionFactory is null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        var options = new ExecuteSqlDatabaseOptions();
        configure?.Invoke(options);

        // 登録後に options を変更されても影響を受けないよう、値をコピーして保持する。
        var database = new NamedDatabase(connectionFactory, options.TestDataDirectory, options.CommandTimeout);
        if (!Databases.TryAdd(name, database))
        {
            throw new ArgumentException($"名前 '{name}' のデータベースはすでに登録されています。", nameof(name));
        }
    }

    /// <summary>
    ///  登録した名前付きのデータベースを削除します。
    /// </summary>
    /// <param name="name">データベースの名前。</param>
    /// <returns>削除した場合は <see langword="true"/> 、指定した名前のデータベースが登録されていない場合は <see langword="false"/> 。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> が <see langword="null"/> です。</exception>
    public static bool RemoveDatabase(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return Databases.TryRemove(name, out _);
    }

    /// <summary>
    ///  SQL ファイルを実行するデータベースの設定を、現在の <see cref="ExecuteSqlSettings"/> の値で解決します。
    /// </summary>
    /// <param name="name">データベースの名前。 <see langword="null"/> の場合は既定のデータベース。</param>
    /// <returns>解決済みの設定。</returns>
    /// <exception cref="InvalidOperationException">
    ///  <paramref name="name"/> が <see langword="null"/> で <see cref="ConnectionFactory"/> が設定されていません。
    ///  または、指定した名前のデータベースが登録されていません。
    /// </exception>
    internal static ExecuteSqlDatabase GetDatabase(string? name)
    {
        if (name is null)
        {
            var connectionFactory = ConnectionFactory
                ?? throw new InvalidOperationException(
                    $"{nameof(ExecuteSqlSettings)}.{nameof(ConnectionFactory)} が設定されていません。テストの実行前に、データベース接続を作成する処理を設定してください。");
            return new ExecuteSqlDatabase(connectionFactory, TestDataDirectory, CommandTimeout);
        }

        if (!Databases.TryGetValue(name, out var database))
        {
            var registeredNames = Databases.Keys.OrderBy(key => key, StringComparer.Ordinal).Select(key => $"'{key}'").ToList();
            var registered = registeredNames.Count == 0 ? "なし" : string.Join(", ", registeredNames);
            throw new InvalidOperationException(
                $"名前 '{name}' のデータベースが登録されていません。テストの実行前に、 {nameof(ExecuteSqlSettings)}.{nameof(AddDatabase)} で登録してください。登録済みのデータベース: {registered}");
        }

        return new ExecuteSqlDatabase(
            database.ConnectionFactory,
            database.TestDataDirectory ?? TestDataDirectory,
            database.CommandTimeout ?? CommandTimeout);
    }

    /// <summary>
    ///  SQL ファイルの基準フォルダーのパスを、出力フォルダーを基準とした絶対パスに解決します。
    /// </summary>
    /// <param name="value">フォルダーのパス。</param>
    /// <returns>解決後の絶対パス。</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see langword="null"/> 、空白、またはパスとして無効です。</exception>
    internal static string ResolveTestDataDirectory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("TestData フォルダーのパスを指定してください。", nameof(value));
        }

        // 相対パスはテストランナーによって変わるカレントディレクトリではなく、出力フォルダーを基準に解決する。
        // 絶対パスはそのまま正規化し、相対パスのみ出力フォルダー基準で結合する。
        return Path.IsPathRooted(value)
            ? Path.GetFullPath(value)
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, value));
    }

    /// <summary>
    ///  コマンドのタイムアウト時間が 0 以上であることを検証します。
    /// </summary>
    /// <param name="value">コマンドのタイムアウト時間（秒）。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> が負の値です。</exception>
    internal static void ValidateCommandTimeout(int? value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "コマンドのタイムアウト時間には 0 以上の値を指定してください。");
        }
    }

    /// <summary>
    ///  登録された名前付きのデータベースです。
    ///  設定しなかった項目は <see langword="null"/> で保持し、 SQL ファイルの実行時に <see cref="ExecuteSqlSettings"/> の値で補います。
    /// </summary>
    private sealed class NamedDatabase
    {
        public NamedDatabase(Func<DbConnection> connectionFactory, string? testDataDirectory, int? commandTimeout)
        {
            this.ConnectionFactory = connectionFactory;
            this.TestDataDirectory = testDataDirectory;
            this.CommandTimeout = commandTimeout;
        }

        public Func<DbConnection> ConnectionFactory { get; }

        public string? TestDataDirectory { get; }

        public int? CommandTimeout { get; }
    }
}
