FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release

# Install dotnet debug tools
RUN dotnet tool install --tool-path /tools dotnet-trace \
 && dotnet tool install --tool-path /tools dotnet-counters \
 && dotnet tool install --tool-path /tools dotnet-dump \
 && dotnet tool install --tool-path /tools dotnet-gcdump

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
# Copy dotnet-tools
WORKDIR /tools
COPY --from=build /tools .

WORKDIR /app
COPY --from=build /app .

EXPOSE 8899

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && apt-get install iputils-ping -y

HEALTHCHECK --interval=10s --timeout=3s --start-period=3s \
  CMD curl -fs http://localhost:8899/ping || exit 1
  
ENTRYPOINT ["dotnet", "ConsoleApp1.dll"]
