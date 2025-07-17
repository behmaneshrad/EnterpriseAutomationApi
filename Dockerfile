# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore NuGet packages
RUN dotnet restore ./EnterpriseAutomation.sln

# Build and publish the WebAPI project
RUN dotnet publish ./EnterpriseAutomation.Api/EnterpriseAutomation.Api.csproj -c Release -o /app/publish

# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port (if you're using port 80 inside the container)
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "EnterpriseAutomation.Api.dll"]
