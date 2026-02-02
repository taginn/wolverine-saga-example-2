using WolverineSagaApi.Events;
using WolverineSagaApi.Models;
using WolverineSagaApi.Sagas;
using WolverineSagaApi.Services;

namespace WolverineSagaApi.Tests.Sagas;

public class ServiceXCallRequestedHandlerTests
{
  [Fact]
  public async Task Handle_Success_ShouldReturnServiceXCallCompleted()
  {
    // Arrange
    var serviceXClient = new Mock<IServiceXClient>();
    var logger = new Mock<ILogger<ServiceXCallRequestedHandler>>();
    var handler = new ServiceXCallRequestedHandler(serviceXClient.Object, logger.Object);

    var request = new ServiceXCallRequested("saga-123", "test-data", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceX",
      true,
      "ServiceX-Processed-test-data",
      DateTime.UtcNow,
      new Dictionary<string, object> { { "duration_ms", 300 } }
    );

    serviceXClient
      .Setup(x => x.CallAsync(request.Data, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceXCallCompleted>();
    var completed = (ServiceXCallCompleted)result;
    completed.SagaId.Should().Be("saga-123");
    completed.Success.Should().BeTrue();
    completed.ResponseData.Should().Be("ServiceX-Processed-test-data");
  }

  [Fact]
  public async Task Handle_Failure_ShouldReturnServiceXCallFailed()
  {
    // Arrange
    var serviceXClient = new Mock<IServiceXClient>();
    var logger = new Mock<ILogger<ServiceXCallRequestedHandler>>();
    var handler = new ServiceXCallRequestedHandler(serviceXClient.Object, logger.Object);

    var request = new ServiceXCallRequested("saga-123", "test-data", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceX",
      false,
      string.Empty,
      DateTime.UtcNow,
      new Dictionary<string, object> { { "error", "Connection timeout" } }
    );

    serviceXClient
      .Setup(x => x.CallAsync(request.Data, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceXCallFailed>();
    var failed = (ServiceXCallFailed)result;
    failed.SagaId.Should().Be("saga-123");
    failed.ErrorMessage.Should().Be("Connection timeout");
  }

  [Fact]
  public async Task Handle_FailureWithoutErrorMetric_ShouldReturnGenericMessage()
  {
    // Arrange
    var serviceXClient = new Mock<IServiceXClient>();
    var logger = new Mock<ILogger<ServiceXCallRequestedHandler>>();
    var handler = new ServiceXCallRequestedHandler(serviceXClient.Object, logger.Object);

    var request = new ServiceXCallRequested("saga-123", "test-data", DateTime.UtcNow);
    var expectedResult = new ServiceCallResult(
      "ServiceX",
      false,
      string.Empty,
      DateTime.UtcNow,
      null
    );

    serviceXClient
      .Setup(x => x.CallAsync(request.Data, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ServiceXCallFailed>();
    var failed = (ServiceXCallFailed)result;
    failed.SagaId.Should().Be("saga-123");
    failed.ErrorMessage.Should().Be("ServiceX call failed");
  }
}
