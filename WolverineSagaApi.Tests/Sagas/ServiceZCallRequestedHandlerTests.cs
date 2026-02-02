using WolverineSagaApi.Events;
using WolverineSagaApi.Models;
using WolverineSagaApi.Sagas;
using WolverineSagaApi.Services;

namespace WolverineSagaApi.Tests.Sagas;

public class ServiceZCallRequestedHandlerTests
{
  [Fact]
  public async Task Handle_Success_ShouldReturnServiceZCallCompleted()
  {
    // Arrange
    var serviceZClient = new Mock<IServiceZClient>();
    var logger = new Mock<ILogger<ServiceZCallRequestedHandler>>();
    var handler = new ServiceZCallRequestedHandler(serviceZClient.Object, logger.Object);

    var request = new ServiceZCallRequested("saga-123", "test-data", "ServiceY-Result", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceZ",
      true,
      "ServiceZ-Final-ServiceY-Result",
      DateTime.UtcNow,
      new Dictionary<string, object> { { "duration_ms", 600 } }
    );

    serviceZClient
      .Setup(x => x.CallAsync(request.Data, request.ServiceYResult, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceZCallCompleted>();
    var completed = (ServiceZCallCompleted)result;
    completed.SagaId.Should().Be("saga-123");
    completed.Success.Should().BeTrue();
    completed.ResponseData.Should().Be("ServiceZ-Final-ServiceY-Result");
  }

  [Fact]
  public async Task Handle_Failure_ShouldReturnServiceZCallFailed()
  {
    // Arrange
    var serviceZClient = new Mock<IServiceZClient>();
    var logger = new Mock<ILogger<ServiceZCallRequestedHandler>>();
    var handler = new ServiceZCallRequestedHandler(serviceZClient.Object, logger.Object);

    var request = new ServiceZCallRequested("saga-123", "test-data", "ServiceY-Result", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceZ",
      false,
      string.Empty,
      DateTime.UtcNow,
      new Dictionary<string, object> { { "error", "Database connection failed" } }
    );

    serviceZClient
      .Setup(x => x.CallAsync(request.Data, request.ServiceYResult, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceZCallFailed>();
    var failed = (ServiceZCallFailed)result;
    failed.SagaId.Should().Be("saga-123");
    failed.ErrorMessage.Should().Be("Database connection failed");
  }

  [Fact]
  public async Task Handle_FailureWithoutErrorMetric_ShouldReturnGenericMessage()
  {
    // Arrange
    var serviceZClient = new Mock<IServiceZClient>();
    var logger = new Mock<ILogger<ServiceZCallRequestedHandler>>();
    var handler = new ServiceZCallRequestedHandler(serviceZClient.Object, logger.Object);

    var request = new ServiceZCallRequested("saga-123", "test-data", "ServiceY-Result", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceZ",
      false,
      string.Empty,
      DateTime.UtcNow,
      null
    );

    serviceZClient
      .Setup(x => x.CallAsync(request.Data, request.ServiceYResult, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceZCallFailed>();
    var failed = (ServiceZCallFailed)result;
    failed.SagaId.Should().Be("saga-123");
    failed.ErrorMessage.Should().Be("ServiceZ call failed");
  }
}
