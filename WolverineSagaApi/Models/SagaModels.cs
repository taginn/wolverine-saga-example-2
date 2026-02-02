namespace WolverineSagaApi.Models;

public record SagaRequest(string RequestId, string InitialData);

public record ServiceCallResult(
    string ServiceName,
    bool Success,
    string ResponseData,
    DateTime Timestamp,
    Dictionary<string, object>? Metrics = null);

public record SagaStatusResponse(
    string SagaId,
    string RequestId,
    string Status,
    string? CurrentStep,
    Dictionary<string, string>? CompletedSteps,
    DateTime StartedAt,
    DateTime? CompletedAt);
