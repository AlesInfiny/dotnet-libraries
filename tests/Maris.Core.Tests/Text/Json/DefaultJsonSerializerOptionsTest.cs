using Maris.Core.Text.Json;

namespace Maris.Core.Tests.Text.Json;

public class DefaultJsonSerializerOptionsTest
{
    [Fact]
    public void GetInstance_ReturnsInstance()
    {
        // Arrange & Act
        var options = DefaultJsonSerializerOptions.GetInstance();

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void GetInstance_ReturnsSameInstanceWhenCalledMultipleTimes()
    {
        // Arrange & Act
        var options1 = DefaultJsonSerializerOptions.GetInstance();
        var options2 = DefaultJsonSerializerOptions.GetInstance();

        // Assert
        Assert.Equal(options1, options2);
    }

    [Fact]
    public void GetInstance_ConfiguresEncoderAsExpected()
    {
        // Arrange & Act
        var options = DefaultJsonSerializerOptions.GetInstance();

        // Assert
        // UnicodeRanges.All の範囲がエンコード可能か確認
        Assert.NotNull(options.Encoder);
        Assert.True(options.Encoder.WillEncode(0x0000));
        Assert.True(options.Encoder.WillEncode(0xFFFF));
        Assert.True(options.Encoder.WillEncode(0x3040));
    }
}
