using WolverineSagaApi.Models;

namespace WolverineSagaApi.Services;

public interface IServiceXClient
{
  Task<ServiceCallResult> CallAsync(string data, CancellationToken cancellationToken = default);
}

public interface IServiceYClient
{
  Task<ServiceCallResult> CallAsync(string data, string serviceXResult, CancellationToken cancellationToken = default);
}

public interface IServiceZClient
{
  Task<ServiceCallResult> CallAsync(string data, string serviceYResult, CancellationToken cancellationToken = default);
}
