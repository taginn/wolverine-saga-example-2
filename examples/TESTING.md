# Testing the Wolverine Three-Service Saga API

This guide provides comprehensive examples for testing the Wolverine Three-Service Saga API.

## Prerequisites

- The application should be running on `http://localhost:5000` (or your configured port)
- `curl` or any HTTP client (Postman, Insomnia, VS Code REST Client, etc.)

## Basic Tests

### 1. Health Check

Verify the application is running:

```bash
curl http://localhost:5000/health
```

Expected response:
```
Healthy
```

### 2. API Health Check

```bash
curl http://localhost:5000/api/health
```

Expected response:
```json
{
  "status": "Healthy",
  "service": "Wolverine Three-Service Saga API",
  "timestamp": "2026-02-02T12:00:00Z"
}
```

### 3. Start a Three-Service Saga

Initiate a new saga that will call ServiceX → ServiceY → ServiceZ in sequence:

```bash
curl -X POST http://localhost:5000/api/saga/start \
  -H "Content-Type: application/json" \
  -d '{
    "requestId": "req-001",
    "initialData": "Sample data for processing"
  }'
```

Expected response (202 Accepted):
```json
{
  "sagaId": "abc-123-def-456",
  "requestId": "req-001",
  "status": "Started",
  "message": "Three-service saga has been initiated and is processing through ServiceX → ServiceY → ServiceZ"
}
```

### 4. Check Saga Status

Query the status of a running saga (replace `{sagaId}` with actual ID from step 3):

```bash
curl http://localhost:5000/api/saga/status/abc-123-def-456
```

Expected response:
```json
{
  "sagaId": "abc-123-def-456",
  "status": "Processing",
  "currentStep": "In Progress",
  "message": "Saga status endpoint - In production, this would query the actual saga state from Wolverine's persistence",
  "note": "Check application logs to see detailed saga execution flow"
}
```

## Advanced Testing Scenarios

### Multiple Concurrent Sagas

Test the system with multiple concurrent saga executions:

```bash
# Start multiple sagas
for i in {1..5}; do
  curl -X POST http://localhost:5000/api/saga/start \
    -H "Content-Type: application/json" \
    -d "{\"requestId\": \"req-$i\", \"initialData\": \"test-data-$i\"}" &
done
wait
```

### Load Testing with Different Data

```bash
# Test with various data payloads
curl -X POST http://localhost:5000/api/saga/start \
  -H "Content-Type: application/json" \
  -d '{
    "requestId": "req-large-payload",
    "initialData": "Large data payload for testing: Lorem ipsum dolor sit amet, consectetur adipiscing elit..."
  }'
```

## Using the HTTP Test File

The project includes a `test-saga.http` file for VS Code REST Client extension:

1. Open `WolverineSagaApi/test-saga.http` in VS Code
2. Install the "REST Client" extension if not already installed
3. Click "Send Request" above each request

## Observing the Saga Execution

When you start a saga, watch the console output to observe the complete flow:

### Successful Execution

You should see logs similar to:

```
🔵 API: Starting new saga - SagaId=..., RequestId=req-001, Data=Sample data
🚀 SAGA STARTED: SagaId=..., RequestId=req-001
📞 Calling ServiceX: SagaId=..., Data=Sample data
   ServiceX: Starting call with data: Sample data
   ServiceX: Successfully completed in 302ms
   📊 ServiceX Metric: duration_ms=302
   📊 ServiceX Metric: service=ServiceX
   📊 ServiceX Metric: success=True
✅ ServiceX COMPLETED: Response=ServiceX-Processed-Sample data
📞 Calling ServiceY: ServiceXResult=ServiceX-Processed-Sample data
   ServiceY: Starting call, ServiceXResult: ServiceX-Processed-Sample data
   ServiceY: Successfully completed in 450ms
   📊 ServiceY Metric: duration_ms=450
   📊 ServiceY Metric: input_from_serviceX=ServiceX-Processed-Sample data
✅ ServiceY COMPLETED: Response=ServiceY-Enhanced-ServiceX-Processed-Sample data
📞 Calling ServiceZ: ServiceYResult=ServiceY-Enhanced-...
   ServiceZ: Starting final call
   ServiceZ: Successfully completed final step in 601ms
   📊 ServiceZ Metric: duration_ms=601
   📊 ServiceZ Metric: final_result=True
✅ ServiceZ COMPLETED: Response=ServiceZ-Final-...
🎉 SAGA COMPLETED SUCCESSFULLY: Duration=1367ms
🎊 FINAL SAGA COMPLETION
   ServiceX Result: ServiceX-Processed-Sample data
   ServiceY Result: ServiceY-Enhanced-ServiceX-Processed-Sample data
   ServiceZ Result: ServiceZ-Final-ServiceY-Enhanced-ServiceX-Processed-Sample data
```

