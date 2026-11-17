using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using Maris.ComponentModel.Resources;

namespace Maris.ComponentModel.Tests.Resources;

public class MessagesTest
{
    // 複合書式指定のプレースホルダー（ {0} 、 {1:yyyy} など）に一致する。
    // エスケープされた波かっこ（ {{ 、 }} ）は、事前に取り除いてから検索する。
    private static readonly Regex PlaceholderRegex = new(@"\{(?<index>\d+)[^{}]*\}", RegexOptions.CultureInvariant);

    [Theory]
    [InlineData("en")] // 英語
    public void Keys_MatchNeutralResource(string cultureName)
    {
        // Arrange
        var neutralResources = GetNeutralResources();

        // Act
        var localizedResources = GetLocalizedResources(cultureName);

        // Assert
        var neutralKeys = neutralResources.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray();
        var localizedKeys = localizedResources.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray();
        Assert.NotEmpty(neutralKeys);
        Assert.Equal(neutralKeys, localizedKeys);
    }

    [Theory]
    [InlineData("en")] // 英語
    public void Placeholders_MatchNeutralResource(string cultureName)
    {
        // Arrange
        var neutralResources = GetNeutralResources();
        var localizedResources = GetLocalizedResources(cultureName);

        // Act
        // キーの一致は Keys_MatchNeutralResource で確認するため、両方に存在するキーのみを対象とする。
        var mismatches = neutralResources
            .Where(neutral => localizedResources.ContainsKey(neutral.Key))
            .Select(neutral => new
            {
                neutral.Key,
                Neutral = GetPlaceholderIndexes(neutral.Value),
                Localized = GetPlaceholderIndexes(localizedResources[neutral.Key]),
            })
            .Where(item => !item.Neutral.SequenceEqual(item.Localized))
            .Select(item => $"{item.Key}: neutral={{{string.Join(",", item.Neutral)}}}, {cultureName}={{{string.Join(",", item.Localized)}}}")
            .ToArray();

        // Assert
        Assert.Empty(mismatches);
    }

    [Theory]
    [InlineData("{0} は {1} より後にしてください。", new[] { 0, 1 })] // プレースホルダーが 2 つ
    [InlineData("{1} must be after {0}.", new[] { 0, 1 })] // プレースホルダーの順序が逆
    [InlineData("{0} と {0} と {2}", new[] { 0, 2 })] // 同じプレースホルダーが複数回出現し、番号が連続しない
    [InlineData("{0:yyyy/MM/dd} と {1,10}", new[] { 0, 1 })] // 書式指定、配置指定を含む
    [InlineData("{{0}} と {1}", new[] { 1 })] // エスケープされた波かっこはプレースホルダーとして扱わない
    [InlineData("プレースホルダーなし", new int[0])] // プレースホルダーがない
    public void GetPlaceholderIndexes_ReturnsDistinctSortedIndexes(string value, int[] expected)
    {
        // Arrange & Act
        var indexes = GetPlaceholderIndexes(value);

        // Assert
        Assert.Equal(expected, indexes);
    }

    private static Dictionary<string, string> GetNeutralResources()
        => GetResources(CultureInfo.InvariantCulture);

    private static Dictionary<string, string> GetLocalizedResources(string cultureName)
        => GetResources(CultureInfo.GetCultureInfo(cultureName));

    private static Dictionary<string, string> GetResources(CultureInfo culture)
    {
        // 親カルチャのリソースにフォールバックさせず、指定したカルチャのリソースのみを取得する。
        var resourceSet = Messages.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.True(resourceSet is not null, $"カルチャ '{culture.Name}' のメッセージリソースが見つかりません。");
        return resourceSet!
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    private static int[] GetPlaceholderIndexes(string value)
    {
        var unescaped = value.Replace("{{", string.Empty).Replace("}}", string.Empty);
        return PlaceholderRegex.Matches(unescaped)
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(match => int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture))
            .Distinct()
            .OrderBy(index => index)
            .ToArray();
    }
}
