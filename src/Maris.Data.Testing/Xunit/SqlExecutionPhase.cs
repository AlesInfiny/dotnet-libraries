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
    ///  テストメソッドの実行後（テストメソッドが失敗した場合も含む）。
    /// </summary>
    /// <remarks>
    ///  <see cref="BeforeTest"/> の SQL ファイルの実行に失敗した場合は、実行されないことがあります。
    ///  確実に実行するには、 <see cref="AfterTest"/> の属性を <see cref="BeforeTest"/> の属性より前に記述してください。
    /// </remarks>
    AfterTest,
}
