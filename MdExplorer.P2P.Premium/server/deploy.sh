#!/bin/bash
# MdExplorer P2P Tracker - Deploy Script
# Run this on your VPS to deploy the tracker

set -e

# Configuration
INSTALL_DIR="/opt/mdexplorer-tracker"
DOMAIN="p2p.mdexplorer.net"
EMAIL="admin@mdexplorer.net"  # For Let's Encrypt notifications

echo "================================================"
echo "MdExplorer P2P Tracker - Deployment Script"
echo "================================================"

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root (sudo)"
    exit 1
fi

# Update system
echo "[1/7] Updating system..."
apt update && apt upgrade -y

# Install Docker if not present
if ! command -v docker &> /dev/null; then
    echo "[2/7] Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    rm get-docker.sh
else
    echo "[2/7] Docker already installed"
fi

# Install Docker Compose plugin if not present
if ! docker compose version &> /dev/null; then
    echo "[3/7] Installing Docker Compose..."
    apt install docker-compose-plugin -y
else
    echo "[3/7] Docker Compose already installed"
fi

# Create installation directory
echo "[4/7] Creating installation directory..."
mkdir -p $INSTALL_DIR
cd $INSTALL_DIR

# Check if files are present
if [ ! -f "docker-compose.yml" ]; then
    echo "ERROR: docker-compose.yml not found in $INSTALL_DIR"
    echo "Please copy the server files first:"
    echo "  scp -r server/* root@your-vps:$INSTALL_DIR/"
    exit 1
fi

# Configure firewall
echo "[5/7] Configuring firewall..."
ufw allow 8000/tcp    # HTTP/WebSocket
ufw allow 8000/udp    # UDP tracker
ufw --force enable

# Optional: Setup SSL with Certbot
read -p "Do you want to setup SSL with Let's Encrypt? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "[6/7] Setting up SSL..."

    # Install certbot
    apt install certbot -y

    # Stop any running services on port 80
    docker compose down 2>/dev/null || true

    # Get certificate
    certbot certonly --standalone -d $DOMAIN --email $EMAIL --agree-tos --non-interactive

    # Setup nginx config
    if [ -f "nginx.conf.example" ] && [ ! -f "nginx.conf" ]; then
        cp nginx.conf.example nginx.conf
        sed -i "s/p2p.mdexplorer.net/$DOMAIN/g" nginx.conf
    fi

    # Enable nginx in docker-compose
    # (You'll need to manually uncomment the nginx service)
    echo "SSL certificate obtained for $DOMAIN"
    echo "Remember to uncomment nginx service in docker-compose.yml"

    # Allow HTTPS
    ufw allow 443/tcp
    ufw allow 80/tcp
else
    echo "[6/7] Skipping SSL setup"
fi

# Start the tracker
echo "[7/7] Starting tracker..."
docker compose pull 2>/dev/null || docker compose build
docker compose up -d

echo ""
echo "================================================"
echo "Deployment complete!"
echo "================================================"
echo ""
echo "Tracker is running at:"
echo "  WebSocket: ws://$DOMAIN:8000"
echo "  HTTP:      http://$DOMAIN:8000/announce"
echo ""
echo "Commands:"
echo "  View logs:   docker compose logs -f"
echo "  Stop:        docker compose down"
echo "  Restart:     docker compose restart"
echo ""
echo "Test the tracker:"
echo "  curl http://localhost:8000/"
echo ""
