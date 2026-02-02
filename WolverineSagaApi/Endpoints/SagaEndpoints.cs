using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineSagaApi.Events;
using WolverineSagaApi.Models;

namespace WolverineSagaApi.Endpoints;

public static class SagaEndpoints
{
  public static void MapSagaEndpoints(this WebApplication app)
  {
    app.MapPost("/api/saga/start", async (
        [FromBody] SagaRequest request,
        IMessageBus messageBus,
        ILogger<Program> logger) =>
    {
      var sagaId = Guid.NewGuid().ToString();

      logger.LogInformation(
              "🔵 API: Starting new saga - SagaId={SagaId}, RequestId={RequestId}, Data={Data}",
              sagaId, request.RequestId, request.InitialData);

      var sagaStarted = new ThreeServiceSagaStarted(
              SagaId: sagaId,
              RequestId: request.RequestId,
              InitialData: request.InitialData,
              StartedAt: DateTime.UtcNow
          );

      // Publish the saga started event to Wolverine message bus
      await messageBus.PublishAsync(sagaStarted);

      logger.LogInformation(
              "✅ API: Saga initiated successfully - SagaId={SagaId}",
              sagaId);

      return Results.Accepted($"/api/saga/status/{sagaId}", new
      {
        SagaId = sagaId,
        RequestId = request.RequestId,
        Status = "Started",
        Message = "Three-service saga has been initiated and is processing through ServiceX → ServiceY → ServiceZ"
      });
    })
    .WithName("StartThreeServiceSaga")
    .WithOpenApi()
    .Produces(StatusCodes.Status202Accepted)
    .Produces(StatusCodes.Status400BadRequest);

    app.MapGet("/api/saga/status/{sagaId}", (
        string sagaId,
        ILogger<Program> logger) =>
    {
      logger.LogInformation("🔍 API: Checking status for SagaId={SagaId}", sagaId);

      // In a real implementation with persistence, you would query the saga state from storage
      // For this example, we're returning a mock status
      return Results.Ok(new
      {
        SagaId = sagaId,
        Status = "Processing",
        CurrentStep = "In Progress",
        Message = "Saga status endpoint - In production, this would query the actual saga state from Wolverine's persistence",
        Note = "Check application logs to see detailed saga execution flow"
      });
    })
    .WithName("GetSagaStatus")
    .WithOpenApi()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

    app.MapGet("/api/health", () =>
    {
      return Results.Ok(new
      {
        Status = "Healthy",
        Service = "Wolverine Three-Service Saga API",
        Timestamp = DateTime.UtcNow
      });
    })
    .WithName("HealthCheck")
    .WithOpenApi()
    .Produces(StatusCodes.Status200OK);
  }
}
