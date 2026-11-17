using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Maris.ComponentModel.DataAnnotations;

namespace Maris.ComponentModel.Tests.DataAnnotations;

public class DateComparisonAttributeTest
{
    private static readonly DateTime BaseDateTime = new(2026, 11, 12, 10, 30, 0);

    public static TheoryData<DateComparisonOperator, int, bool> ComparisonCases => new()
    {
        // 比較方法, 検証対象の値と比較先の値の差（日）, 検証成功となるか
        { DateComparisonOperator.After, 1, true },
        { DateComparisonOperator.After, 0, false },
        { DateComparisonOperator.After, -1, false },
        { DateComparisonOperator.OnOrAfter, 1, true },
        { DateComparisonOperator.OnOrAfter, 0, true },
        { DateComparisonOperator.OnOrAfter, -1, false },
        { DateComparisonOperator.Before, 1, false },
        { DateComparisonOperator.Before, 0, false },
        { DateComparisonOperator.Before, -1, true },
        { DateComparisonOperator.OnOrBefore, 1, false },
        { DateComparisonOperator.OnOrBefore, 0, true },
        { DateComparisonOperator.OnOrBefore, -1, true },
    };

    public static TheoryData<DateComparisonOperator, string, string> DefaultErrorMessageCases => new()
    {
        // 比較方法, 日本語のメッセージ, 英語のメッセージ
        { DateComparisonOperator.After, "検証対象 は 比較先 より後にしてください。", "検証対象 must be after 比較先." },
        { DateComparisonOperator.OnOrAfter, "検証対象 は 比較先 以降にしてください。", "検証対象 must be on or after 比較先." },
        { DateComparisonOperator.Before, "検証対象 は 比較先 より前にしてください。", "検証対象 must be before 比較先." },
        { DateComparisonOperator.OnOrBefore, "検証対象 は 比較先 以前にしてください。", "検証対象 must be on or before 比較先." },
    };

    [Fact]
    public void Constructor_OtherPropertyIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => new DateComparisonAttribute(null!, DateComparisonOperator.After);

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("otherProperty", exception.ParamName);
    }

    [Fact]
    public void Constructor_SetsPropertiesAndDefaultValues()
    {
        // Arrange & Act
        var attribute = new DateComparisonAttribute("Other", DateComparisonOperator.OnOrBefore);

        // Assert
        Assert.Equal("Other", attribute.OtherProperty);
        Assert.Equal(DateComparisonOperator.OnOrBefore, attribute.Operator);
        Assert.Equal(typeof(DateTime), attribute.ConversionType);
        Assert.Null(attribute.Format);
        Assert.True(attribute.RequiresValidationContext);
    }

    [Fact]
    public void TypeId_DiffersForEachInstance()
    {
        // Arrange
        var attribute1 = new DateComparisonAttribute("Other", DateComparisonOperator.After);
        var attribute2 = new DateComparisonAttribute("Other", DateComparisonOperator.After);

        // Act & Assert
        Assert.NotEqual(attribute1.TypeId, attribute2.TypeId);
        Assert.Same(attribute1.TypeId, attribute1.TypeId);
    }

    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void GetValidationResult_DateTime_ComparesByOperator(DateComparisonOperator @operator, int offsetDays, bool expected)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), @operator);
        var pair = new Pair { Target = BaseDateTime.AddDays(offsetDays), Other = BaseDateTime };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Equal(expected, result is null);
    }

    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void GetValidationResult_DateTimeOffset_ComparesByOperator(DateComparisonOperator @operator, int offsetDays, bool expected)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), @operator);
        var other = new DateTimeOffset(BaseDateTime, TimeSpan.FromHours(9));
        var pair = new Pair { Target = other.AddDays(offsetDays), Other = other };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Equal(expected, result is null);
    }

    [Fact]
    public void GetValidationResult_DateTimeOffset_ComparesAsPointInTime()
    {
        // Arrange
        // 見た目の時刻は検証対象の値のほうが前だが、時点としては検証対象の値のほうが後。
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair
        {
            Target = new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero),
            Other = new DateTimeOffset(2026, 11, 12, 10, 0, 0, TimeSpan.FromHours(9)),
        };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValidationResult_DateTime_IgnoresKind()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.OnOrAfter);
        var pair = new Pair
        {
            Target = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc),
            Other = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Local),
        };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

