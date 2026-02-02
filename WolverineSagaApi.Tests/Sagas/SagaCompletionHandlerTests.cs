using WolverineSagaApi.Events;
using WolverineSagaApi.Sagas;

namespace WolverineSagaApi.Tests.Sagas;

public class SagaCompletionHandlerTests
{
  [Fact]
  public void Handle_SagaCompleted_ShouldLogCompletion()
  {
    // Arrange
    var completed = new ThreeServiceSagaCompleted(
      "saga-123",
      "ServiceX-Result",
      "ServiceY-Result",
      "ServiceZ-Result",
      DateTime.UtcNow
    );
    var logger = new Mock<ILogger<ThreeServiceSagaCompletedHandler>>();
    var handler = new ThreeServiceSagaCompletedHandler(logger.Object);

    // Act
    handler.Handle(completed);

    // Assert
    logger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("FINAL SAGA COMPLETION")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public void Handle_SagaFailed_ShouldLogError()
  {
    // Arrange
    var failed = new ThreeServiceSagaFailed(
      "saga-123",
      "ServiceY",
      "Service error occurred",
      DateTime.UtcNow
    );
    var logger = new Mock<ILogger<ThreeServiceSagaFailedHandler>>();
    var handler = new ThreeServiceSagaFailedHandler(logger.Object);

    // Act
    handler.Handle(failed);

    // Assert
    logger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SAGA FAILURE")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }
}
