using Wolverine;
using WolverineSagaApi.Events;
using WolverineSagaApi.Services;

namespace WolverineSagaApi.Sagas;

/// <summary>
/// Saga that orchestrates three distinct service calls (ServiceX, ServiceY, ServiceZ)
/// Each service has unique messages, logging, metrics, and return values
/// </summary>
public class ThreeServiceSaga : Saga
{
  public string Id { get; set; } = string.Empty;
  public string RequestId { get; set; } = string.Empty;
  public string InitialData { get; set; } = string.Empty;
  public DateTime StartedAt { get; set; }
  public DateTime? CompletedAt { get; set; }

  // Track results from each service
  public string? ServiceXResult { get; set; }
  public string? ServiceYResult { get; set; }
  public string? ServiceZResult { get; set; }

  // Track current step
  public string CurrentStep { get; set; } = "NotStarted";

  // Store metrics from each service
  public Dictionary<string, Dictionary<string, object>> ServiceMetrics { get; set; } = new();

  // Wolverine identity methods for message correlation
  public static string IdentityFor(ThreeServiceSagaStarted started) => started.SagaId;
  public static string IdentityFor(ServiceXCallCompleted completed) => completed.SagaId;
  public static string IdentityFor(ServiceYCallCompleted completed) => completed.SagaId;
  public static string IdentityFor(ServiceZCallCompleted completed) => completed.SagaId;
  public static string IdentityFor(ServiceXCallFailed failed) => failed.SagaId;
  public static string IdentityFor(ServiceYCallFailed failed) => failed.SagaId;
  public static string IdentityFor(ServiceZCallFailed failed) => failed.SagaId;

  /// <summary>
  /// Start the saga and initiate ServiceX call
  /// </summary>
  public static (ThreeServiceSaga, ServiceXCallRequested) Start(
      ThreeServiceSagaStarted started,
      ILogger<ThreeServiceSaga> logger)
  {
    logger.LogInformation(
        "🚀 SAGA STARTED: SagaId={SagaId}, RequestId={RequestId}, InitialData={Data}",
        started.SagaId, started.RequestId, started.InitialData);

    var state = new ThreeServiceSaga
    {
      Id = started.SagaId,
      RequestId = started.RequestId,
      InitialData = started.InitialData,
      StartedAt = started.StartedAt,
      CurrentStep = "ServiceX"
    };

    var command = new ServiceXCallRequested(started.SagaId, started.InitialData, DateTime.UtcNow);
    return (state, command);
  }

  /// <summary>
  /// Handle ServiceX completion and transition to ServiceY
  /// </summary>
  public ServiceYCallRequested Handle(ServiceXCallCompleted completed, ILogger<ThreeServiceSaga> logger)
  {
    logger.LogInformation(
        "✅ ServiceX COMPLETED: SagaId={SagaId}, Success={Success}, Response={Response}",
        completed.SagaId, completed.Success, completed.ResponseData);

    ServiceXResult = completed.ResponseData;
    CurrentStep = "ServiceY";

    return new ServiceYCallRequested(
        completed.SagaId,
        InitialData,
        completed.ResponseData,
        DateTime.UtcNow);
  }

  /// <summary>
  /// Handle ServiceX failure
  /// </summary>
  public ThreeServiceSagaFailed Handle(ServiceXCallFailed failed, ILogger<ThreeServiceSaga> logger)
  {
    logger.LogError(
        "❌ ServiceX FAILED: SagaId={SagaId}, Error={Error}",
        failed.SagaId, failed.ErrorMessage);

    CurrentStep = "Failed";
    MarkCompleted();

    return new ThreeServiceSagaFailed(failed.SagaId, "ServiceX", failed.ErrorMessage, DateTime.UtcNow);
  }

