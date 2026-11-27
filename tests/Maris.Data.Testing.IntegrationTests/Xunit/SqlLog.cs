namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  コミットされた SQL を、コミットされた順に記録します。
/// </summary>
internal sealed class SqlLog
{
    private readonly List<string> committedSqlLines = [];
    private readonly object syncRoot = new();

    /// <summary>
    ///  コミットされた SQL を、コミットされた順に取得します。
    /// </summary>
    public IReadOnlyList<string> CommittedSqlLines
    {
        get
        {
            lock (this.syncRoot)
            {
                return [.. this.committedSqlLines];
            }
        }
    }

    /// <summary>
    ///  コミットされた SQL を追加します。
    /// </summary>
    /// <param name="sqlLines">コミットされた SQL 。</param>
    public void AddRange(IEnumerable<string> sqlLines)
    {
        lock (this.syncRoot)
        {
            this.committedSqlLines.AddRange(sqlLines);
        }
    }
}
