namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  <see cref="XunitPipeline"/> で実行したテストの結果の種類です。
/// </summary>
internal enum XunitTestOutcome
{
    /// <summary>
    ///  成功。
    /// </summary>
    Passed,

    /// <summary>
    ///  失敗。
    /// </summary>
    Failed,

    /// <summary>
    ///  スキップ。
    /// </summary>
    Skipped,

    /// <summary>
    ///  未実行。
    /// </summary>
    NotRun,
}
