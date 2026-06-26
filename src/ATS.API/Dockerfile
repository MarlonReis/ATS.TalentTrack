FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ATS.API/ATS.API.csproj", "src/ATS.API/"]
COPY ["src/ATS.Application/ATS.Application.csproj", "src/ATS.Application/"]
COPY ["src/ATS.Domain/ATS.Domain.csproj", "src/ATS.Domain/"]
COPY ["src/ATS.Infrastructure/ATS.Infrastructure.csproj", "src/ATS.Infrastructure/"]

RUN dotnet restore "src/ATS.API/ATS.API.csproj"

COPY . .
WORKDIR "/src/src/ATS.API"
RUN dotnet publish "ATS.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cria usuário não-root para execução da aplicação
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD wget -qO- http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "ATS.API.dll"]
