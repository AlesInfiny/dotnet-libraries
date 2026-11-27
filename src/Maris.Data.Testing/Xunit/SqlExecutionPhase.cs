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
    ///  <see cref="BeforeTest"/> の SQL ファイルの実行に失敗した場合は、属性の記述順に関係なく実行されません。
    ///  xUnit は、いずれかの属性の <see cref="global::Xunit.v3.BeforeAfterTestAttribute.Before"/> で例外が発生すると、
    ///  すべての属性の <see cref="global::Xunit.v3.BeforeAfterTestAttribute.After"/> を呼び出さないためです。
    ///  失敗時にデータを残さないためには、 <see cref="BeforeTest"/> の SQL ファイルを 1 つの属性にまとめて指定してください。
    /// </remarks>
    AfterTest,
}
