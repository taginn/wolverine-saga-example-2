using WolverineSagaApi.Events;
using WolverineSagaApi.Models;
using WolverineSagaApi.Sagas;
using WolverineSagaApi.Services;

namespace WolverineSagaApi.Tests.Sagas;

public class ServiceYCallRequestedHandlerTests
{
  [Fact]
  public async Task Handle_Success_ShouldReturnServiceYCallCompleted()
  {
    // Arrange
    var serviceYClient = new Mock<IServiceYClient>();
    var logger = new Mock<ILogger<ServiceYCallRequestedHandler>>();
    var handler = new ServiceYCallRequestedHandler(serviceYClient.Object, logger.Object);

    var request = new ServiceYCallRequested("saga-123", "test-data", "ServiceX-Result", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceY",
      true,
      "ServiceY-Enhanced-ServiceX-Result",
      DateTime.UtcNow,
      new Dictionary<string, object> { { "duration_ms", 450 } }
    );

    serviceYClient
      .Setup(x => x.CallAsync(request.Data, request.ServiceXResult, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceYCallCompleted>();
    var completed = (ServiceYCallCompleted)result;
    completed.SagaId.Should().Be("saga-123");
    completed.Success.Should().BeTrue();
    completed.ResponseData.Should().Be("ServiceY-Enhanced-ServiceX-Result");
  }

  [Fact]
  public async Task Handle_Failure_ShouldReturnServiceYCallFailed()
  {
    // Arrange
    var serviceYClient = new Mock<IServiceYClient>();
    var logger = new Mock<ILogger<ServiceYCallRequestedHandler>>();
    var handler = new ServiceYCallRequestedHandler(serviceYClient.Object, logger.Object);

    var request = new ServiceYCallRequested("saga-123", "test-data", "ServiceX-Result", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceY",
      false,
      string.Empty,
      DateTime.UtcNow,
      new Dictionary<string, object> { { "error", "Service unavailable" } }
    );

    serviceYClient
      .Setup(x => x.CallAsync(request.Data, request.ServiceXResult, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceYCallFailed>();
    var failed = (ServiceYCallFailed)result;
    failed.SagaId.Should().Be("saga-123");
    failed.ErrorMessage.Should().Be("Service unavailable");
  }

  [Fact]
  public async Task Handle_FailureWithoutErrorMetric_ShouldReturnGenericMessage()
  {
    // Arrange
    var serviceYClient = new Mock<IServiceYClient>();
    var logger = new Mock<ILogger<ServiceYCallRequestedHandler>>();
    var handler = new ServiceYCallRequestedHandler(serviceYClient.Object, logger.Object);

    var request = new ServiceYCallRequested("saga-123", "test-data", "ServiceX-Result", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceY",
      false,
      string.Empty,
      DateTime.UtcNow,
      null
    );

    serviceYClient
      .Setup(x => x.CallAsync(request.Data, request.ServiceXResult, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceYCallFailed>();
    var failed = (ServiceYCallFailed)result;
    failed.SagaId.Should().Be("saga-123");
    failed.ErrorMessage.Should().Be("ServiceY call failed");
  }
}
