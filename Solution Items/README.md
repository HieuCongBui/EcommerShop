# EcommerShop Docker Setup

This directory contains Docker configuration files for running the EcommerShop microservices application.

## Architecture

The application consists of the following services:

- **Identity.API** - Authentication and authorization service using OpenIddict + Identity
- **Catalog.API** - Product catalog management service
- **WebApp** - Frontend web application (Aspire AppHost)
- **PostgreSQL** - Database for Identity and Catalog services
- **Redis** - Caching and session storage
- **Nginx** - Reverse proxy and load balancer
- **pgAdmin** - Database management tool (development only)
- **Redis Commander** - Redis management tool (development only)

## Quick Start

### Prerequisites

- Docker and Docker Compose installed
- .NET 8 SDK (for local development)

### 1. Environment Setup

Copy the environment template and customize it:

```bash
cp .env.template .env
# Edit .env file with your specific configuration
```

### 2. Start Services

#### Development Mode (with dev tools)
```bash
# Windows
docker-manage.bat start-dev

# Linux/Mac
./docker-manage.sh start-dev
```

#### Production Mode
```bash
# Windows
docker-manage.bat start-prod

# Linux/Mac
./docker-manage.sh start-prod
```

### 3. Access the Application

- **Web Application**: http://localhost:5000
- **Identity API**: http://localhost:5001
- **Catalog API**: http://localhost:5002
- **pgAdmin**: http://localhost:5050 (dev mode only)
- **Redis Commander**: http://localhost:8081 (dev mode only)

## Service Configuration

### Databases

- **Identity Database**: PostgreSQL on port 5432
  - Database: `IdentityDb`
  - Username: `postgres`
  - Password: Configured in environment

- **Catalog Database**: PostgreSQL on port 5433
  - Database: `CatalogDb`
  - Username: `postgres`
  - Password: Configured in environment

### Networking

All services communicate through the `ecommershop-network` bridge network.

### Health Checks

Health checks are configured for all services:
- Database services: PostgreSQL connection check
- API services: HTTP health endpoints
- Redis: Redis ping command

## Development

### Building Images

```bash
# Build all services
docker-manage.bat build

# Build specific service
docker-compose -f docker-compose.yml build identity-api
```

### Viewing Logs

```bash
# All services
docker-manage.bat logs

# Specific service
docker-manage.bat logs-identity
docker-manage.bat logs-catalog
docker-manage.bat logs-webapp
```

### Database Management

#### pgAdmin Access (Development)
- URL: http://localhost:5050
- Email: admin@ecommershop.com
- Password: Admin@123456

#### Manual Database Connection
```bash
# Connect to Identity database
docker exec -it ecommershop-identity-db psql -U postgres -d IdentityDb

# Connect to Catalog database
docker exec -it ecommershop-catalog-db psql -U postgres -d CatalogDb
```

## Production Deployment

### Security Considerations

1. **Secrets Management**: Use Docker secrets or external secret management
2. **SSL/TLS**: Configure proper SSL certificates
3. **Database Security**: Use strong passwords and restrict access
4. **Network Security**: Use proper firewall rules

### Environment Variables

Production environment variables should be configured through:
- Docker secrets
- Environment variable files
- External configuration management

### Scaling

The production configuration supports scaling:

```bash
# Scale API services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --scale identity-api=3 --scale catalog-api=3
```

## Troubleshooting

### Common Issues

1. **Port Conflicts**: Check if ports 5000-5002, 5432-5433, 6379, 8081 are available
2. **Database Connection Issues**: Ensure database containers are healthy before starting API services
3. **Memory Issues**: Ensure sufficient Docker memory allocation (minimum 4GB recommended)

### Debug Commands

```bash
# Check service status
docker-manage.bat status

# View container logs
docker logs ecommershop-identity-api

# Execute commands in container
docker exec -it ecommershop-identity-api bash

# Check network connectivity
docker network inspect ecommershop-network
```

### Cleanup

```bash
# Stop and remove all containers, networks, and volumes
docker-manage.bat clean
```

## File Structure

```
Solution Items/
??? docker-compose.yml              # Main compose file
??? docker-compose.override.yml     # Development overrides
??? docker-compose.prod.yml         # Production overrides
??? .env.template                   # Environment template
??? docker-manage.bat               # Windows management script
??? docker-manage.sh                # Linux/Mac management script
??? nginx/
?   ??? nginx.conf                  # Nginx configuration
??? README.md                       # This file

src/
??? Identity.API/
?   ??? Dockerfile                  # Identity API Docker image
?   ??? .dockerignore
??? Catalog.API/
?   ??? Dockerfile                  # Catalog API Docker image
?   ??? .dockerignore
??? WebApp/
    ??? Dockerfile                  # Web App Docker image
    ??? .dockerignore
```

## Contributing

When adding new services:

1. Create appropriate Dockerfile in the service directory
2. Add service configuration to docker-compose.yml
3. Update networking and dependency configuration
4. Add health checks
5. Update this README

## Support

For issues and questions, please refer to the main project documentation or create an issue in the project repository.