  /// <summary>
  /// Handle ServiceY completion and transition to ServiceZ
  /// </summary>
  public ServiceZCallRequested Handle(ServiceYCallCompleted completed, ILogger<ThreeServiceSaga> logger)
  {
    logger.LogInformation(
        "✅ ServiceY COMPLETED: SagaId={SagaId}, Success={Success}, Response={Response}",
        completed.SagaId, completed.Success, completed.ResponseData);

    ServiceYResult = completed.ResponseData;
    CurrentStep = "ServiceZ";

    return new ServiceZCallRequested(
        completed.SagaId,
        InitialData,
        completed.ResponseData,
        DateTime.UtcNow);
  }

  /// <summary>
  /// Handle ServiceY failure
  /// </summary>
  public ThreeServiceSagaFailed Handle(ServiceYCallFailed failed, ILogger<ThreeServiceSaga> logger)
  {
    logger.LogError(
        "❌ ServiceY FAILED: SagaId={SagaId}, Error={Error}",
        failed.SagaId, failed.ErrorMessage);

    CurrentStep = "Failed";
    MarkCompleted();

    return new ThreeServiceSagaFailed(failed.SagaId, "ServiceY", failed.ErrorMessage, DateTime.UtcNow);
  }

  /// <summary>
  /// Handle ServiceZ completion - final step
  /// </summary>
  public ThreeServiceSagaCompleted Handle(ServiceZCallCompleted completed, ILogger<ThreeServiceSaga> logger)
  {
    logger.LogInformation(
        "✅ ServiceZ COMPLETED: SagaId={SagaId}, Success={Success}, Response={Response}",
        completed.SagaId, completed.Success, completed.ResponseData);

    ServiceZResult = completed.ResponseData;
    CurrentStep = "Completed";
    CompletedAt = DateTime.UtcNow;

    MarkCompleted();

    logger.LogInformation(
        "🎉 SAGA COMPLETED SUCCESSFULLY: SagaId={SagaId}, Duration={Duration}ms",
        completed.SagaId, (CompletedAt.Value - StartedAt).TotalMilliseconds);

    return new ThreeServiceSagaCompleted(
        completed.SagaId,
        ServiceXResult ?? "",
        ServiceYResult ?? "",
        completed.ResponseData,
        DateTime.UtcNow);
  }

  /// <summary>
  /// Handle ServiceZ failure
  /// </summary>
  public ThreeServiceSagaFailed Handle(ServiceZCallFailed failed, ILogger<ThreeServiceSaga> logger)
  {
    logger.LogError(
        "❌ ServiceZ FAILED: SagaId={SagaId}, Error={Error}",
        failed.SagaId, failed.ErrorMessage);

    CurrentStep = "Failed";
    MarkCompleted();

    return new ThreeServiceSagaFailed(failed.SagaId, "ServiceZ", failed.ErrorMessage, DateTime.UtcNow);
  }
}

/// <summary>
/// Handler for ServiceX call requests
/// </summary>
public class ServiceXCallRequestedHandler
{
  private readonly IServiceXClient _serviceXClient;
  private readonly ILogger<ServiceXCallRequestedHandler> _logger;

  public ServiceXCallRequestedHandler(IServiceXClient serviceXClient, ILogger<ServiceXCallRequestedHandler> logger)
  {
    _serviceXClient = serviceXClient;
    _logger = logger;
  }

  public async Task<object> Handle(ServiceXCallRequested request, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "📞 Calling ServiceX: SagaId={SagaId}, Data={Data}",
        request.SagaId, request.Data);

    var result = await _serviceXClient.CallAsync(request.Data, cancellationToken);

    if (result.Metrics != null)
    {
      foreach (var metric in result.Metrics)
      {
        _logger.LogInformation(
            "📊 ServiceX Metric: {Key}={Value}",
            metric.Key, metric.Value);
      }
    }

    if (result.Success)
    {
      return new ServiceXCallCompleted(request.SagaId, true, result.ResponseData, DateTime.UtcNow);
    }
    else
    {
      return new ServiceXCallFailed(request.SagaId, "ServiceX call failed", DateTime.UtcNow);
    }
  }
}

/// <summary>
/// Handler for ServiceY call requests
/// </summary>
public class ServiceYCallRequestedHandler
{
  private readonly IServiceYClient _serviceYClient;
  private readonly ILogger<ServiceYCallRequestedHandler> _logger;

