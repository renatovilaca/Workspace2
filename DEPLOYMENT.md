# Deployment Guide - Robot.ED.FacebookConnector System

This guide provides instructions for deploying the Robot.ED.FacebookConnector system in various environments.

## Table of Contents

1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Production Deployment](#production-deployment)
4. [Environment Configuration](#environment-configuration)
5. [Database Setup](#database-setup)
6. [Troubleshooting](#troubleshooting)

## Local Development

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+
- Chrome browser
- ChromeDriver (matching Chrome version)

### Steps

1. **Clone the repository:**
```bash
git clone <repository-url>
cd Workspace2
```

2. **Setup PostgreSQL database:**
```bash
createdb robotfacebookconnector
```

3. **Update appsettings.json (if needed):**

Update connection strings in:
- `Robot.ED.FacebookConnector.Service.API/appsettings.json`
- `Robot.ED.FacebookConnector.Service.RPA/appsettings.json`
- `Robot.ED.FacebookConnector.Dashboard/appsettings.json`

4. **Build the solution:**
```bash
dotnet build
```

5. **Run migrations (automatic on first run, or manually):**
```bash
cd Robot.ED.FacebookConnector.Service.API
dotnet ef database update

cd ../Robot.ED.FacebookConnector.Dashboard
dotnet ef database update
```

6. **Run the services:**

Open 3 terminal windows:

**Terminal 1 - API:**
```bash
cd Robot.ED.FacebookConnector.Service.API
dotnet run
```

**Terminal 2 - RPA:**
```bash
cd Robot.ED.FacebookConnector.Service.RPA
dotnet run
```

**Terminal 3 - Dashboard:**
```bash
cd Robot.ED.FacebookConnector.Dashboard
dotnet run
```

7. **Access the applications:**
- API: https://localhost:5001/swagger
- RPA: https://localhost:8081/swagger
- Dashboard: https://localhost:7001

8. **Login to Dashboard:**
- Username: `admin`
- Password: `admin@1932`

## Docker Deployment

### Prerequisites

- Docker
- Docker Compose

### Steps

1. **Clone the repository:**
```bash
git clone <repository-url>
cd Workspace2
```

2. **Build and run with Docker Compose:**
```bash
docker-compose up -d
```

3. **Check services status:**
```bash
docker-compose ps
```

4. **View logs:**
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f rpa
docker-compose logs -f dashboard
```

5. **Access the applications:**
- API: http://localhost:5000/swagger
- RPA: http://localhost:8080/swagger
- Dashboard: http://localhost:7000

6. **Stop services:**
```bash
docker-compose down
```

7. **Remove volumes (reset database):**
```bash
docker-compose down -v
```

## Production Deployment

### Prerequisites

- Linux server (Ubuntu 20.04+ recommended)
- Nginx (for reverse proxy)
- SSL certificates (Let's Encrypt recommended)
- PostgreSQL 12+
- Docker and Docker Compose

### Recommended Architecture

```
Internet
   |
   v
Nginx (Reverse Proxy + SSL)
   |
   +-- Dashboard (Port 7000)
   +-- API (Port 5000)
   +-- RPA (Port 8080)
   |
   v
PostgreSQL Database
```

### Steps

1. **Prepare the server:**
```bash
sudo apt update
sudo apt install -y docker.io docker-compose nginx
sudo systemctl enable docker
sudo systemctl start docker
```

2. **Clone and configure:**
```bash
cd /opt
sudo git clone <repository-url> robotfb
cd robotfb
```

3. **Create production configuration:**

Create `.env` file:
```bash
POSTGRES_DB=robotfacebookconnector
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<strong-password>
API_TOKEN=<generated-secure-token>
FACEBOOK_USERNAME=<real-username>
FACEBOOK_PASSWORD=<real-password>
```

Update docker-compose.yml to use .env file.

4. **Setup SSL certificates:**
```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.yourdomain.com
sudo certbot --nginx -d dashboard.yourdomain.com
```

5. **Configure Nginx:**

Create `/etc/nginx/sites-available/robotfb`:
```nginx
# Dashboard
server {
    listen 80;
    server_name dashboard.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl;
    server_name dashboard.yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/dashboard.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/dashboard.yourdomain.com/privkey.pem;
    
    location / {
        proxy_pass http://localhost:7000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# API
server {
    listen 80;
    server_name api.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl;
    server_name api.yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/api.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.yourdomain.com/privkey.pem;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site:
```bash
sudo ln -s /etc/nginx/sites-available/robotfb /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

6. **Start services:**
```bash
sudo docker-compose up -d
```

7. **Setup systemd service for auto-start:**

Create `/etc/systemd/system/robotfb.service`:
```ini
[Unit]
Description=Robot Facebook Connector
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/robotfb
ExecStart=/usr/bin/docker-compose up -d
ExecStop=/usr/bin/docker-compose down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
```

Enable service:
```bash
sudo systemctl enable robotfb
sudo systemctl start robotfb
```

## Environment Configuration

### Security Best Practices

1. **Change default passwords:**
   - Database password
   - Admin dashboard password
   - API tokens

2. **Use environment variables for secrets:**
```bash
export ConnectionStrings__DefaultConnection="Host=postgres;..."
export OrchestratorSettings__WebhookBearerToken="<secure-token>"
```

3. **Enable firewall:**
```bash
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

4. **Regular backups:**
```bash
# Backup database
docker exec robotfb-postgres pg_dump -U postgres robotfacebookconnector > backup.sql

# Backup volumes
docker run --rm -v robotfb_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/db-backup.tar.gz /data
```

## Database Setup

### Manual Migration

```bash
# API migrations
dotnet ef migrations add InitialCreate --project Robot.ED.FacebookConnector.Service.API
dotnet ef database update --project Robot.ED.FacebookConnector.Service.API

# Dashboard migrations
dotnet ef migrations add InitialCreate --project Robot.ED.FacebookConnector.Dashboard
dotnet ef database update --project Robot.ED.FacebookConnector.Dashboard
```

### Seed Data

The system automatically seeds:
- Default admin user (admin/admin@1932)
- Default API token (default-token)
- Sample RPA robots (RPA 1, RPA 2)

To add more data, update `Program.cs` in Dashboard project or insert via SQL.

## Troubleshooting

### ChromeDriver Issues

**Error:** ChromeDriver not found or version mismatch

**Solution:**
```bash
# Check Chrome version
google-chrome --version

# Download matching ChromeDriver
wget https://chromedriver.storage.googleapis.com/<version>/chromedriver_linux64.zip
unzip chromedriver_linux64.zip
sudo mv chromedriver /usr/local/bin/
sudo chmod +x /usr/local/bin/chromedriver
```

### Database Connection Issues

**Error:** Cannot connect to PostgreSQL

**Solution:**
```bash
# Check PostgreSQL is running
sudo systemctl status postgresql

# Check connection
psql -h localhost -U postgres -d robotfacebookconnector

# Check docker network
docker network inspect robotfb_default
```

### Memory Issues

**Error:** Out of memory

**Solution:**
```bash
# Increase Docker memory limit
# Edit /etc/docker/daemon.json
{
  "default-ulimits": {
    "memlock": {
      "Hard": -1,
      "Name": "memlock",
      "Soft": -1
    }
  }
}

# Restart Docker
sudo systemctl restart docker
```

### Port Conflicts

**Error:** Port already in use

**Solution:**
```bash
# Find process using port
sudo lsof -i :5000
sudo lsof -i :8080
sudo lsof -i :7000

# Kill process or change port in appsettings.json
```

## Monitoring

### Health Checks

- API: https://your-domain.com/api/health
- RPA: https://your-domain.com/api/health

### Logs

```bash
# Docker logs
docker-compose logs -f

# Application logs (in container)
docker exec -it robotfb-api cat /app/logs/app.log
```

### Database Monitoring

```bash
# Connect to database
docker exec -it robotfb-postgres psql -U postgres -d robotfacebookconnector

# Check queue status
SELECT COUNT(*) as pending FROM queue WHERE is_processed = false;
SELECT COUNT(*) as processed_today FROM queue 
WHERE is_processed = true AND updated_at >= CURRENT_DATE;

# Check robot status
SELECT * FROM robot;
```

## Maintenance

### Regular Tasks

1. **Monitor disk space:**
```bash
df -h
docker system df
```

2. **Clean old screenshots:**
```bash
find ./screenshots -name "*.png" -mtime +90 -delete
```

3. **Update Docker images:**
```bash
docker-compose pull
docker-compose up -d
```

4. **Backup database weekly:**
```bash
# Add to crontab
0 2 * * 0 docker exec robotfb-postgres pg_dump -U postgres robotfacebookconnector > /backups/backup-$(date +\%Y\%m\%d).sql
```

## Support

For additional help:
1. Check application logs
2. Review Swagger documentation
3. Contact system administrator
