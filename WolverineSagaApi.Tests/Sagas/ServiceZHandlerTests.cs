using WolverineSagaApi.Events;
using WolverineSagaApi.Sagas;

namespace WolverineSagaApi.Tests.Sagas;

public class ServiceZHandlerTests
{
  [Fact]
  public void Handle_ServiceZCompleted_ShouldCompleteSaga()
  {
    // Arrange
    var saga = new ThreeServiceSaga
    {
      Id = "saga-123",
      RequestId = "req-123",
      InitialData = "test-data",
      ServiceXResult = "ServiceX-Result",
      ServiceYResult = "ServiceY-Result",
      CurrentStep = "ServiceZ"
    };
    var completed = new ServiceZCallCompleted("saga-123", true, "ServiceZ-Result", DateTime.UtcNow);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var result = saga.Handle(completed, logger.Object);

    // Assert
    saga.ServiceZResult.Should().Be("ServiceZ-Result");
    saga.CurrentStep.Should().Be("Completed");
    saga.CompletedAt.Should().NotBeNull();
    result.Should().BeOfType<ThreeServiceSagaCompleted>();
    result.ServiceXResult.Should().Be("ServiceX-Result");
    result.ServiceYResult.Should().Be("ServiceY-Result");
    result.ServiceZResult.Should().Be("ServiceZ-Result");
  }

  [Fact]
  public void Handle_ServiceZFailed_ShouldFailSaga()
  {
    // Arrange
    var saga = new ThreeServiceSaga
    {
      Id = "saga-123",
      RequestId = "req-123",
      InitialData = "test-data",
      ServiceXResult = "ServiceX-Result",
      ServiceYResult = "ServiceY-Result",
      CurrentStep = "ServiceZ"
    };
    var failed = new ServiceZCallFailed("saga-123", "ServiceZ error", DateTime.UtcNow);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var result = saga.Handle(failed, logger.Object);

    // Assert
    saga.CurrentStep.Should().Be("Failed");
    result.Should().BeOfType<ThreeServiceSagaFailed>();
    result.FailedService.Should().Be("ServiceZ");
    result.ErrorMessage.Should().Be("ServiceZ error");
  }
}
