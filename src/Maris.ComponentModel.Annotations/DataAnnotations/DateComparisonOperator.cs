namespace Maris.ComponentModel.DataAnnotations;

/// <summary>
///  <see cref="DateComparisonAttribute"/> で使用する日付の比較方法を表します。
/// </summary>
public enum DateComparisonOperator
{
    /// <summary>
    ///  検証対象の値が、比較先の値より後であることを表します。
    /// </summary>
    After,

    /// <summary>
    ///  検証対象の値が、比較先の値と同じか、比較先の値より後であることを表します。
    /// </summary>
    OnOrAfter,

    /// <summary>
    ///  検証対象の値が、比較先の値より前であることを表します。
    /// </summary>
    Before,

    /// <summary>
    ///  検証対象の値が、比較先の値と同じか、比較先の値より前であることを表します。
    /// </summary>
    OnOrBefore,
}