  public ServiceYCallRequestedHandler(IServiceYClient serviceYClient, ILogger<ServiceYCallRequestedHandler> logger)
  {
    _serviceYClient = serviceYClient;
    _logger = logger;
  }

  public async Task<object> Handle(ServiceYCallRequested request, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "📞 Calling ServiceY: SagaId={SagaId}, Data={Data}, ServiceXResult={ServiceXResult}",
        request.SagaId, request.Data, request.ServiceXResult);

    var result = await _serviceYClient.CallAsync(request.Data, request.ServiceXResult, cancellationToken);

    if (result.Metrics != null)
    {
      foreach (var metric in result.Metrics)
      {
        _logger.LogInformation(
            "📊 ServiceY Metric: {Key}={Value}",
            metric.Key, metric.Value);
      }
    }

    if (result.Success)
    {
      return new ServiceYCallCompleted(request.SagaId, true, result.ResponseData, DateTime.UtcNow);
    }
    else
    {
      return new ServiceYCallFailed(request.SagaId, "ServiceY call failed", DateTime.UtcNow);
    }
  }
}

/// <summary>
/// Handler for ServiceZ call requests
/// </summary>
public class ServiceZCallRequestedHandler
{
  private readonly IServiceZClient _serviceZClient;
  private readonly ILogger<ServiceZCallRequestedHandler> _logger;

  public ServiceZCallRequestedHandler(IServiceZClient serviceZClient, ILogger<ServiceZCallRequestedHandler> logger)
  {
    _serviceZClient = serviceZClient;
    _logger = logger;
  }

  public async Task<object> Handle(ServiceZCallRequested request, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "📞 Calling ServiceZ: SagaId={SagaId}, Data={Data}, ServiceYResult={ServiceYResult}",
        request.SagaId, request.Data, request.ServiceYResult);

    var result = await _serviceZClient.CallAsync(request.Data, request.ServiceYResult, cancellationToken);

    if (result.Metrics != null)
    {
      foreach (var metric in result.Metrics)
      {
        _logger.LogInformation(
            "📊 ServiceZ Metric: {Key}={Value}",
            metric.Key, metric.Value);
      }
    }

    if (result.Success)
    {
      return new ServiceZCallCompleted(request.SagaId, true, result.ResponseData, DateTime.UtcNow);
    }
    else
    {
      return new ServiceZCallFailed(request.SagaId, "ServiceZ call failed", DateTime.UtcNow);
    }
  }
}

/// <summary>
/// Handler for saga completion events
/// </summary>
public class ThreeServiceSagaCompletedHandler
{
  private readonly ILogger<ThreeServiceSagaCompletedHandler> _logger;

  public ThreeServiceSagaCompletedHandler(ILogger<ThreeServiceSagaCompletedHandler> logger)
  {
    _logger = logger;
  }

  public void Handle(ThreeServiceSagaCompleted completed)
  {
    _logger.LogInformation(
        "🎊 FINAL SAGA COMPLETION: SagaId={SagaId}\n" +
        "  ServiceX Result: {ServiceXResult}\n" +
        "  ServiceY Result: {ServiceYResult}\n" +
        "  ServiceZ Result: {ServiceZResult}",
        completed.SagaId,
        completed.ServiceXResult,
        completed.ServiceYResult,
        completed.ServiceZResult);
  }
}

/// <summary>
/// Handler for saga failure events
/// </summary>
public class ThreeServiceSagaFailedHandler
{
  private readonly ILogger<ThreeServiceSagaFailedHandler> _logger;

  public ThreeServiceSagaFailedHandler(ILogger<ThreeServiceSagaFailedHandler> logger)
  {
    _logger = logger;
  }

  public void Handle(ThreeServiceSagaFailed failed)
  {
    _logger.LogError(
        "💥 SAGA FAILURE: SagaId={SagaId}, FailedService={FailedService}, Error={ErrorMessage}",
        failed.SagaId,
        failed.FailedService,
        failed.ErrorMessage);
  }
}
