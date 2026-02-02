using WolverineSagaApi.Events;
using WolverineSagaApi.Sagas;

namespace WolverineSagaApi.Tests.Sagas;

public class ThreeServiceSagaStartTests
{
  [Fact]
  public void Start_ShouldInitializeSagaStateCorrectly()
  {
    // Arrange
    var sagaId = Guid.NewGuid().ToString();
    var requestId = "req-123";
    var initialData = "test-data";
    var startedAt = DateTime.UtcNow;
    var started = new ThreeServiceSagaStarted(sagaId, requestId, initialData, startedAt);
    var logger = new Mock<ILogger<ThreeServiceSaga>>();

    // Act
    var (state, command) = ThreeServiceSaga.Start(started, logger.Object);

    // Assert
    state.Should().NotBeNull();
    state.Id.Should().Be(sagaId);
    state.RequestId.Should().Be(requestId);
    state.InitialData.Should().Be(initialData);
    state.StartedAt.Should().Be(startedAt);
    state.CurrentStep.Should().Be("ServiceX");

    command.Should().NotBeNull();
    command.SagaId.Should().Be(sagaId);
    command.Data.Should().Be(initialData);
  }
}