#if NET
    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void GetValidationResult_DateOnly_ComparesByOperator(DateComparisonOperator @operator, int offsetDays, bool expected)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), @operator);
        var other = DateOnly.FromDateTime(BaseDateTime);
        var pair = new Pair { Target = other.AddDays(offsetDays), Other = other };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Equal(expected, result is null);
    }

    [Fact]
    public void GetValidationResult_StringAndDateOnly_ConvertsStringToDateOnly()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.OnOrAfter)
        {
            ConversionType = typeof(DateOnly),
            Format = "yyyy/MM/dd",
        };
        var valid = new Pair { Target = "2026/11/12", Other = new DateOnly(2026, 11, 12) };
        var invalid = new Pair { Target = "2026/11/11", Other = new DateOnly(2026, 11, 12) };

        // Act
        var validResult = Validate(attribute, valid);
        var invalidResult = Validate(attribute, invalid);

        // Assert
        Assert.Null(validResult);
        Assert.NotNull(invalidResult);
    }

    [Fact]
    public void GetValidationResult_StringAndDateOnlyWithoutConversionType_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.OnOrAfter)
        {
            Format = "yyyy/MM/dd",
        };
        var pair = new Pair { Target = "2026/11/12", Other = new DateOnly(2026, 11, 12) };

        // Act
        var action = () => Validate(attribute, pair);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }
