namespace WolverineSagaApi.Events;

// Events for ServiceX
public record ServiceXCallRequested(string SagaId, string Data, DateTime RequestedAt);
public record ServiceXCallCompleted(string SagaId, bool Success, string ResponseData, DateTime CompletedAt);
public record ServiceXCallFailed(string SagaId, string ErrorMessage, DateTime FailedAt);

// Events for ServiceY
public record ServiceYCallRequested(string SagaId, string Data, string ServiceXResult, DateTime RequestedAt);
public record ServiceYCallCompleted(string SagaId, bool Success, string ResponseData, DateTime CompletedAt);
public record ServiceYCallFailed(string SagaId, string ErrorMessage, DateTime FailedAt);

// Events for ServiceZ
public record ServiceZCallRequested(string SagaId, string Data, string ServiceYResult, DateTime RequestedAt);
public record ServiceZCallCompleted(string SagaId, bool Success, string ResponseData, DateTime CompletedAt);
public record ServiceZCallFailed(string SagaId, string ErrorMessage, DateTime FailedAt);

// Saga lifecycle events
public record ThreeServiceSagaStarted(string SagaId, string RequestId, string InitialData, DateTime StartedAt);
public record ThreeServiceSagaCompleted(
    string SagaId,
    string ServiceXResult,
    string ServiceYResult,
    string ServiceZResult,
    DateTime CompletedAt);
public record ThreeServiceSagaFailed(string SagaId, string FailedService, string ErrorMessage, DateTime FailedAt);
