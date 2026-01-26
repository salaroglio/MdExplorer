# MdExplorer P2P Tracker Server

WebSocket-based BitTorrent tracker for MdExplorer P2P file sharing.

## Overview

This tracker enables peer discovery for WebTorrent clients in MdExplorer. It supports:
- **WebSocket** (primary) - for WebTorrent/browser clients
- **HTTP** - for standard BitTorrent announce/scrape
- **UDP** - for native BitTorrent clients

## Quick Start

### Local Development

```bash
# Install dependencies
npm install

# Start the tracker
npm start

# Or with hot reload
npm run dev
```

### Docker Deployment

```bash
# Build and run with Docker Compose
docker-compose up -d

# Check logs
docker-compose logs -f

# Stop
docker-compose down
```

## Configuration

Environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `TRACKER_HTTP_PORT` | 8000 | HTTP/WebSocket port |
| `TRACKER_UDP_PORT` | 8000 | UDP port |
| `TRACKER_WS_PORT` | 8000 | WebSocket port (same as HTTP) |
| `TRACKER_HTTP` | true | Enable HTTP protocol |
| `TRACKER_UDP` | true | Enable UDP protocol |
| `TRACKER_WS` | true | Enable WebSocket protocol |
| `TRACKER_STATS_INTERVAL` | 60000 | Stats logging interval (ms) |
| `TRUST_PROXY` | false | Trust X-Forwarded-For headers |

## Production Deployment

### 1. VPS Setup (Ubuntu/Debian)

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo apt install docker-compose-plugin -y

# Create directory
sudo mkdir -p /opt/mdexplorer-tracker
cd /opt/mdexplorer-tracker
```

### 2. Copy Files

```bash
# Copy server files to VPS
scp -r server/* user@your-vps:/opt/mdexplorer-tracker/
```

### 3. Configure SSL (Optional but Recommended)

```bash
# Install Certbot
sudo apt install certbot -y

# Get SSL certificate
sudo certbot certonly --standalone -d p2p.mdexplorer.net

# Copy nginx config
cp nginx.conf.example nginx.conf
# Edit nginx.conf with your domain

# Uncomment nginx service in docker-compose.yml
```

### 4. Start Tracker

```bash
cd /opt/mdexplorer-tracker
docker-compose up -d
```

### 5. Firewall Configuration

```bash
# Allow tracker ports
sudo ufw allow 8000/tcp   # HTTP/WebSocket
sudo ufw allow 8000/udp   # UDP tracker
sudo ufw allow 443/tcp    # HTTPS (if using nginx)
sudo ufw allow 80/tcp     # HTTP redirect (if using nginx)
```

## Client Configuration

### WebTorrent Client (Electron Plugin)

```javascript
const WebTorrent = require('webtorrent');

const client = new WebTorrent({
  tracker: {
    announce: [
      'wss://p2p.mdexplorer.net:443',  // WSS with nginx
      'ws://p2p.mdexplorer.net:8000'   // WS direct (fallback)
    ]
  }
});
```

### Magnet Link Format

Generated magnet links will include the tracker:

```
magnet:?xt=urn:btih:INFOHASH&dn=filename&tr=wss://p2p.mdexplorer.net
```

## Monitoring

### View Logs

```bash
docker-compose logs -f tracker
```

### Stats Output

The tracker logs stats every minute:
```
[STATS] Torrents: 5, Peers: 12
```

### Health Check

```bash
curl http://localhost:8000/
# Returns empty response with 200 OK if healthy
```

## Troubleshooting

### Connection Issues

1. **Check firewall**: Ensure ports 8000 (or 443 for SSL) are open
2. **Check logs**: `docker-compose logs tracker`
3. **Test connectivity**: `telnet your-vps 8000`

### WebSocket Issues

1. **Behind reverse proxy**: Set `TRUST_PROXY=true`
2. **SSL required**: Modern browsers require WSS for secure contexts

### No Peers Found

1. **Verify tracker URL** in client config
2. **Check if tracker is running**: `docker-compose ps`
3. **Both clients must use same tracker**

## Security Considerations

1. **Rate limiting**: Consider adding nginx rate limiting for production
2. **Firewall**: Only expose necessary ports
3. **Monitoring**: Set up alerting for abnormal traffic
4. **Updates**: Keep Docker images updated

## Architecture

```
                    ┌──────────────────────┐
                    │   Nginx (optional)   │
                    │   SSL Termination    │
                    │   :443 (wss)         │
                    └──────────┬───────────┘
                               │
                    ┌──────────▼───────────┐
                    │   Tracker Server     │
                    │   :8000 (ws/http)    │
                    └──────────┬───────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         │                     │                     │
┌────────▼────────┐  ┌────────▼────────┐  ┌────────▼────────┐
│  MdExplorer A   │  │  MdExplorer B   │  │  MdExplorer C   │
│  (Seeder)       │◄─►│  (Leecher)     │◄─►│  (Leecher)     │
└─────────────────┘  └─────────────────┘  └─────────────────┘
         ▲                     │                     │
         └─────────────────────┴─────────────────────┘
                    Direct P2P connections
```

## License

MIT
