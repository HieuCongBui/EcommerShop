@echo off
REM EcommerShop Docker Management Scripts for Windows

if "%1"=="start" (
    echo Starting EcommerShop services...
    docker-compose -f "Solution Items/docker-compose.yml" up -d
    goto :end
)

if "%1"=="start-dev" (
    echo Starting EcommerShop services in development mode...
    docker-compose -f "Solution Items/docker-compose.yml" -f "Solution Items/docker-compose.override.yml" up -d
    goto :end
)

if "%1"=="start-prod" (
    echo Starting EcommerShop services in production mode...
    docker-compose -f "Solution Items/docker-compose.yml" -f "Solution Items/docker-compose.prod.yml" up -d
    goto :end
)

if "%1"=="stop" (
    echo Stopping EcommerShop services...
    docker-compose -f "Solution Items/docker-compose.yml" down
    goto :end
)

if "%1"=="restart" (
    echo Restarting EcommerShop services...
    docker-compose -f "Solution Items/docker-compose.yml" down
    docker-compose -f "Solution Items/docker-compose.yml" up -d
    goto :end
)

if "%1"=="logs" (
    echo Showing logs for all services...
    docker-compose -f "Solution Items/docker-compose.yml" logs -f
    goto :end
)

if "%1"=="logs-identity" (
    echo Showing logs for Identity API...
    docker-compose -f "Solution Items/docker-compose.yml" logs -f identity-api
    goto :end
)

if "%1"=="logs-catalog" (
    echo Showing logs for Catalog API...
    docker-compose -f "Solution Items/docker-compose.yml" logs -f catalog-api
    goto :end
)

if "%1"=="logs-webapp" (
    echo Showing logs for Web App...
    docker-compose -f "Solution Items/docker-compose.yml" logs -f webapp
    goto :end
)

if "%1"=="build" (
    echo Building all services...
    docker-compose -f "Solution Items/docker-compose.yml" build
    goto :end
)

if "%1"=="clean" (
    echo Cleaning up containers, networks, and volumes...
    docker-compose -f "Solution Items/docker-compose.yml" down -v --remove-orphans
    docker system prune -f
    goto :end
)

if "%1"=="status" (
    echo Checking status of EcommerShop services...
    docker-compose -f "Solution Items/docker-compose.yml" ps
    goto :end
)

echo EcommerShop Docker Management
echo Usage: %0 {start^|start-dev^|start-prod^|stop^|restart^|logs^|logs-identity^|logs-catalog^|logs-webapp^|build^|clean^|status}
echo.
echo Commands:
echo   start       - Start all services
echo   start-dev   - Start all services in development mode (with dev tools)
echo   start-prod  - Start all services in production mode
echo   stop        - Stop all services
echo   restart     - Restart all services
echo   logs        - Show logs for all services
echo   logs-*      - Show logs for specific service
echo   build       - Build all Docker images
echo   clean       - Clean up containers, networks, and volumes
echo   status      - Show status of all services

:end
cd src/Identity.API
dotnet add package OpenIddict.AspNetCore
dotnet add package OpenIddict.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools