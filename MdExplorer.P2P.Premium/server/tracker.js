/**
 * MdExplorer P2P Tracker Server
 *
 * WebSocket-based BitTorrent tracker for WebTorrent clients.
 * Enables peer discovery for P2P file sharing in MdExplorer.
 *
 * AUTHENTICATION: Requires valid token in URL query parameter
 * Example: wss://errantia.net/p2p/?token=YOUR_TOKEN
 *
 * The token is validated at WebSocket upgrade time, before
 * the connection reaches bittorrent-tracker.
 */

import http from 'http';
import Server from 'bittorrent-tracker/server';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ============================================
// Token Management
// ============================================

const TOKENS_FILE = process.env.TOKENS_FILE || path.join(__dirname, 'tokens.json');

function loadTokens() {
    const tokens = new Set();

    if (process.env.TRACKER_TOKENS) {
        process.env.TRACKER_TOKENS.split(',').forEach(t => {
            const token = t.trim();
            if (token) tokens.add(token);
        });
        console.log(`[AUTH] Loaded ${tokens.size} tokens from environment`);
    }

    if (fs.existsSync(TOKENS_FILE)) {
        try {
            const fileContent = JSON.parse(fs.readFileSync(TOKENS_FILE, 'utf8'));
            const fileTokens = fileContent.tokens || [];
            fileTokens.forEach(t => {
                if (typeof t === 'string') {
                    tokens.add(t);
                } else if (t.token && t.enabled !== false) {
                    tokens.add(t.token);
                }
            });
            console.log(`[AUTH] Loaded tokens from ${TOKENS_FILE}`);
        } catch (err) {
            console.error(`[AUTH] Error loading tokens file: ${err.message}`);
        }
    }

    if (tokens.size === 0) {
        console.warn('[AUTH] WARNING: No tokens configured! All connections will be rejected.');
    }

    console.log(`[AUTH] Total valid tokens: ${tokens.size}`);
    return tokens;
}

let validTokens = loadTokens();

process.on('SIGHUP', () => {
    console.log('[AUTH] Received SIGHUP, reloading tokens...');
    validTokens = loadTokens();
});

function isValidToken(token) {
    return token && validTokens.has(token);
}

function extractTokenFromUrl(url) {
    try {
        const urlObj = new URL(url, 'http://localhost');
        return urlObj.searchParams.get('token');
    } catch {
        return null;
    }
}

// ============================================
// Configuration
// ============================================

const config = {
    httpPort: parseInt(process.env.TRACKER_HTTP_PORT) || 8000,
    host: process.env.TRACKER_HOST || '127.0.0.1',
    statsInterval: parseInt(process.env.TRACKER_STATS_INTERVAL) || 60000,
    trustProxy: process.env.TRUST_PROXY === 'true',
};

// ============================================
// Statistics
// ============================================

let rejectedConnections = 0;
let acceptedConnections = 0;

// ============================================
// HTTP Server with Auth Middleware
// ============================================

const httpServer = http.createServer((req, res) => {
    // Handle HTTP requests (announce via HTTP GET)
    const token = extractTokenFromUrl(req.url);

    if (!isValidToken(token)) {
        rejectedConnections++;
        const clientIP = req.headers['x-forwarded-for'] || req.socket.remoteAddress || 'unknown';
        console.log(`[AUTH] Rejected HTTP request from ${clientIP} - invalid token (total rejected: ${rejectedConnections})`);
        res.writeHead(401, { 'Content-Type': 'text/plain' });
        res.end('Unauthorized: Invalid or missing token');
        return;
    }

    // Valid token - let bittorrent-tracker handle it
    // The tracker attaches its own handler, this is just a fallback
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('MdExplorer P2P Tracker');
});

// ============================================
// WebSocket Upgrade Authentication
// ============================================

// Store original upgrade listeners to call after auth
const originalUpgradeListeners = [];

