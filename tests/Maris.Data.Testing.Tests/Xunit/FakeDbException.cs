using System.Data.Common;

namespace Maris.Data.Testing.Tests.Xunit;

/// <summary>
///  <see cref="FakeDbConnection"/> で SQL の実行に失敗したことを表す例外です。
/// </summary>
internal sealed class FakeDbException : DbException
{
    /// <summary>
    ///  <see cref="FakeDbException"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">エラーメッセージ。</param>
    public FakeDbException(string message)
        : base(message)
    {
    }
}
