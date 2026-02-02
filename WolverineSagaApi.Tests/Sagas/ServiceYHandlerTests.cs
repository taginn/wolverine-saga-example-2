using WolverineSagaApi.Events;
using WolverineSagaApi.Sagas;

namespace WolverineSagaApi.Tests.Sagas;

public class ServiceYHandlerTests
{
  [Fact]
  public void Handle_ServiceYCompleted_ShouldTransitionToServiceZ()
  {
    // Arrange
    var saga = new ThreeServiceSaga
    {
      Id = "saga-123",
      RequestId = "req-123",
      InitialData = "test-data",
      ServiceXResult = "ServiceX-Result",
      CurrentStep = "ServiceY"
    };
    var completed = new ServiceYCallCompleted("saga-123", true, "ServiceY-Result", DateTime.UtcNow);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var command = saga.Handle(completed, logger.Object);

    // Assert
    saga.ServiceYResult.Should().Be("ServiceY-Result");
    saga.CurrentStep.Should().Be("ServiceZ");
    command.Should().BeOfType<ServiceZCallRequested>();
    command.SagaId.Should().Be("saga-123");
    command.ServiceYResult.Should().Be("ServiceY-Result");
  }

  [Fact]
  public void Handle_ServiceYFailed_ShouldFailSaga()
  {
    // Arrange
    var saga = new ThreeServiceSaga
    {
      Id = "saga-123",
      RequestId = "req-123",
      InitialData = "test-data",
      ServiceXResult = "ServiceX-Result",
      CurrentStep = "ServiceY"
    };
    var failed = new ServiceYCallFailed("saga-123", "ServiceY error", DateTime.UtcNow);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var result = saga.Handle(failed, logger.Object);

    // Assert
    saga.CurrentStep.Should().Be("Failed");
    result.Should().BeOfType<ThreeServiceSagaFailed>();
    result.FailedService.Should().Be("ServiceY");
    result.ErrorMessage.Should().Be("ServiceY error");
  }
}
