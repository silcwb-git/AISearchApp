# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AISearchApp.Domain/AISearchApp.Domain.csproj", "src/AISearchApp.Domain/"]
COPY ["src/AISearchApp.Application/AISearchApp.Application.csproj", "src/AISearchApp.Application/"]
COPY ["src/AISearchApp.Infrastructure/AISearchApp.Infrastructure.csproj", "src/AISearchApp.Infrastructure/"]
COPY ["src/AISearchApp.WebAPI/AISearchApp.WebAPI.csproj", "src/AISearchApp.WebAPI/"]

RUN dotnet restore "src/AISearchApp.WebAPI/AISearchApp.WebAPI.csproj"

COPY . .
RUN dotnet build "src/AISearchApp.WebAPI/AISearchApp.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AISearchApp.WebAPI/AISearchApp.WebAPI.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "AISearchApp.WebAPI.dll"]