using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Maris.ComponentModel.Resources;

namespace Maris.ComponentModel.DataAnnotations;

/// <summary>
///  プロパティの値と、同じオブジェクトの別のプロパティの値について、
///  日付の前後関係を検証する属性です。
/// </summary>
/// <remarks>
///  <para>
///   検証対象の値および比較先の値として、 <see cref="DateTime"/> 、 <see cref="DateTimeOffset"/> 、
///   <c>DateOnly</c> （ .NET 10 以降のみ）とそれぞれの null 許容型、および <see cref="string"/> に対応します。
///  </para>
///  <para>
///   日付型の値は、型を変換せずそのまま比較します。
///   <see cref="DateTime"/> の <see cref="DateTime.Kind"/> は考慮しません。
///   検証対象の値と比較先の値は、同じ型である必要があります。
///  </para>
///  <para>
///   <see cref="string"/> の値は、 <see cref="ConversionType"/> に設定した型へ変換してから比較します。
///   <see cref="Format"/> を設定した場合、その書式に厳密に一致する文字列のみを、カルチャに依存せず変換します。
///   <see cref="Format"/> を設定しない場合、現在のカルチャの書式に従って変換します。
///  </para>
///  <para>
///   以下の場合は比較せず、検証成功とします。
///  </para>
///  <list type="bullet">
///   <item>
///    <description>
///     検証対象の値、比較先の値のいずれかが <see langword="null"/> または空の文字列 ("") である場合。
///     値が必須であることの検証には <see cref="RequiredAttribute"/> を併用してください。
///    </description>
///   </item>
///   <item>
///    <description>
///     検証対象の値、比較先の値のいずれかが、日付型へ変換できない文字列である場合。
///     この属性は文字列の書式を検証しません。
///     書式の検証には <see cref="RegularExpressionAttribute"/> などの検証属性を併用してください。
///    </description>
///   </item>
///  </list>
///  <para>
///   この属性は、同じプロパティに複数付与できます。
///   複数付与した場合は、すべての属性について検証します。
///  </para>
/// </remarks>
/// <example>
///  <para>
///   開始日と終了日の前後関係を検証する例を次に示します。
///  </para>
///  <code language="csharp">
///   <![CDATA[
///    using Maris.ComponentModel.DataAnnotations;
///
///    public class Period
///    {
///        [Display(Name = "開始日")]
///        public DateTime? StartDate { get; set; }
///
///        [Display(Name = "終了日")]
///        [DateComparison(nameof(StartDate), DateComparisonOperator.OnOrAfter)]
///        public DateTime? EndDate { get; set; }
///    }
///   ]]>
///  </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DateComparisonAttribute : ValidationAttribute
{
    private readonly object typeId = new object();

    /// <summary>
    ///  比較先のプロパティ名と比較方法を指定して
    ///  <see cref="DateComparisonAttribute"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="otherProperty">比較先のプロパティ名。</param>
    /// <param name="operator">比較方法。</param>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="otherProperty"/> が <see langword="null"/> です。
    /// </exception>
    public DateComparisonAttribute(string otherProperty, DateComparisonOperator @operator)
        : base(() => GetDefaultErrorMessage(@operator))
    {
        this.OtherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
        this.Operator = @operator;
    }

    /// <summary>
    ///  比較先のプロパティ名を取得します。
    /// </summary>
    public string OtherProperty { get; }

    /// <summary>
    ///  比較方法を取得します。
    /// </summary>
    public DateComparisonOperator Operator { get; }

    /// <summary>
    ///  <see cref="string"/> の値を変換する先の型を取得または設定します。
    ///  既定値は <see cref="DateTime"/> です。
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see cref="DateTime"/> 、 <see cref="DateTimeOffset"/> 、 <c>DateOnly</c> （ .NET 10 以降のみ）の
    ///   いずれかを設定できます。
    ///   検証対象の値、比較先の値のどちらであっても、 <see cref="string"/> の値はこの型へ変換してから比較します。
    ///   日付型の値には作用しません。
    ///  </para>
    ///  <para>
    ///   オフセットを含む文字列を比較する場合は、 <see cref="DateTimeOffset"/> を設定してください。
    ///  </para>
    ///  <para>
    ///   この型へ変換できない文字列は比較せず、検証成功とします。
    ///   書式の検証には <see cref="RegularExpressionAttribute"/> などの検証属性を併用してください。
    ///  </para>
    /// </remarks>
    public Type ConversionType { get; set; } = typeof(DateTime);

    /// <summary>
    ///  <see cref="string"/> の値を日付型へ変換するときの書式を取得または設定します。
    ///  既定値は <see langword="null"/> です。
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   書式を設定した場合、その書式に厳密に一致する文字列のみを、カルチャに依存せず変換します。
    ///   <see langword="null"/> の場合、現在のカルチャの書式に従って変換します。
    ///  </para>
    ///  <para>
    ///   書式に一致しない文字列は比較せず、検証成功とします。
    ///   書式の検証には <see cref="RegularExpressionAttribute"/> などの検証属性を併用してください。
    ///  </para>
    /// </remarks>
    public string? Format { get; set; }

    /// <inheritdoc/>
    public override bool RequiresValidationContext => true;

    /// <summary>
    ///  この属性のインスタンスを一意に識別する値を取得します。
    /// </summary>
    /// <remarks>
    ///  同じプロパティに複数付与した属性がすべて検証に使用されるよう、
    ///  インスタンスごとに異なる値を返します。
    /// </remarks>
    public override object TypeId => this.typeId;

    /// <summary>
    ///  検証対象の名前を指定して、エラーメッセージを書式設定します。
    ///  比較先の名前には <see cref="OtherProperty"/> の値を使用します。
    /// </summary>
    /// <param name="name">検証対象の名前。</param>
    /// <returns>書式設定されたエラーメッセージ。</returns>
    public override string FormatErrorMessage(string name)
        => this.FormatErrorMessage(name, this.OtherProperty);

    /// <summary>
    ///  検証対象の値と比較先のプロパティの値について、日付の前後関係を検証します。
    /// </summary>
    /// <param name="value">検証対象の値。</param>
    /// <param name="validationContext">検証コンテキスト。</param>
    /// <returns>
    ///  検証に成功した場合は <see cref="ValidationResult.Success"/> 。
    ///  それ以外の場合はエラーメッセージを設定した <see cref="ValidationResult"/> 。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///  <list type="bullet">
    ///   <item><description>比較方法に定義されていない値が設定されています。</description></item>
    ///   <item><description><see cref="ConversionType"/> に対応していない型が設定されています。</description></item>
    ///   <item><description>比較先のプロパティが存在しないか、インデクサーです。</description></item>
    ///   <item><description>検証対象の値または比較先の値が、対応していない型です。</description></item>
    ///   <item><description>検証対象の値と比較先の値の型が一致しません。</description></item>
    ///  </list>
    /// </exception>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        this.EnsureConfigurationIsValid();
        var otherPropertyInfo = this.GetOtherPropertyInfo(validationContext.ObjectType);
        var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

        var comparable = this.ToComparable(value);
        var otherComparable = this.ToComparable(otherValue);
        if (comparable is null || otherComparable is null)
        {
            return ValidationResult.Success;
        }

        if (comparable.GetType() != otherComparable.GetType())
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.DateComparisonTypeMismatch,
                    comparable.GetType(),
                    otherComparable.GetType()));
        }

        if (this.Satisfies(comparable.CompareTo(otherComparable)))
        {
            return ValidationResult.Success;
        }

        var otherDisplayName = GetDisplayName(otherPropertyInfo);
        string[]? memberNames = validationContext.MemberName is null ? null : [validationContext.MemberName];
        return new ValidationResult(
            this.FormatErrorMessage(validationContext.DisplayName, otherDisplayName),
            memberNames);
    }

    private static string GetDefaultErrorMessage(DateComparisonOperator @operator)
        => @operator switch
        {
            DateComparisonOperator.After => Messages.DateComparisonAfter,
            DateComparisonOperator.OnOrAfter => Messages.DateComparisonOnOrAfter,
            DateComparisonOperator.Before => Messages.DateComparisonBefore,
            DateComparisonOperator.OnOrBefore => Messages.DateComparisonOnOrBefore,
            _ => throw CreateUndefinedOperatorException(@operator),
        };

    private static InvalidOperationException CreateUndefinedOperatorException(DateComparisonOperator @operator)
        => new(string.Format(CultureInfo.CurrentCulture, Messages.DateComparisonUndefinedOperator, @operator));

    private static string GetDisplayName(PropertyInfo propertyInfo)
        => propertyInfo.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? propertyInfo.Name;

    private static bool IsSupportedDateType(Type? type)
        => type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
