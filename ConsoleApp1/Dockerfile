FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build

COPY ["ConsoleApp1/ConsoleApp1.csproj", "ConsoleApp1/"]
RUN dotnet restore "ConsoleApp1/ConsoleApp1.csproj"

COPY . .
WORKDIR "/build/ConsoleApp1"
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

EXPOSE 8899

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && apt-get install iputils-ping -y

HEALTHCHECK --interval=10s --timeout=3s --start-period=3s \
  CMD curl -fs http://localhost:8899/health/live || exit 1
  
ENTRYPOINT ["dotnet", "ConsoleApp1.dll"]
