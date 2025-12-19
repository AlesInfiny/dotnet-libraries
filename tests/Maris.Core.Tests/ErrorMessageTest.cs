namespace Maris.Core.Tests;

public class ErrorMessageTest
{
    [Fact]
    public void Constructor_SetsErrorMessageValuesToSpecifiedValues()
    {
        // Arrange & Act
        var message = "An error occurred. ID: {0}";
        var errorMessageValue1 = 1;
        var errorMessageValue2 = 2;
        ErrorMessage errorMessage = new ErrorMessage(message, errorMessageValue1, errorMessageValue2);

        // Assert
        Assert.Collection(
            errorMessage.ErrorMessageValues,
            v => Assert.Equal(v, errorMessageValue1),
            v => Assert.Equal(v, errorMessageValue2));
    }

    [Fact]
    public void Constructor_SetsFormattedMessageWithPlaceholderValues()
    {
        // Arrange & Act
        var message = "An error occurred. ID: {0}";
        var errorMessageValues = "1";
        ErrorMessage errorMessage = new ErrorMessage(message, errorMessageValues);

        // Assert
        Assert.Equal(string.Format(message, errorMessageValues), errorMessage.Message);
    }

    [Fact]
    public void Constructor_NullMessageSetsEmptyString()
    {
        // Arrange & Act
        ErrorMessage errorMessage = new ErrorMessage(null!);

        // Assert
        Assert.Empty(errorMessage.Message);
    }

    [Fact]
    public void ToString_ReturnsFormattedMessageWithPlaceholderValues()
    {
        // Arrange & Act
        var message = "An error occurred. ID: {0}";
        var errorMessageValues = "1";
        ErrorMessage errorMessage = new ErrorMessage(message, errorMessageValues);

        // Assert
        Assert.Equal(string.Format(message, errorMessageValues), errorMessage.ToString());
    }
}
