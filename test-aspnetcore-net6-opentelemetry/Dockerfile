FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /build
ARG BUILD_CONFIGURATION=Release

COPY ["test-aspnetcore-net6-opentelemetry/test-aspnetcore-net6-opentelemetry.csproj", "test-aspnetcore-net6-opentelemetry/"]
RUN dotnet restore "test-aspnetcore-net6-opentelemetry/test-aspnetcore-net6-opentelemetry.csproj"

COPY . .
WORKDIR "/build/test-aspnetcore-net6-opentelemetry"
RUN dotnet publish \
  --configuration Release \
  --no-self-contained \
  --framework net6.0 \
  --use-current-runtime \
  /p:PreserveCompilationContext=true \
  --output /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 8888

RUN apt-get update && apt-get install -y --no-install-recommends curl

HEALTHCHECK --interval=10s --timeout=3s --start-period=3s \
  CMD curl -fs http://localhost:8888/ping || exit 1
  
ENTRYPOINT ["dotnet", "test-aspnetcore-net6-opentelemetry.dll"]
