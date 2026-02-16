/**
 * MdExplorer P2P Test Download Client
 *
 * Client di test per scaricare file via magnet link usando:
 * - STUN server: errantia.net:3478
 * - Tracker: wss://errantia.net/p2p/ (con autenticazione token)
 *
 * Usage:
 *   node test-download-client.js <magnet-link>
 *   node test-download-client.js <infohash>
 *
 * Example:
 *   node test-download-client.js "magnet:?xt=urn:btih:abc123..."
 *   node test-download-client.js abc123def456...
 */

import WebTorrent from 'webtorrent';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ============================================
// Configuration
// ============================================

const CONFIG = {
    // Token di autenticazione per il tracker
    token: 'mdexp_2026_premium_abc123xyz',

    // Tracker URL (con token)
    get trackerUrl() {
        return `wss://errantia.net/p2p/?token=${this.token}`;
    },

    // ICE Servers per WebRTC (STUN + TURN)
    iceServers: [
        // STUN servers
        { urls: 'stun:errantia.net:3478' },
        { urls: 'stun:stun.l.google.com:19302' },
        // TURN server (relay for symmetric NAT)
        {
            urls: 'turn:errantia.net:3478',
            username: 'mdexplorer',
            credential: 'MdExp2026P2P!'
        }
    ],

    // Directory di download
    downloadPath: path.join(__dirname, 'downloads'),

    // Timeout in secondi
    timeout: 120
};

// ============================================
// Helpers
// ============================================

function formatBytes(bytes) {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function formatSpeed(bytesPerSec) {
    return formatBytes(bytesPerSec) + '/s';
}

function buildMagnetUri(input) {
    // Se è già un magnet link, usalo
    if (input.startsWith('magnet:')) {
        // Aggiungi il nostro tracker se non presente
        if (!input.includes('errantia.net')) {
            const separator = input.includes('&') ? '&' : '&';
            return input + separator + 'tr=' + encodeURIComponent(CONFIG.trackerUrl);
        }
        return input;
    }

    // Altrimenti è un infohash, costruisci il magnet
    const infohash = input.replace(/[^a-fA-F0-9]/g, '');
    return `magnet:?xt=urn:btih:${infohash}&tr=${encodeURIComponent(CONFIG.trackerUrl)}`;
}

// ============================================
// Main
// ============================================

async function main() {
    const input = process.argv[2];

    if (!input) {
        console.log('Usage: node test-download-client.js <magnet-link|infohash>');
        console.log('');
        console.log('Examples:');
        console.log('  node test-download-client.js "magnet:?xt=urn:btih:abc123..."');
        console.log('  node test-download-client.js abc123def456789...');
        process.exit(1);
    }

    const magnetUri = buildMagnetUri(input);

    console.log('='.repeat(60));
    console.log('MdExplorer P2P Test Download Client');
    console.log('='.repeat(60));
    console.log('');
    console.log('[CONFIG] STUN Server:', CONFIG.iceServers[0].urls);
    console.log('[CONFIG] Tracker:', CONFIG.trackerUrl.replace(CONFIG.token, '***'));
    console.log('[CONFIG] Download path:', CONFIG.downloadPath);
    console.log('[CONFIG] Timeout:', CONFIG.timeout, 'seconds');
    console.log('');
    console.log('[MAGNET]', magnetUri.substring(0, 80) + '...');
    console.log('');

    // Crea client WebTorrent con configurazione custom
    // IMPORTANTE: dht:false per usare SOLO il tracker autenticato
    const client = new WebTorrent({
        maxConns: 55,
        dht: false,    // Disabilita DHT (peer discovery non autenticato)
        lsd: false,    // Disabilita Local Service Discovery
        tracker: {
            rtcConfig: {
                iceServers: CONFIG.iceServers
            }
        }
    });

    client.on('error', (err) => {
        console.error('[CLIENT ERROR]', err.message);
    });

    console.log('[STATUS] Inizializzazione client WebTorrent...');
    console.log('[STATUS] Connessione al tracker...');
    console.log('');

    // Aggiungi il torrent
    const torrent = client.add(magnetUri, {
        path: CONFIG.downloadPath,
        announce: [CONFIG.trackerUrl]
    });

    // Timeout
    const timeoutId = setTimeout(() => {
        console.log('');
        console.log('[TIMEOUT] Nessun peer trovato dopo', CONFIG.timeout, 'secondi');
        console.log('[DEBUG] InfoHash:', torrent.infoHash);
        console.log('[DEBUG] Peers connessi:', torrent.numPeers);

        client.destroy(() => {
            process.exit(1);
        });
    }, CONFIG.timeout * 1000);

    // Eventi torrent
    torrent.on('infoHash', () => {
        console.log('[TORRENT] InfoHash:', torrent.infoHash);
    });

    torrent.on('metadata', () => {
        console.log('[TORRENT] Metadata ricevuti!');
        console.log('[TORRENT] Nome:', torrent.name);
        console.log('[TORRENT] Dimensione:', formatBytes(torrent.length));
        console.log('[TORRENT] Files:', torrent.files.length);
        torrent.files.forEach((file, i) => {
            console.log(`  [${i + 1}] ${file.name} (${formatBytes(file.length)})`);
        });
        console.log('');
    });

    torrent.on('ready', () => {
        console.log('[TORRENT] Pronto per il download');
    });

    torrent.on('warning', (err) => {
        console.warn('[TORRENT WARN]', err.message || err);
    });

    torrent.on('error', (err) => {
        console.error('[TORRENT ERROR]', err.message);
    });

    // Tracker events
    torrent.on('trackerAnnounce', () => {
        console.log('[TRACKER] Announce inviato');
    });

    torrent.on('trackerWarning', (err) => {
        console.warn('[TRACKER WARN]', err.message || err);
    });

    torrent.on('trackerError', (err) => {
        console.error('[TRACKER ERROR]', err.message || err);
    });

    // Peer events
    torrent.on('peer', (peer) => {
        console.log('[PEER] Nuovo peer trovato:', peer);
    });

    torrent.on('wire', (wire) => {
        console.log('[WIRE] Connesso a peer:', wire.remoteAddress || 'WebRTC');
    });

    // Progress
    let lastProgress = -1;
    torrent.on('download', (bytes) => {
        const progress = Math.floor(torrent.progress * 100);
        if (progress !== lastProgress && progress % 5 === 0) {
            lastProgress = progress;
            console.log(`[DOWNLOAD] ${progress}% - ${formatBytes(torrent.downloaded)} / ${formatBytes(torrent.length)} - ${formatSpeed(torrent.downloadSpeed)} - Peers: ${torrent.numPeers}`);
        }
    });

    // Completato
    torrent.on('done', () => {
        clearTimeout(timeoutId);
        console.log('');
        console.log('='.repeat(60));
        console.log('[DONE] Download completato!');
        console.log('='.repeat(60));
        console.log('');
        console.log('File salvati in:', CONFIG.downloadPath);
        torrent.files.forEach((file) => {
            console.log('  -', file.path);
        });
        console.log('');

        // Chiudi dopo 2 secondi
        setTimeout(() => {
            client.destroy(() => {
                console.log('[STATUS] Client chiuso');
                process.exit(0);
            });
        }, 2000);
    });
}

main().catch((err) => {
    console.error('[FATAL]', err);
    process.exit(1);
});
