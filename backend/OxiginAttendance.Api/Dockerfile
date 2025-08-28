# Backend Dockerfile for OxiginAttendance.Api
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OxiginAttendance.Api.csproj", "."]
RUN dotnet restore "./OxiginAttendance.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "OxiginAttendance.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OxiginAttendance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "OxiginAttendance.Api.dll"]