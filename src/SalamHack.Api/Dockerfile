# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["SalamHack.sln", "."]
COPY ["src/SalamHack.Api/SalamHack.Api.csproj", "src/SalamHack.Api/"]
COPY ["src/SalamHack.Application/SalamHack.Application.csproj", "src/SalamHack.Application/"]
COPY ["src/SalamHack.Infrastructure/SalamHack.Infrastructure.csproj", "src/SalamHack.Infrastructure/"]
COPY ["src/SalamHack.Domain/SalamHack.Domain.csproj", "src/SalamHack.Domain/"]
COPY ["src/SalamHack.Contracts/SalamHack.Contracts.csproj", "src/SalamHack.Contracts/"]

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "src/SalamHack.Api/SalamHack.Api.csproj"

COPY . .
WORKDIR /src/src/SalamHack.Api
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "SalamHack.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --ingroup appgroup --no-create-home appuser \
    && mkdir -p /app/.aspnet/DataProtection-Keys \
    && chown -R appuser:appgroup /app/.aspnet

COPY --from=build /app/publish .
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SalamHack.Api.dll"]
