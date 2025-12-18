namespace Maris.Core.Tests;

public class BusinessErrorTest
{
    [Fact]
    public void Constructor_ParameterlessCtor_SetsExceptionIdToEmptyString()
    {
        // Arrange & Act
        var error = new BusinessError();

        // Assert
        Assert.Equal(string.Empty, error.ExceptionId);
    }

    [Fact]
    public void Constructor_ParameterlessCtor_SetsErrorMessagesToEmptyList()
    {
        // Arrange & Act
        var error = new BusinessError();

        // Assert
        Assert.Empty(error.ErrorMessages);
    }

    [Fact]
    public void Constructor_WithParameters_SetsExceptionIdToSpecifiedValue()
    {
        // Arrange
        string? exceptionId = "ERR_CODE";
        ErrorMessage[] errorMessages = [new ErrorMessage("ERR_MESSAGE1"), new ErrorMessage("ERR_MESSAGE2")];

        // Act
        var error = new BusinessError(exceptionId, errorMessages);

        // Assert
        Assert.Equal(exceptionId, error.ExceptionId);
    }

    [Fact]
    public void Constructor_WithParameters_SetsErrorMessagesToSpecifiedValues()
    {
        // Arrange
        string? exceptionId = "ERR_CODE";
        ErrorMessage[] errorMessages = [new ErrorMessage("ERR_MESSAGE1"), new ErrorMessage("ERR_MESSAGE2")];

        // Act
        var error = new BusinessError(exceptionId, errorMessages);

        // Assert
        Assert.Collection(
            error.ErrorMessages.Select(e => e.Message),
            errorMessage => Assert.Equal(errorMessages[0].Message, errorMessage),
            errorMessage => Assert.Equal(errorMessages[1].Message, errorMessage));
    }

    [Fact]
    public void AddErrorMessage_AddsErrorMessageToList()
    {
        // Arrange
        var error = new BusinessError("ERR_CODE", new ErrorMessage("ERR_MESSAGE1"));
        ErrorMessage errorMessage = new ErrorMessage("ERR_MESSAGE2");

        // Act
        error.AddErrorMessage(errorMessage);

        // Assert
        Assert.Collection(
            error.ErrorMessages,
            e => Assert.Equal("ERR_MESSAGE1", e.Message),
            e => Assert.Equal("ERR_MESSAGE2", e.Message));
    }

    [Fact]
    public void ToString_WhenExceptionIdNotSet_ReturnsJsonWithEmptyKey()
    {
        // Arrange
        var error = new BusinessError();

        // Act
        var str = error.ToString();

        // Assert
        Assert.Equal("{\"\":[]}", str);
    }

    [Fact]
    public void ToString_WhenExceptionIdSet_ReturnsJsonWithMessages()
    {
        // Arrange
        var error = new BusinessError("ERR_CODE", new ErrorMessage("エラー1"), new ErrorMessage("ERR_MESSAGE2"));

        // Act
        var str = error.ToString();

        // Assert
        Assert.Equal("{\"ERR_CODE\":[\"エラー1\",\"ERR_MESSAGE2\"]}", str);
    }
}