### Service-Specific Details

#### ServiceX
- **Purpose**: First service in the chain
- **Input**: Initial data from the request
- **Processing**: Prefixes data with "ServiceX-Processed-"
- **Duration**: ~300ms
- **Metrics**: duration_ms, service, success
- **Output**: Passed to ServiceY

#### ServiceY
- **Purpose**: Second service in the chain
- **Input**: Original data + ServiceX result
- **Processing**: Prefixes ServiceX result with "ServiceY-Enhanced-"
- **Duration**: ~450ms
- **Metrics**: duration_ms, service, success, input_from_serviceX
- **Output**: Passed to ServiceZ

#### ServiceZ
- **Purpose**: Final service in the chain
- **Input**: Original data + ServiceY result
- **Processing**: Prefixes ServiceY result with "ServiceZ-Final-"
- **Duration**: ~600ms
- **Metrics**: duration_ms, service, success, input_from_serviceY, final_result
- **Output**: Final saga result

## Testing with PowerShell (Windows)

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5000/health"

# Start saga
$body = @{
    requestId = "req-001"
    initialData = "test data"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/saga/start" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

## Running Unit Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ServiceXHandlerTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Expected Test Results

The test suite includes:

✅ **Saga Initialization Tests**
- Verifies proper saga state initialization
- Confirms ServiceX is called first

✅ **ServiceX Handler Tests**
- Success: Transitions to ServiceY
- Failure: Marks saga as failed

✅ **ServiceY Handler Tests**
- Success: Transitions to ServiceZ with ServiceX result
- Failure: Marks saga as failed

✅ **ServiceZ Handler Tests**
- Success: Completes saga with all results
- Failure: Marks saga as failed

✅ **Completion Handler Tests**
- Verifies logging on success
- Verifies error logging on failure

✅ **Endpoint Integration Tests**
- POST /api/saga/start returns 202 Accepted
- GET /api/saga/status returns 200 OK
- GET /health returns Healthy

## Performance Testing

Monitor saga execution times:

```bash
# Time a saga execution
time curl -X POST http://localhost:5000/api/saga/start \
  -H "Content-Type: application/json" \
  -d '{"requestId": "perf-test", "initialData": "performance"}'
```

Expected total time: ~1.3-1.5 seconds (300ms + 450ms + 600ms + overhead)

## Troubleshooting

### Saga Not Starting
- Check application logs for errors
- Verify the request body is valid JSON
- Ensure the application is running on the correct port

### Services Not Being Called
- Check Wolverine message bus configuration
- Verify service clients are registered in DI container
- Review logs for handler exceptions

### Tests Failing
- Ensure .NET 10 SDK is installed
- Run `dotnet restore` to restore packages
- Check that test project references main project correctly

## Production Testing Checklist

Before deploying to production:

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Health check endpoints respond correctly
- [ ] Saga can handle concurrent requests
- [ ] Error scenarios are handled gracefully
- [ ] Metrics are being collected
- [ ] Logs are structured and searchable
- [ ] Database persistence is configured
- [ ] External services are properly configured
- [ ] Performance meets requirements
