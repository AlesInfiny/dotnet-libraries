namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  コミットされた SQL を、コミットされた順に記録します。
/// </summary>
internal sealed class SqlLog
{
    private readonly List<string> committedSqls = [];
    private readonly object syncRoot = new();

    /// <summary>
    ///  コミットされた SQL を、コミットされた順に取得します。
    /// </summary>
    public IReadOnlyList<string> CommittedSqls
    {
        get
        {
            lock (this.syncRoot)
            {
                return [.. this.committedSqls];
            }
        }
    }

    /// <summary>
    ///  コミットされた SQL を追加します。
    /// </summary>
    /// <param name="sqls">コミットされた SQL 。</param>
    public void AddRange(IEnumerable<string> sqls)
    {
        lock (this.syncRoot)
        {
            this.committedSqls.AddRange(sqls);
        }
    }
}
