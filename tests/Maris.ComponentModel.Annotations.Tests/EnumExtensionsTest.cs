using System.ComponentModel.DataAnnotations;

namespace Maris.ComponentModel.Tests;

public class EnumExtensionsTest
{
    private enum Status
    {
        [Display(Name = "Preparation is ready.")]
        Ready = 1,

        [Display(ResourceType = typeof(EnumExtensionsTestResources), Name = nameof(EnumExtensionsTestResources.InProgress))]
        InProgress = 2,
        Done = 3,
    }

    [Flags]
    private enum DaysOfWeek
    {
        None = 0,

        [Display(Name = "MONDAY")]
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,

        [Display(Name = "WEEKDAYS")]
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        [Display(ResourceType = typeof(EnumExtensionsTestResources), Name = nameof(EnumExtensionsTestResources.Weekend))]
        Weekend = Saturday | Sunday,
        All = Weekdays | Weekend,
    }

    [Fact]
    public void GetDisplayName_CanGetSimpleDisplayName()
    {
        // Arrange
        var ready = Status.Ready;

        // Act
        var displayName = ready.GetDisplayName();

        // Assert
        Assert.Equal("Preparation is ready.", displayName);
    }

    [Fact]
    public void GetDisplayName_DisplayNameGetFromResource()
    {
        // Arrange
        var inProgress = Status.InProgress;

        // Act
        var displayName = inProgress.GetDisplayName();

        // Assert
        Assert.Equal("Work is in progress.", displayName);
    }

    [Fact]
    public void GetDisplayName_IfNoDisplayAttributeReturnEnumName()
    {
        // Arrange
        var done = Status.Done;

        // Act
        var displayName = done.GetDisplayName();

        // Assert
        Assert.Equal(nameof(Status.Done), displayName);
    }

    [Fact]
    public void GetDisplayName_NotDefinedNameReturnEnumValue()
    {
        // Arrange
        var defaultStatus = default(Status); // 0

        // Act
        var displayName = defaultStatus.GetDisplayName();

        // Assert
        Assert.Equal("0", displayName);
    }

    [Fact]
    public void GetDisplayName_Flags_CanGetSimpleDisplayName()
    {
        // Arrange
        var weekdays = DaysOfWeek.Weekdays;

        // Act
        var displayName = weekdays.GetDisplayName();

        // Assert
        Assert.Equal("WEEKDAYS", displayName);
    }

    [Fact]
    public void GetDisplayName_Flags_DisplayNameGetFromResource()
    {
        // Arrange
        var weekend = DaysOfWeek.Weekend;

        // Act
        var displayName = weekend.GetDisplayName();

        // Assert
        Assert.Equal("WEEKEND", displayName);
    }

    [Fact]
    public void GetDisplayName_Flags_IfNoDisplayAttributeReturnEnumName()
    {
        // Arrange
        var all = DaysOfWeek.All;

        // Act
        var displayName = all.GetDisplayName();

        // Assert
        Assert.Equal(nameof(DaysOfWeek.All), displayName);
    }

    [Fact]
    public void GetDisplayName_Flags_ReturnsStringThatConcatenatesEachEnumNameForUndefinedCombinationsOfFlags()
    {
        // Arrange
        var monAndTue = DaysOfWeek.Monday | DaysOfWeek.Tuesday;

        // Act
        var displayName = monAndTue.GetDisplayName();

        // Assert
        Assert.Equal(monAndTue.ToString(), displayName);
    }
}
