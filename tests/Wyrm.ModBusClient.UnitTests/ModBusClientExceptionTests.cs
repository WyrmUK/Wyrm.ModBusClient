using Shouldly;

namespace Wyrm.ModBusClient.UnitTests;

public class ModBusClientExceptionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_Should_Set_Values(bool withInnerException)
    {
        const string message = "Exception message";
        Exception? innerException = withInnerException ? new ArgumentException() : null;

        var exception = new ModBusClientException(message, ModBusExceptionCode.ConnectionError, innerException);

        exception.Message.ShouldBe(message);
        exception.ExceptionCode.ShouldBe(ModBusExceptionCode.ConnectionError);
        exception.InnerException.ShouldBe(innerException);
    }
}
