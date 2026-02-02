using Wolverine;
using WolverineSagaApi.Endpoints;
using WolverineSagaApi.Sagas;
using WolverineSagaApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Configure HTTP clients for the three external services
builder.Services.AddHttpClient("ServiceX", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ExternalServices:ServiceX"] ?? "https://api.example.com/serviceX");
  client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("ServiceY", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ExternalServices:ServiceY"] ?? "https://api.example.com/serviceY");
  client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("ServiceZ", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ExternalServices:ServiceZ"] ?? "https://api.example.com/serviceZ");
  client.Timeout = TimeSpan.FromSeconds(30);
});

// Register service clients
builder.Services.AddScoped<IServiceXClient, ServiceXClient>();
builder.Services.AddScoped<IServiceYClient, ServiceYClient>();
builder.Services.AddScoped<IServiceZClient, ServiceZClient>();

// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
  // Use local (in-memory) queuing for development
  // In production, you would use a durable transport like RabbitMQ, Azure Service Bus, etc.
  opts.LocalQueue("default")
      .Sequential();

  // Auto-discover handlers and sagas in the application assembly
  opts.Discovery.IncludeAssembly(typeof(Program).Assembly);

  // Configure policies
  opts.Policies.AutoApplyTransactions();

  // Optional: Configure saga persistence
  // For production, you would add EF Core or other persistence
  // opts.PersistMessagesWithEntityFramework<SagaDbContext>();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map saga endpoints
app.MapSagaEndpoints();

app.Logger.LogInformation("🚀 Wolverine Three-Service Saga API is starting...");
app.Logger.LogInformation("📝 Available endpoints:");
app.Logger.LogInformation("   POST /api/saga/start - Start a new saga");
app.Logger.LogInformation("   GET  /api/saga/status/{{sagaId}} - Get saga status");
app.Logger.LogInformation("   GET  /api/health - Health check");

app.Run();

// Make the Program class accessible to tests
public partial class Program { }
