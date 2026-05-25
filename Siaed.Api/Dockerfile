FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Siaed.Api/SiaedBackend.Api.csproj", "Siaed.Api/"]
COPY ["Siaed.Application/Siaed.Application.csproj", "Siaed.Application/"]
COPY ["Siaed.Domain/Siaed.Domain.csproj", "Siaed.Domain/"]
COPY ["Siaed.Infra/Siaed.Infra.csproj", "Siaed.Infra/"]
RUN dotnet restore "Siaed.Api/SiaedBackend.Api.csproj"

COPY . .
WORKDIR /src/Siaed.Api
RUN dotnet publish "SiaedBackend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

RUN mkdir -p /app/logs

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SiaedBackend.Api.dll"]