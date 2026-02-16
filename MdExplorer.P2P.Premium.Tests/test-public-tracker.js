/**
 * Test con tracker pubblici WebTorrent
 * Per verificare se WebRTC/WebTorrent funziona in generale
 */

import WebTorrent from 'webtorrent';

// Tracker pubblici WebTorrent
const PUBLIC_TRACKERS = [
    'wss://tracker.openwebtorrent.com',
    'wss://tracker.btorrent.xyz',
    'wss://tracker.webtorrent.dev'
];

console.log('='.repeat(60));
console.log('Test WebTorrent con Tracker Pubblici');
console.log('='.repeat(60));
console.log('');
console.log('Trackers:', PUBLIC_TRACKERS.join(', '));
console.log('');

const client = new WebTorrent({
    dht: false,
    lsd: false
});

// Usa un torrent pubblico noto (Sintel - film open source)
// Questo è un torrent molto seedato, dovrebbe funzionare
const SINTEL_MAGNET = 'magnet:?xt=urn:btih:08ada5a7a6183aae1e09d831df6748d566095a10&dn=Sintel';

console.log('[TEST] Provo a scaricare Sintel (torrent pubblico molto seedato)...');
console.log('[TEST] Timeout: 60 secondi');
console.log('');

const torrent = client.add(SINTEL_MAGNET, {
    announce: PUBLIC_TRACKERS
});

const timeout = setTimeout(() => {
    console.log('');
    console.log('[TIMEOUT] Nessuna connessione dopo 60 secondi');
    console.log('[DEBUG] Peers:', torrent.numPeers);
    client.destroy();
    process.exit(1);
}, 60000);

torrent.on('infoHash', () => {
    console.log('[TORRENT] InfoHash:', torrent.infoHash);
});

torrent.on('metadata', () => {
    console.log('[TORRENT] Metadata ricevuti!');
    console.log('[TORRENT] Nome:', torrent.name);
    console.log('[TORRENT] Files:', torrent.files.length);
    clearTimeout(timeout);

    console.log('');
    console.log('='.repeat(60));
    console.log('[SUCCESS] WebTorrent/WebRTC FUNZIONA!');
    console.log('='.repeat(60));

    // Stop dopo aver verificato che funziona
    client.destroy();
    process.exit(0);
});

torrent.on('wire', (wire) => {
    console.log('[WIRE] Connesso a peer:', wire.remoteAddress || 'WebRTC');
});

torrent.on('warning', (err) => {
    console.warn('[WARN]', err.message || err);
});

torrent.on('error', (err) => {
    console.error('[ERROR]', err.message);
});

client.on('error', (err) => {
    console.error('[CLIENT ERROR]', err.message);
});
