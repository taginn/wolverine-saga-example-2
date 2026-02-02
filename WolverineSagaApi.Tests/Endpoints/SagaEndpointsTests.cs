using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using WolverineSagaApi.Models;

namespace WolverineSagaApi.Tests.Endpoints;

public class SagaEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _factory;

  public SagaEndpointsTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task StartSaga_ShouldReturnAccepted()
  {
    // Arrange
    var client = _factory.CreateClient();
    var request = new SagaRequest("req-123", "test-data");

    // Act
    var response = await client.PostAsJsonAsync("/api/saga/start", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("saga");
    content.Should().Contain("Started");
  }

  [Fact]
  public async Task GetSagaStatus_ShouldReturnOk()
  {
    // Arrange
    var client = _factory.CreateClient();
    var sagaId = Guid.NewGuid().ToString();

    // Act
    var response = await client.GetAsync($"/api/saga/status/{sagaId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task HealthCheck_ShouldReturnHealthy()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("Healthy");
  }
}
