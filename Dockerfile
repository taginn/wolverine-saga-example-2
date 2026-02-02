# Use the official .NET 10 SDK image as build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["WolverineSagaApi/WolverineSagaApi.csproj", "WolverineSagaApi/"]
RUN dotnet restore "WolverineSagaApi/WolverineSagaApi.csproj"

# Copy all source files and build the project
COPY WolverineSagaApi/ WolverineSagaApi/
WORKDIR "/src/WolverineSagaApi"
RUN dotnet build "WolverineSagaApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "WolverineSagaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 10 runtime image for final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WolverineSagaApi.dll"]
