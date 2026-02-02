# Wolverine Three-Service Saga Tests

This project contains unit and integration tests for the Wolverine Three-Service Saga API.

## Test Structure

### Saga Tests
- `ThreeServiceSagaStartTests.cs` - Tests for saga initialization
- `ServiceXHandlerTests.cs` - Tests for ServiceX handling
- `ServiceYHandlerTests.cs` - Tests for ServiceY handling
- `ServiceZHandlerTests.cs` - Tests for ServiceZ handling
- `SagaCompletionHandlerTests.cs` - Tests for saga completion and failure handlers

### Endpoint Tests
- `SagaEndpointsTests.cs` - Integration tests for API endpoints

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~ServiceXHandlerTests"
```

## Test Coverage

The test suite covers:
- Saga state initialization
- Service call transitions (X → Y → Z)
- Success and failure scenarios
- Event handling
- API endpoint integration
