# Wolverine Three-Service Saga Example

This project demonstrates a Wolverine saga that orchestrates three distinct service calls (ServiceX, ServiceY, ServiceZ), each with unique messages, logging, metrics, and return values.

## Architecture

### Saga Flow
```
Client Request
    ↓
ThreeServiceSagaStarted
    ↓
ServiceX Call (unique metrics & logging)
    ↓
ServiceXCallCompleted
    ↓
ServiceY Call (receives ServiceX result, unique metrics & logging)
    ↓
ServiceYCallCompleted
    ↓
ServiceZ Call (receives ServiceY result, unique metrics & logging)
    ↓
ServiceZCallCompleted
    ↓
ThreeServiceSagaCompleted
```

### Key Features

1. **Three Distinct Endpoints**: ServiceX, ServiceY, and ServiceZ
2. **Unique Messages**: Each service has its own request/completion/failure events
3. **Unique Logging**: Distinct log messages with emojis for easy tracking
4. **Unique Metrics**: Each service tracks duration, success, and service-specific metadata
5. **Unique Return Values**: Results from previous services are passed to subsequent ones
6. **Sequential Processing**: ServiceY receives ServiceX's result, ServiceZ receives ServiceY's result

## Project Structure

```
WolverineSagaApi/
├── Events/
│   └── SagaEvents.cs          # Event definitions for all three services
├── Models/
│   └── SagaModels.cs          # Request/response models
├── Sagas/
│   └── ThreeServiceSaga.cs    # Main saga orchestration
├── Services/
│   ├── IServiceClients.cs     # Service interfaces
│   └── ServiceClients.cs      # Service implementations
├── Endpoints/
│   └── SagaEndpoints.cs       # API endpoints
└── Program.cs                 # Application configuration
```

## API Endpoints

### Start a Saga
```bash
POST /api/saga/start
Content-Type: application/json

{
  "requestId": "req-123",
  "initialData": "test-data"
}
```

### Check Saga Status
```bash
GET /api/saga/status/{sagaId}
```

### Health Check
```bash
GET /api/health
```

## Running the Application

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project WolverineSagaApi

# Test the saga
curl -X POST http://localhost:5000/api/saga/start \
  -H "Content-Type: application/json" \
  -d '{"requestId": "req-123", "initialData": "test-data"}'
```

## Observing the Saga

Watch the console output to see:
- 🚀 Saga start
- 📞 Service calls (X, Y, Z)
- 📊 Unique metrics for each service
- ✅ Successful completions
- ❌ Failures (if any)
- 🎉 Final saga completion

## Service-Specific Details

### ServiceX
- First service in the chain
- Processes initial data
- Duration: ~300ms
- Returns: `ServiceX-Processed-{data}`

### ServiceY
- Second service in the chain
- Receives ServiceX result
- Duration: ~450ms
- Returns: `ServiceY-Enhanced-{serviceXResult}`

### ServiceZ
- Final service in the chain
- Receives ServiceY result
- Duration: ~600ms
- Returns: `ServiceZ-Final-{serviceYResult}`

## Differences from wolverine-saga-example

1. **Three Services**: Instead of a generic multi-service flow, this explicitly orchestrates three distinct services
2. **Unique Events**: Each service has its own request, completion, and failure events
3. **Unique Handlers**: Separate handlers for ServiceX, ServiceY, and ServiceZ requests
4. **Unique Logging**: Distinct log messages with service-specific context
5. **Unique Metrics**: Each service tracks different metrics
6. **Data Flow**: Results cascade from X → Y → Z

## Technologies

- .NET 10.0
- WolverineFx 5.13.0
- ASP.NET Core Minimal APIs
- In-memory message transport (for development)