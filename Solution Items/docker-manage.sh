#!/bin/bash

# EcommerShop Docker Management Scripts

case "$1" in
    "start")
        echo "Starting EcommerShop services..."
        docker-compose -f Solution\ Items/docker-compose.yml up -d
        ;;
    "start-dev")
        echo "Starting EcommerShop services in development mode..."
        docker-compose -f Solution\ Items/docker-compose.yml -f Solution\ Items/docker-compose.override.yml up -d
        ;;
    "start-prod")
        echo "Starting EcommerShop services in production mode..."
        docker-compose -f Solution\ Items/docker-compose.yml -f Solution\ Items/docker-compose.prod.yml up -d
        ;;
    "stop")
        echo "Stopping EcommerShop services..."
        docker-compose -f Solution\ Items/docker-compose.yml down
        ;;
    "restart")
        echo "Restarting EcommerShop services..."
        docker-compose -f Solution\ Items/docker-compose.yml down
        docker-compose -f Solution\ Items/docker-compose.yml up -d
        ;;
    "logs")
        echo "Showing logs for all services..."
        docker-compose -f Solution\ Items/docker-compose.yml logs -f
        ;;
    "logs-identity")
        echo "Showing logs for Identity API..."
        docker-compose -f Solution\ Items/docker-compose.yml logs -f identity-api
        ;;
    "logs-catalog")
        echo "Showing logs for Catalog API..."
        docker-compose -f Solution\ Items/docker-compose.yml logs -f catalog-api
        ;;
    "logs-webapp")
        echo "Showing logs for Web App..."
        docker-compose -f Solution\ Items/docker-compose.yml logs -f webapp
        ;;
    "build")
        echo "Building all services..."
        docker-compose -f Solution\ Items/docker-compose.yml build
        ;;
    "clean")
        echo "Cleaning up containers, networks, and volumes..."
        docker-compose -f Solution\ Items/docker-compose.yml down -v --remove-orphans
        docker system prune -f
        ;;
    "status")
        echo "Checking status of EcommerShop services..."
        docker-compose -f Solution\ Items/docker-compose.yml ps
        ;;
    *)
        echo "EcommerShop Docker Management"
        echo "Usage: $0 {start|start-dev|start-prod|stop|restart|logs|logs-identity|logs-catalog|logs-webapp|build|clean|status}"
        echo ""
        echo "Commands:"
        echo "  start       - Start all services"
        echo "  start-dev   - Start all services in development mode (with dev tools)"
        echo "  start-prod  - Start all services in production mode"
        echo "  stop        - Stop all services"
        echo "  restart     - Restart all services"
        echo "  logs        - Show logs for all services"
        echo "  logs-*      - Show logs for specific service"
        echo "  build       - Build all Docker images"
        echo "  clean       - Clean up containers, networks, and volumes"
        echo "  status      - Show status of all services"
        ;;
esac