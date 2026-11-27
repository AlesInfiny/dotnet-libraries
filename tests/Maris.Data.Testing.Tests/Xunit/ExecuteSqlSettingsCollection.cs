namespace Maris.Data.Testing.Tests.Xunit;

/// <summary>
///  静的な <see cref="Testing.Xunit.ExecuteSqlSettings"/> を変更するテストを、他のテストと並列に実行しないためのコレクションです。
/// </summary>
[CollectionDefinition(nameof(ExecuteSqlSettingsCollection), DisableParallelization = true)]
public sealed class ExecuteSqlSettingsCollection
{
}
