namespace Maris.Data.Testing.Xunit;

/// <summary>
///  <see cref="ExecuteSqlAttribute"/> で SQL ファイルを実行するタイミングです。
/// </summary>
public enum SqlExecutionPhase
{
    /// <summary>
    ///  テストメソッドの実行前。
    /// </summary>
    BeforeTest,

    /// <summary>
    ///  テストメソッドの実行後（テストが失敗した場合も含む）。
    /// </summary>
    AfterTest,
}
