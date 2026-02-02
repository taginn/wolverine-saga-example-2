using System.Diagnostics;
using WolverineSagaApi.Models;

namespace WolverineSagaApi.Services;

public class ServiceXClient : IServiceXClient
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILogger<ServiceXClient> _logger;

  public ServiceXClient(IHttpClientFactory httpClientFactory, ILogger<ServiceXClient> logger)
  {
    _httpClientFactory = httpClientFactory;
    _logger = logger;
  }

  public async Task<ServiceCallResult> CallAsync(string data, CancellationToken cancellationToken = default)
  {
    var stopwatch = Stopwatch.StartNew();
    try
    {
      _logger.LogInformation("ServiceX: Starting call with data: {Data}", data);

      // Simulate external service call
      await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

      var responseData = $"ServiceX-Processed-{data}";
      stopwatch.Stop();

      var metrics = new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "service", "ServiceX" },
                { "success", true }
            };

      _logger.LogInformation("ServiceX: Successfully completed in {Duration}ms", stopwatch.ElapsedMilliseconds);

      return new ServiceCallResult(
          ServiceName: "ServiceX",
          Success: true,
          ResponseData: responseData,
          Timestamp: DateTime.UtcNow,
          Metrics: metrics
      );
    }
    catch (OperationCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      stopwatch.Stop();
      _logger.LogError(ex, "ServiceX: Call failed after {Duration}ms", stopwatch.ElapsedMilliseconds);

      var metrics = new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "service", "ServiceX" },
                { "success", false },
                { "error", ex.Message }
            };

      return new ServiceCallResult(
          ServiceName: "ServiceX",
          Success: false,
          ResponseData: string.Empty,
          Timestamp: DateTime.UtcNow,
          Metrics: metrics
      );
    }
  }
}

public class ServiceYClient : IServiceYClient
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILogger<ServiceYClient> _logger;

  public ServiceYClient(IHttpClientFactory httpClientFactory, ILogger<ServiceYClient> logger)
  {
    _httpClientFactory = httpClientFactory;
    _logger = logger;
  }

  public async Task<ServiceCallResult> CallAsync(string data, string serviceXResult, CancellationToken cancellationToken = default)
  {
    var stopwatch = Stopwatch.StartNew();
    try
    {
      _logger.LogInformation("ServiceY: Starting call with data: {Data}, ServiceXResult: {ServiceXResult}",
          data, serviceXResult);

      // Simulate external service call with different timing
      await Task.Delay(TimeSpan.FromMilliseconds(450), cancellationToken);

      var responseData = $"ServiceY-Enhanced-{serviceXResult}";
      stopwatch.Stop();

      var metrics = new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "service", "ServiceY" },
                { "success", true },
                { "input_from_serviceX", serviceXResult }
            };

      _logger.LogInformation("ServiceY: Successfully completed in {Duration}ms", stopwatch.ElapsedMilliseconds);

      return new ServiceCallResult(
          ServiceName: "ServiceY",
          Success: true,
          ResponseData: responseData,
          Timestamp: DateTime.UtcNow,
          Metrics: metrics
      );
    }
    catch (OperationCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      stopwatch.Stop();
      _logger.LogError(ex, "ServiceY: Call failed after {Duration}ms", stopwatch.ElapsedMilliseconds);

      var metrics = new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "service", "ServiceY" },
                { "success", false },
                { "error", ex.Message }
            };

      return new ServiceCallResult(
          ServiceName: "ServiceY",
          Success: false,
          ResponseData: string.Empty,
          Timestamp: DateTime.UtcNow,
          Metrics: metrics
      );
    }
  }
}

public class ServiceZClient : IServiceZClient
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILogger<ServiceZClient> _logger;

  public ServiceZClient(IHttpClientFactory httpClientFactory, ILogger<ServiceZClient> logger)
  {
    _httpClientFactory = httpClientFactory;
    _logger = logger;
  }

  public async Task<ServiceCallResult> CallAsync(string data, string serviceYResult, CancellationToken cancellationToken = default)
  {
    var stopwatch = Stopwatch.StartNew();
    try
    {
      _logger.LogInformation("ServiceZ: Starting final call with data: {Data}, ServiceYResult: {ServiceYResult}",
          data, serviceYResult);

      // Simulate external service call with different timing
      await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);

      var responseData = $"ServiceZ-Final-{serviceYResult}";
      stopwatch.Stop();

      var metrics = new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "service", "ServiceZ" },
                { "success", true },
                { "input_from_serviceY", serviceYResult },
                { "final_result", true }
            };

      _logger.LogInformation("ServiceZ: Successfully completed final step in {Duration}ms", stopwatch.ElapsedMilliseconds);

      return new ServiceCallResult(
          ServiceName: "ServiceZ",
          Success: true,
          ResponseData: responseData,
          Timestamp: DateTime.UtcNow,
          Metrics: metrics
      );
    }
    catch (OperationCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      stopwatch.Stop();
      _logger.LogError(ex, "ServiceZ: Final call failed after {Duration}ms", stopwatch.ElapsedMilliseconds);

      var metrics = new Dictionary<string, object>
            {
                { "duration_ms", stopwatch.ElapsedMilliseconds },
                { "service", "ServiceZ" },
                { "success", false },
                { "error", ex.Message }
            };

      return new ServiceCallResult(
          ServiceName: "ServiceZ",
          Success: false,
          ResponseData: string.Empty,
          Timestamp: DateTime.UtcNow,
          Metrics: metrics
      );
    }
  }
}
