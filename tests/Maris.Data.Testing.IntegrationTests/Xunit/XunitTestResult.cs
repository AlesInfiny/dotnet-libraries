namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  <see cref="XunitPipeline"/> で実行したテストの結果です。
/// </summary>
internal sealed class XunitTestResult
{
    /// <summary>
    ///  <see cref="XunitTestResult"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="displayName">テストの表示名。</param>
    /// <param name="outcome">テストの結果。</param>
    /// <param name="messages">失敗やスキップの理由を表すメッセージ。</param>
    public XunitTestResult(string displayName, XunitTestOutcome outcome, IReadOnlyList<string> messages)
    {
        this.DisplayName = displayName;
        this.Outcome = outcome;
        this.Messages = messages;
    }

    /// <summary>
    ///  テストの表示名を取得します。
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///  テストの結果を取得します。
    /// </summary>
    public XunitTestOutcome Outcome { get; }

    /// <summary>
    ///  失敗やスキップの理由を表すメッセージを取得します。
    /// </summary>
    public IReadOnlyList<string> Messages { get; }

    /// <inheritdoc/>
    public override string ToString()
        => $"{this.DisplayName}: {this.Outcome}{(this.Messages.Count > 0 ? $" ({string.Join(" / ", this.Messages)})" : string.Empty)}";
}