httpServer.on('newListener', (event, listener) => {
    if (event === 'upgrade') {
        // Intercept upgrade listeners added by bittorrent-tracker
        originalUpgradeListeners.push(listener);
    }
});

// Our auth middleware for WebSocket upgrades
httpServer.on('upgrade', (request, socket, head) => {
    const token = extractTokenFromUrl(request.url);
    const clientIP = request.headers['x-forwarded-for'] || request.socket.remoteAddress || 'unknown';

    if (!isValidToken(token)) {
        rejectedConnections++;
        console.log(`[AUTH] Rejected WebSocket from ${clientIP} - invalid token (total rejected: ${rejectedConnections})`);

        // Close the socket with HTTP 401 response
        socket.write('HTTP/1.1 401 Unauthorized\r\n');
        socket.write('Content-Type: text/plain\r\n');
        socket.write('Connection: close\r\n');
        socket.write('\r\n');
        socket.write('Invalid or missing authentication token');
        socket.destroy();
        return;
    }

    // Token valid - log and let it proceed
    acceptedConnections++;
    console.log(`[AUTH] Accepted WebSocket from ${clientIP} (total accepted: ${acceptedConnections})`);

    // Note: We don't need to manually call the tracker's upgrade handler
    // because we're using the 'upgrade' event which fires for all listeners
    // The bittorrent-tracker will receive this event too since it listens on httpServer
});

// ============================================
// Tracker Server (uses our HTTP server)
// ============================================

const tracker = new Server({
    http: httpServer,  // Use our custom HTTP server
    udp: false,
    ws: true,
    trustProxy: config.trustProxy,
});

// ============================================
// Event Handlers
// ============================================

tracker.on('error', function (err) {
    console.error('[ERROR]', err.message);
});

tracker.on('warning', function (err) {
    console.warn('[WARN]', err.message);
});

tracker.on('start', function (addr, params) {
    console.log(`[START] Peer from ${addr}`);
});

tracker.on('complete', function (addr) {
    console.log(`[COMPLETE] Peer ${addr}`);
});

tracker.on('stop', function (addr) {
    console.log(`[STOP] Peer ${addr}`);
});

// ============================================
// Stats
// ============================================

setInterval(function () {
    const torrents = Object.keys(tracker.torrents).length;
    let peers = 0;
    for (const hash in tracker.torrents) {
        if (tracker.torrents[hash].peers) {
            peers += Object.keys(tracker.torrents[hash].peers).length;
        }
    }
    console.log(`[STATS] Torrents: ${torrents}, Peers: ${peers}, Accepted: ${acceptedConnections}, Rejected: ${rejectedConnections}`);
}, config.statsInterval);

// ============================================
// Graceful Shutdown
// ============================================

process.on('SIGINT', function () {
    console.log('\n[SHUTDOWN] Received SIGINT...');
    tracker.close(() => {
        httpServer.close(() => {
            console.log('[SHUTDOWN] Server closed');
            process.exit(0);
        });
    });
});

process.on('SIGTERM', function () {
    console.log('\n[SHUTDOWN] Received SIGTERM...');
    tracker.close(() => {
        httpServer.close(() => {
            console.log('[SHUTDOWN] Server closed');
            process.exit(0);
        });
    });
});

// ============================================
// Start Server
// ============================================

httpServer.listen(config.httpPort, config.host, () => {
    console.log('='.repeat(50));
    console.log('MdExplorer P2P Tracker Server (Authenticated)');
    console.log('='.repeat(50));
    console.log(`Host: ${config.host}:${config.httpPort}`);
    console.log(`Tokens loaded: ${validTokens.size}`);
    console.log('');
    console.log('Client URL format:');
    console.log(`  wss://errantia.net/p2p/?token=YOUR_TOKEN`);
    console.log('');
    console.log('Reload tokens: kill -SIGHUP ' + process.pid);
    console.log('='.repeat(50));
});

export default tracker;
