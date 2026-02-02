# Azure Deployment Guide

This guide explains how to deploy the Wolverine Three-Service Saga API to Azure Container Apps.

## Prerequisites

- Azure CLI installed
- Docker installed (for local testing)
- Azure subscription

## Deployment Options

### Option 1: Deploy using Azure Container Apps (Recommended)

1. **Login to Azure**
   ```bash
   az login
   ```

2. **Create a Resource Group**
   ```bash
   az group create --name wolverine-three-saga-rg --location eastus
   ```

3. **Build and Push Docker Image to Azure Container Registry**
   ```bash
   # Create Azure Container Registry
   az acr create --resource-group wolverine-three-saga-rg --name wolverinethreesagaregistry --sku Basic
   
   # Login to ACR
   az acr login --name wolverinethreesagaregistry
   
   # Build and push the image
   docker build -t wolverinethreesagaregistry.azurecr.io/wolverine-three-saga-api:latest .
   docker push wolverinethreesagaregistry.azurecr.io/wolverine-three-saga-api:latest
   ```

4. **Deploy using ARM Template**
   ```bash
   az deployment group create \
     --resource-group wolverine-three-saga-rg \
     --template-file azure/container-app-template.json \
     --parameters containerImage=wolverinethreesagaregistry.azurecr.io/wolverine-three-saga-api:latest
   ```

### Option 2: Deploy using Azure App Service

1. **Create App Service Plan**
   ```bash
   az appservice plan create --name wolverine-three-saga-plan --resource-group wolverine-three-saga-rg --is-linux --sku B1
   ```

2. **Create Web App**
   ```bash
   az webapp create --resource-group wolverine-three-saga-rg --plan wolverine-three-saga-plan --name wolverine-three-saga-api --deployment-container-image-name wolverinethreesagaregistry.azurecr.io/wolverine-three-saga-api:latest
   ```

3. **Configure App Settings**
   ```bash
   az webapp config appsettings set --resource-group wolverine-three-saga-rg --name wolverine-three-saga-api --settings \
     ASPNETCORE_ENVIRONMENT=Production \
     ExternalServices__ServiceX=https://api.example.com/serviceX \
     ExternalServices__ServiceY=https://api.example.com/serviceY \
     ExternalServices__ServiceZ=https://api.example.com/serviceZ
   ```

### Option 3: Deploy using Azure Kubernetes Service (AKS)

For production deployments requiring high scalability and advanced orchestration, consider deploying to AKS.

## Environment Variables

Configure the following environment variables for production:

- `ASPNETCORE_ENVIRONMENT`: Set to "Production"
- `ConnectionStrings__DefaultConnection`: Database connection string for saga persistence
- `ExternalServices__ServiceX`: URL for external service X
- `ExternalServices__ServiceY`: URL for external service Y
- `ExternalServices__ServiceZ`: URL for external service Z

## Database Configuration

For production, replace the in-memory database with a persistent database:

1. **Azure SQL Database**
   ```bash
   az sql server create --name wolverine-three-saga-sql --resource-group wolverine-three-saga-rg --location eastus --admin-user sqladmin --admin-password <your-password>
   az sql db create --resource-group wolverine-three-saga-rg --server wolverine-three-saga-sql --name WolverineThreeSagaDb --service-objective S0
   ```

2. **Azure Database for PostgreSQL**
   ```bash
   az postgres server create --resource-group wolverine-three-saga-rg --name wolverine-three-saga-postgres --location eastus --admin-user pgadmin --admin-password <your-password> --sku-name B_Gen5_1
   az postgres db create --resource-group wolverine-three-saga-rg --server-name wolverine-three-saga-postgres --name WolverineThreeSagaDb
   ```

## Monitoring and Logging

The application is configured with:
- Health checks at `/health` and `/api/health`
- Structured logging to Application Insights (when configured)
- OpenAPI documentation at `/openapi/v1.json` (in Development)
- Detailed saga execution logs with emoji markers for easy tracking

## Testing the Deployment

After deployment, test the endpoints:

```bash
# Get the FQDN from deployment output
FQDN=$(az deployment group show -g wolverine-three-saga-rg -n <deployment-name> --query properties.outputs.containerAppFQDN.value -o tsv)

# Test health endpoint
curl https://$FQDN/health

# Test API health endpoint
curl https://$FQDN/api/health

# Start a saga
curl -X POST https://$FQDN/api/saga/start \
  -H "Content-Type: application/json" \
  -d '{"requestId": "req-001", "initialData": "test-data"}'
```

## Scaling Configuration

The default template configures:
- Minimum replicas: 1
- Maximum replicas: 3
- Auto-scaling based on HTTP traffic

To adjust scaling:

```bash
az containerapp update \
  --name wolverine-three-saga-api \
  --resource-group wolverine-three-saga-rg \
  --min-replicas 2 \
  --max-replicas 5
```

## Message Transport for Production

For production workloads, replace the in-memory local queue with a durable transport:

### Azure Service Bus
```bash
# Create Service Bus namespace
az servicebus namespace create \
  --name wolverine-three-saga-sb \
  --resource-group wolverine-three-saga-rg \
  --location eastus \
  --sku Standard

# Get connection string
az servicebus namespace authorization-rule keys list \
  --resource-group wolverine-three-saga-rg \
  --namespace-name wolverine-three-saga-sb \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString -o tsv
```

Update your Program.cs to use Azure Service Bus instead of local queuing.

## Clean Up Resources

```bash
az group delete --name wolverine-three-saga-rg --yes --no-wait
```
