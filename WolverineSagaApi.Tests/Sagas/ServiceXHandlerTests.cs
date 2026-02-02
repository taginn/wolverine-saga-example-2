using WolverineSagaApi.Events;
using WolverineSagaApi.Sagas;

namespace WolverineSagaApi.Tests.Sagas;

public class ServiceXHandlerTests
{
  [Fact]
  public void Handle_ServiceXCompleted_ShouldTransitionToServiceY()
  {
    // Arrange
    var saga = new ThreeServiceSaga
    {
      Id = "saga-123",
      RequestId = "req-123",
      InitialData = "test-data",
      CurrentStep = "ServiceX"
    };
    var completed = new ServiceXCallCompleted("saga-123", true, "ServiceX-Result", DateTime.UtcNow);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var command = saga.Handle(completed, logger.Object);

    // Assert
    saga.ServiceXResult.Should().Be("ServiceX-Result");
    saga.CurrentStep.Should().Be("ServiceY");
    command.Should().BeOfType<ServiceYCallRequested>();
    command.SagaId.Should().Be("saga-123");
    command.ServiceXResult.Should().Be("ServiceX-Result");
  }

  [Fact]
  public void Handle_ServiceXFailed_ShouldFailSaga()
  {
    // Arrange
    var saga = new ThreeServiceSaga
    {
      Id = "saga-123",
      RequestId = "req-123",
      InitialData = "test-data",
      CurrentStep = "ServiceX"
    };
    var failed = new ServiceXCallFailed("saga-123", "ServiceX error", DateTime.UtcNow);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var result = saga.Handle(failed, logger.Object);

    // Assert
    saga.CurrentStep.Should().Be("Failed");
    result.Should().BeOfType<ThreeServiceSagaFailed>();
    result.FailedService.Should().Be("ServiceX");
    result.ErrorMessage.Should().Be("ServiceX error");
  }
}
