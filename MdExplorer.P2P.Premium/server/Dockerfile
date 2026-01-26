# MdExplorer P2P Tracker Server
# Based on Node.js Alpine for minimal image size

FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies (production only)
RUN npm ci --only=production

# Copy application files
COPY tracker.js ./

# Create non-root user for security
RUN addgroup -g 1001 -S tracker && \
    adduser -S tracker -u 1001 -G tracker

# Switch to non-root user
USER tracker

# Environment variables with defaults
ENV NODE_ENV=production \
    TRACKER_HTTP_PORT=8000 \
    TRACKER_UDP_PORT=8000 \
    TRACKER_WS_PORT=8000 \
    TRACKER_HTTP=true \
    TRACKER_UDP=true \
    TRACKER_WS=true \
    TRACKER_STATS_INTERVAL=60000 \
    TRUST_PROXY=false

# Expose ports
# HTTP/WebSocket
EXPOSE 8000/tcp
# UDP
EXPOSE 8000/udp

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8000/ || exit 1

# Start the tracker
CMD ["node", "tracker.js"]