#endif

    [Theory]
    [InlineData(true, false)] // 検証対象の値のみ null
    [InlineData(false, true)] // 比較先の値のみ null
    [InlineData(true, true)] // 両方の値が null
    public void GetValidationResult_EitherValueIsNull_ReturnsSuccess(bool targetIsNull, bool otherIsNull)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair
        {
            Target = targetIsNull ? null : BaseDateTime.AddDays(-1),
            Other = otherIsNull ? null : BaseDateTime,
        };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "2026/11/12")] // 検証対象の値のみ空の文字列
    [InlineData("2026/11/11", "")] // 比較先の値のみ空の文字列
    [InlineData("", "")] // 両方の値が空の文字列
    public void GetValidationResult_EitherValueIsEmptyString_ReturnsSuccess(string target, string other)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After)
        {
            Format = "yyyy/MM/dd",
        };
        var pair = new Pair { Target = target, Other = other };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("not a date", "2026/11/12")] // 検証対象の値が日付ではない文字列
    [InlineData("2026/11/11", "not a date")] // 比較先の値が日付ではない文字列
    [InlineData("2026-11-11", "2026/11/12")] // 検証対象の値の区切り文字が Format と異なる
    [InlineData("2026/11/11 ", "2026/11/12")] // 検証対象の値の末尾に空白があり、 Format に厳密には一致しない
    [InlineData("2026/13/01", "2026/11/12")] // 検証対象の値が Format には一致するが、存在しない日付（ 13 月）
    public void GetValidationResult_StringCannotBeConverted_ReturnsSuccess(string target, string other)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After)
        {
            Format = "yyyy/MM/dd",
        };
        var pair = new Pair { Target = target, Other = other };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void GetValidationResult_BothStrings_ConvertsToDateTimeByDefault(DateComparisonOperator @operator, int offsetDays, bool expected)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), @operator)
        {
            Format = "yyyyMMdd",
        };
        var pair = new Pair
        {
            // 文字列として比較した場合と結果が異なるよう、年をまたぐ値を使用する。
            Target = new DateTime(2026, 12, 31).AddDays(offsetDays).ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            Other = "20261231",
        };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Equal(expected, result is null);
    }

    [Fact]
    public void GetValidationResult_StringAndDateTime_ConvertsStringToDateTime()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.Before)
        {
            Format = "yyyy/MM/dd HH:mm",
        };
        var valid = new Pair { Target = "2026/11/12 10:29", Other = BaseDateTime };
        var invalid = new Pair { Target = "2026/11/12 10:30", Other = BaseDateTime };

        // Act
        var validResult = Validate(attribute, valid);
        var invalidResult = Validate(attribute, invalid);

        // Assert
        Assert.Null(validResult);
        Assert.NotNull(invalidResult);
    }

    [Fact]
    public void GetValidationResult_DateTimeAndString_ConvertsOtherStringToDateTime()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After)
        {
            Format = "yyyy/MM/dd HH:mm",
        };
        var valid = new Pair { Target = BaseDateTime, Other = "2026/11/12 10:29" };
        var invalid = new Pair { Target = BaseDateTime, Other = "2026/11/12 10:30" };

        // Act
        var validResult = Validate(attribute, valid);
        var invalidResult = Validate(attribute, invalid);

        // Assert
        Assert.Null(validResult);
        Assert.NotNull(invalidResult);
    }

    [Fact]
    public void GetValidationResult_StringsWithOffset_ConvertsToDateTimeOffset()
    {
        // Arrange
        // 見た目の時刻は検証対象の値のほうが前だが、時点としては検証対象の値のほうが後。
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After)
        {
            ConversionType = typeof(DateTimeOffset),
            Format = "yyyy-MM-ddTHH:mm:ssK",
        };
        var pair = new Pair
        {
            Target = "2026-11-12T09:00:00+00:00",
            Other = "2026-11-12T10:00:00+09:00",
        };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("ja-JP", "2026/12/01", "2026/11/13", true)] // 年/月/日の書式で、検証対象の値（ 12 月 1 日）が比較先の値（ 11 月 13 日）より後
    [InlineData("ja-JP", "2026/11/13", "2026/12/01", false)] // 年/月/日の書式で、検証対象の値（ 11 月 13 日）が比較先の値（ 12 月 1 日）より前
    [InlineData("en-GB", "01/12/2026", "13/11/2026", true)] // 日/月/年の書式で、検証対象の値（ 12 月 1 日）が比較先の値（ 11 月 13 日）より後
    [InlineData("en-GB", "13/11/2026", "01/12/2026", false)] // 日/月/年の書式で、検証対象の値（ 11 月 13 日）が比較先の値（ 12 月 1 日）より前
    public void GetValidationResult_FormatIsNull_ConvertsWithCurrentCulture(string culture, string target, string other, bool expected)
    {
        // Arrange
        using var scope = new CultureScope(culture);
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair { Target = target, Other = other };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Equal(expected, result is null);
    }

    [Fact]
    public void GetValidationResult_FormatIsSet_ConvertsWithoutDependingOnCurrentCulture()
    {
        // Arrange
        // en-GB の既定の書式（dd/MM/yyyy）ではなく、 Format の書式で変換される。
        using var scope = new CultureScope("en-GB");
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After)
        {
            Format = "MM/dd/yyyy",
        };
        var pair = new Pair { Target = "12/01/2026", Other = "11/02/2026" };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DefaultErrorMessageCases))]
    public void GetValidationResult_Invalid_ReturnsDefaultErrorMessageInJapanese(DateComparisonOperator @operator, string japanese, string english)
    {
        // Arrange
        _ = english;
        using var scope = new CultureScope("ja-JP");
        var attribute = new DateComparisonAttribute(nameof(DisplayNamePair.Other), @operator);
        var pair = CreateInvalidDisplayNamePair(@operator);

        // Act
        var result = Validate(attribute, pair, nameof(DisplayNamePair.Target));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(japanese, result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(DefaultErrorMessageCases))]
    public void GetValidationResult_Invalid_ReturnsDefaultErrorMessageInEnglish(DateComparisonOperator @operator, string japanese, string english)
    {
        // Arrange
        _ = japanese;
        using var scope = new CultureScope("en-US");
        var attribute = new DateComparisonAttribute(nameof(DisplayNamePair.Other), @operator);
        var pair = CreateInvalidDisplayNamePair(@operator);

        // Act
        var result = Validate(attribute, pair, nameof(DisplayNamePair.Target));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(english, result.ErrorMessage);
    }

    [Fact]
    public void GetValidationResult_InvalidWithOtherCulture_ReturnsDefaultErrorMessageInJapanese()
    {
        // Arrange
        using var scope = new CultureScope("fr-FR");
        var attribute = new DateComparisonAttribute(nameof(DisplayNamePair.Other), DateComparisonOperator.After);
        var pair = CreateInvalidDisplayNamePair(DateComparisonOperator.After);

        // Act
        var result = Validate(attribute, pair, nameof(DisplayNamePair.Target));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("検証対象 は 比較先 より後にしてください。", result.ErrorMessage);
    }

    [Fact]
    public void GetValidationResult_InvalidWithoutDisplayAttribute_UsesPropertyNames()
    {
        // Arrange
        using var scope = new CultureScope("ja-JP");
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair { Target = BaseDateTime, Other = BaseDateTime };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Target は Other より後にしてください。", result.ErrorMessage);
    }

    [Fact]
    public void GetValidationResult_InvalidWithCustomErrorMessage_UsesCustomErrorMessage()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(DisplayNamePair.Other), DateComparisonOperator.After)
        {
            ErrorMessage = "{1} < {0} となるようにしてください。",
        };
        var pair = CreateInvalidDisplayNamePair(DateComparisonOperator.After);

        // Act
        var result = Validate(attribute, pair, nameof(DisplayNamePair.Target));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("比較先 < 検証対象 となるようにしてください。", result.ErrorMessage);
    }

    [Fact]
    public void GetValidationResult_InvalidWithErrorMessageResource_UsesErrorMessageOfResource()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(DisplayNamePair.Other), DateComparisonOperator.After)
        {
            ErrorMessageResourceType = typeof(TestMessages),
            ErrorMessageResourceName = nameof(TestMessages.TargetMustBeAfterOther),
        };
        var pair = CreateInvalidDisplayNamePair(DateComparisonOperator.After);

        // Act
        var result = Validate(attribute, pair, nameof(DisplayNamePair.Target));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("リソースのメッセージ：検証対象 は 比較先 より後です。", result.ErrorMessage);
    }

    [Fact]
    public void GetValidationResult_Invalid_ReturnsOnlyValidatedMemberName()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair { Target = BaseDateTime, Other = BaseDateTime };

        // Act
        var result = Validate(attribute, pair);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([nameof(Pair.Target)], result.MemberNames);
    }

    [Fact]
    public void GetValidationResult_InvalidWithoutMemberName_ReturnsNoMemberNames()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair { Target = BaseDateTime, Other = BaseDateTime };
        var context = new ValidationContext(pair);

        // Act
        var result = attribute.GetValidationResult(pair.Target, context);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.MemberNames);
    }

    [Fact]
    public void FormatErrorMessage_UsesOtherPropertyAsOtherName()
    {
        // Arrange
        using var scope = new CultureScope("ja-JP");
        var attribute = new DateComparisonAttribute("StartDate", DateComparisonOperator.OnOrAfter);

        // Act
        var message = attribute.FormatErrorMessage("終了日");

        // Assert
        Assert.Equal("終了日 は StartDate 以降にしてください。", message);
    }

    [Theory]
    [InlineData("NotExists")] // 存在しないプロパティ
    [InlineData("PrivateOther")] // 公開されていないプロパティ
    [InlineData("WriteOnlyOther")] // 値を取得できない、書き込み専用のプロパティ
    public void GetValidationResult_OtherPropertyIsNotAccessible_ThrowsInvalidOperationException(string otherProperty)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(otherProperty, DateComparisonOperator.After);
        var pair = new Pair { Target = BaseDateTime, Other = BaseDateTime };

        // Act
        var action = () => Validate(attribute, pair);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(otherProperty, exception.Message);
    }

    [Fact]
    public void GetValidationResult_OtherPropertyIsIndexer_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new DateComparisonAttribute("Item", DateComparisonOperator.After);
        var model = new IndexerModel { Target = BaseDateTime };
        var context = new ValidationContext(model) { MemberName = nameof(IndexerModel.Target) };

        // Act
        var action = () => attribute.GetValidationResult(model.Target, context);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Theory]
    [InlineData(true)] // 検証対象の値が対応していない型（ int ）
    [InlineData(false)] // 比較先の値が対応していない型（ int ）
    public void GetValidationResult_ValueTypeIsNotSupported_ThrowsInvalidOperationException(bool targetIsUnsupported)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair
        {
            Target = targetIsUnsupported ? 20261112 : BaseDateTime,
            Other = targetIsUnsupported ? BaseDateTime : 20261112,
        };

        // Act
        var action = () => Validate(attribute, pair);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(typeof(int).ToString(), exception.Message);
    }

    [Fact]
    public void GetValidationResult_ValueTypesDoNotMatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After);
        var pair = new Pair { Target = BaseDateTime, Other = new DateTimeOffset(BaseDateTime, TimeSpan.Zero) };

        // Act
        var action = () => Validate(attribute, pair);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(typeof(DateTime).ToString(), exception.Message);
        Assert.Contains(typeof(DateTimeOffset).ToString(), exception.Message);
    }

    [Theory]
    [InlineData(typeof(int))] // 日付ではない型
    [InlineData(typeof(string))] // 変換元の型である string
    [InlineData(typeof(TimeSpan))] // 日付に関連するが、対応していない型
    [InlineData(null)] // 型を設定していない
    public void GetValidationResult_ConversionTypeIsNotSupported_ThrowsInvalidOperationException(Type? conversionType)
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), DateComparisonOperator.After)
        {
            ConversionType = conversionType!,
        };
        var pair = new Pair { Target = BaseDateTime.AddDays(1), Other = BaseDateTime };

        // Act
        var action = () => Validate(attribute, pair);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void GetValidationResult_OperatorIsNotDefined_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new DateComparisonAttribute(nameof(Pair.Other), (DateComparisonOperator)99);
        var pair = new Pair { Target = BaseDateTime.AddDays(1), Other = BaseDateTime };

        // Act
        var action = () => Validate(attribute, pair);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("99", exception.Message);
    }

    [Fact]
    public void TryValidateObject_ValidPeriod_ReturnsTrue()
    {
        // Arrange
        var period = new Period
        {
            StartDate = new DateTime(2026, 11, 12),
            EndDate = new DateTime(2026, 11, 12),
        };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(period, new ValidationContext(period), results, validateAllProperties: true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void TryValidateObject_InvalidPeriod_ReturnsErrorOfAnnotatedProperty()
    {
        // Arrange
        using var scope = new CultureScope("ja-JP");
        var period = new Period
        {
            StartDate = new DateTime(2026, 11, 12),
            EndDate = new DateTime(2026, 11, 11),
        };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(period, new ValidationContext(period), results, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        var result = Assert.Single(results);
        Assert.Equal("終了日 は 開始日 以降にしてください。", result.ErrorMessage);
        Assert.Equal([nameof(Period.EndDate)], result.MemberNames);
    }

    [Fact]
    public void TryValidateObject_MultipleAttributes_AllSatisfied_ReturnsTrue()
    {
        // Arrange
        var schedule = new Schedule
        {
            StartDate = new DateTime(2026, 11, 1),
            CheckDate = new DateTime(2026, 11, 15),
            EndDate = new DateTime(2026, 11, 30),
        };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(schedule, new ValidationContext(schedule), results, validateAllProperties: true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("2026/10/31", "CheckDate は StartDate より後にしてください。")] // 開始日（ 11 月 1 日）より前で、 1 つ目の属性の条件のみ満たさない
    [InlineData("2026/12/01", "CheckDate は EndDate より前にしてください。")] // 終了日（ 11 月 30 日）より後で、 2 つ目の属性の条件のみ満たさない
    public void TryValidateObject_MultipleAttributes_EvaluatesEachAttribute(string checkDate, string expectedMessage)
    {
        // Arrange
        using var scope = new CultureScope("ja-JP");
        var schedule = new Schedule
        {
            StartDate = new DateTime(2026, 11, 1),
            CheckDate = DateTime.ParseExact(checkDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
            EndDate = new DateTime(2026, 11, 30),
        };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(schedule, new ValidationContext(schedule), results, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        var result = Assert.Single(results);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    [Fact]
    public void TryValidateObject_MultipleAttributes_AllViolated_ReturnsAllErrors()
    {
        // Arrange
        using var scope = new CultureScope("ja-JP");
        var schedule = new Schedule
        {
            // 開始日と終了日が逆転しているため、両方の条件を満たさない。
            StartDate = new DateTime(2026, 11, 30),
            CheckDate = new DateTime(2026, 11, 15),
            EndDate = new DateTime(2026, 11, 1),
        };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(schedule, new ValidationContext(schedule), results, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.ErrorMessage == "CheckDate は StartDate より後にしてください。");
        Assert.Contains(results, r => r.ErrorMessage == "CheckDate は EndDate より前にしてください。");
    }

    [Fact]
    public void TryValidateObject_StringProperties_ConvertsAndCompares()
    {
        // Arrange
        var valid = new SearchCondition { StartDate = "2026/11/12", EndDate = "2026/11/12" };
        var invalid = new SearchCondition { StartDate = "2026/11/12", EndDate = "2026/11/11" };
        var validResults = new List<ValidationResult>();
        var invalidResults = new List<ValidationResult>();

        // Act
        var validIsValid = Validator.TryValidateObject(valid, new ValidationContext(valid), validResults, validateAllProperties: true);
        var invalidIsValid = Validator.TryValidateObject(invalid, new ValidationContext(invalid), invalidResults, validateAllProperties: true);

        // Assert
        Assert.True(validIsValid);
        Assert.False(invalidIsValid);
        var result = Assert.Single(invalidResults);
        Assert.Equal([nameof(SearchCondition.EndDate)], result.MemberNames);
    }

    private static ValidationResult? Validate(DateComparisonAttribute attribute, Pair pair)
    {
        var context = new ValidationContext(pair) { MemberName = nameof(Pair.Target) };
        return attribute.GetValidationResult(pair.Target, context);
    }

    private static ValidationResult? Validate(DateComparisonAttribute attribute, DisplayNamePair pair, string memberName)
    {
        var context = new ValidationContext(pair) { MemberName = memberName };
        return attribute.GetValidationResult(pair.Target, context);
    }

    private static DisplayNamePair CreateInvalidDisplayNamePair(DateComparisonOperator @operator)
    {
        // After / OnOrAfter は検証対象の値を前に、 Before / OnOrBefore は検証対象の値を後にして、検証エラーにする。
        var offsetDays = @operator is DateComparisonOperator.After or DateComparisonOperator.OnOrAfter ? -1 : 1;
        return new DisplayNamePair { Target = BaseDateTime.AddDays(offsetDays), Other = BaseDateTime };
    }

    private static class TestMessages
    {
        public static string TargetMustBeAfterOther => "リソースのメッセージ：{0} は {1} より後です。";
    }

    private sealed class Pair
    {
        public object? Target { get; set; }

        public object? Other { get; set; }

        public object? WriteOnlyOther
        {
            set => _ = value;
        }

        private object? PrivateOther { get; set; }
    }

    private sealed class DisplayNamePair
    {
        [Display(Name = "検証対象")]
        public DateTime? Target { get; set; }

        [Display(Name = "比較先")]
        public DateTime? Other { get; set; }
    }

    private sealed class IndexerModel
    {
        public DateTime? Target { get; set; }

        public DateTime this[int index] => BaseDateTime.AddDays(index);
    }

    private sealed class Period
    {
        [Display(Name = "開始日")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "終了日")]
        [DateComparison(nameof(StartDate), DateComparisonOperator.OnOrAfter)]
        public DateTime? EndDate { get; set; }
    }

    private sealed class Schedule
    {
        public DateTime? StartDate { get; set; }

        [DateComparison(nameof(StartDate), DateComparisonOperator.After)]
        [DateComparison(nameof(EndDate), DateComparisonOperator.Before)]
        public DateTime? CheckDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    private sealed class SearchCondition
    {
        public string? StartDate { get; set; }

        [DateComparison(nameof(StartDate), DateComparisonOperator.OnOrAfter, Format = "yyyy/MM/dd")]
        public string? EndDate { get; set; }
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo originalUICulture = CultureInfo.CurrentUICulture;

        public CultureScope(string name)
        {
            var culture = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = this.originalCulture;
            CultureInfo.CurrentUICulture = this.originalUICulture;
        }
    }
}