#if NET
            || type == typeof(DateOnly)
#endif
            ;

    private string FormatErrorMessage(string name, string otherName)
        => string.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, name, otherName);

    private void EnsureConfigurationIsValid()
    {
        if (!Enum.IsDefined(typeof(DateComparisonOperator), this.Operator))
        {
            throw CreateUndefinedOperatorException(this.Operator);
        }

        if (!IsSupportedDateType(this.ConversionType))
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.DateComparisonUnsupportedConversionType,
                    this.ConversionType));
        }
    }

    private PropertyInfo GetOtherPropertyInfo(Type objectType)
    {
        var propertyInfo = objectType.GetRuntimeProperty(this.OtherProperty);
        if (propertyInfo is null || propertyInfo.GetMethod is null || !propertyInfo.GetMethod.IsPublic)
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.DateComparisonOtherPropertyNotFound,
                    objectType,
                    this.OtherProperty));
        }

        if (propertyInfo.GetIndexParameters().Length > 0)
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.DateComparisonOtherPropertyIsIndexer,
                    objectType,
                    this.OtherProperty));
        }

        return propertyInfo;
    }

    /// <summary>
    ///  値を比較可能な日付型の値に変換します。
    /// </summary>
    /// <param name="value">変換する値。</param>
    /// <returns>
    ///  比較可能な日付型の値。
    ///  比較の対象としない値（ <see langword="null"/> 、空の文字列、日付型へ変換できない文字列）の場合は <see langword="null"/> 。
    /// </returns>
    private IComparable? ToComparable(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string text)
        {
            return this.ParseOrDefault(text);
        }

        if (!IsSupportedDateType(value.GetType()))
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.DateComparisonUnsupportedValueType,
                    value.GetType()));
        }

        return (IComparable)value;
    }

    private IComparable? ParseOrDefault(string text)
    {
        if (text.Length == 0)
        {
            return null;
        }

        var format = this.Format;
        if (this.ConversionType == typeof(DateTime))
        {
            var parsed = format is null
                ? DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateTime)
                : DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return parsed ? dateTime : null;
        }

        if (this.ConversionType == typeof(DateTimeOffset))
        {
            var parsed = format is null
                ? DateTimeOffset.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateTimeOffset)
                : DateTimeOffset.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset);
            return parsed ? dateTimeOffset : null;
        }

#if NET
        if (this.ConversionType == typeof(DateOnly))
        {
            var parsed = format is null
                ? DateOnly.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateOnly)
                : DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOnly);
            return parsed ? dateOnly : null;
        }
#endif

        // EnsureConfigurationIsValid で検証済みのため、ここには到達しません。
        throw new InvalidOperationException(
            string.Format(
                CultureInfo.CurrentCulture,
                Messages.DateComparisonUnsupportedConversionType,
                this.ConversionType));
    }

    private bool Satisfies(int comparisonResult)
        => this.Operator switch
        {
            DateComparisonOperator.After => comparisonResult > 0,
            DateComparisonOperator.OnOrAfter => comparisonResult >= 0,
            DateComparisonOperator.Before => comparisonResult < 0,
            DateComparisonOperator.OnOrBefore => comparisonResult <= 0,
            _ => throw CreateUndefinedOperatorException(this.Operator),
        };
}
