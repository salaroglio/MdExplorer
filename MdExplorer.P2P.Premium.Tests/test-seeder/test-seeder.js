/**
 * MdExplorer Test Seeder
 *
 * Standalone seeder for testing P2P downloads.
 * Run this on a remote computer, then use the magnet link to download from MdExplorer.
 *
 * Usage:
 *   node test-seeder.js [token]
 *
 * Example:
 *   node test-seeder.js mySecretToken123
 */

import WebTorrent from 'webtorrent';
import fs from 'fs';
import path from 'path';
import crypto from 'crypto';
import { fileURLToPath } from 'url';

// ESM equivalent of __dirname
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Configuration
const TRACKER_URL = 'wss://errantia.net/p2p/';
const TEST_FILE_NAME = 'test-file.txt';
const TEST_FILE_SIZE_KB = 100; // Size in KB

// Token hardcoded for errantia.net tracker
const token = 'mdexp_2026_premium_abc123xyz';

console.log('==========================================');
console.log('  MdExplorer Test Seeder');
console.log('==========================================');
console.log('');

// Build tracker URL with token
const trackerUrl = token ? `${TRACKER_URL}?token=${token}` : TRACKER_URL;
console.log('Tracker:', TRACKER_URL);
console.log('Token:', token ? 'SET' : 'NOT SET');
console.log('');

// Create test file if it doesn't exist
const testFilePath = path.join(__dirname, TEST_FILE_NAME);

if (!fs.existsSync(testFilePath)) {
  console.log(`Creating test file (${TEST_FILE_SIZE_KB} KB)...`);

  // Generate random content
  const content = [];
  content.push(`MdExplorer P2P Test File`);
  content.push(`Generated: ${new Date().toISOString()}`);
  content.push(`Size: ${TEST_FILE_SIZE_KB} KB`);
  content.push('');
  content.push('--- Random Data Below ---');
  content.push('');

  // Fill with random data to reach desired size
  const headerSize = content.join('\n').length;
  const remainingBytes = (TEST_FILE_SIZE_KB * 1024) - headerSize;
  const randomData = crypto.randomBytes(Math.max(0, remainingBytes)).toString('base64');
  content.push(randomData);

  fs.writeFileSync(testFilePath, content.join('\n'));
  console.log('Test file created:', testFilePath);
} else {
  console.log('Using existing test file:', testFilePath);
}

const fileStats = fs.statSync(testFilePath);
console.log('File size:', (fileStats.size / 1024).toFixed(2), 'KB');
console.log('');

// Create WebTorrent client
// SECURITY: dht/lsd disabled to enforce tracker authentication (no public broadcast)
console.log('Initializing WebTorrent client...');
console.log('  - DHT: DISABLED (private tracker only)');
console.log('  - LSD: DISABLED (no local broadcast)');
console.log('  - STUN/TURN: errantia.net');
console.log('');

const client = new WebTorrent({
  dht: false,    // Disable DHT - use only authenticated tracker
  lsd: false,    // Disable Local Service Discovery
  tracker: {
    rtcConfig: {
      iceServers: [
        // STUN servers (for NAT discovery)
        { urls: 'stun:errantia.net:3478' },
        { urls: 'stun:stun.l.google.com:19302' },
        // TURN server (relay for symmetric NAT)
        {
          urls: 'turn:errantia.net:3478',
          username: 'mdexplorer',
          credential: 'MdExp2026P2P!'
        }
      ]
    }
  }
});

client.on('error', (err) => {
  console.error('Client error:', err.message);
});

// Seed the file
console.log('Starting to seed...');
console.log('');

client.seed(testFilePath, {
  name: TEST_FILE_NAME,
  announce: [trackerUrl],
  // Private torrent = no DHT/PEX sharing
  private: true
}, (torrent) => {
  console.log('==========================================');
  console.log('  SEEDING ACTIVE');
  console.log('==========================================');
  console.log('');
  console.log('Torrent name:', torrent.name);
  console.log('Info hash:', torrent.infoHash);
  console.log('');
  console.log('MAGNET LINK (copy this to MdExplorer):');
  console.log('');
  console.log(torrent.magnetURI);
  console.log('');
  console.log('==========================================');
  console.log('');
  console.log('Waiting for peers...');
  console.log('Press Ctrl+C to stop seeding.');
  console.log('');

  // Track connections
  torrent.on('wire', (wire) => {
    const peerAddr = wire.remoteAddress || 'WebRTC';
    console.log(`[${new Date().toLocaleTimeString()}] Peer connected: ${peerAddr}`);
  });

  torrent.on('upload', (bytes) => {
    // Log every 100KB uploaded
    if (torrent.uploaded % (100 * 1024) < bytes) {
      console.log(`[${new Date().toLocaleTimeString()}] Uploaded: ${(torrent.uploaded / 1024).toFixed(0)} KB, Speed: ${(torrent.uploadSpeed / 1024).toFixed(1)} KB/s, Peers: ${torrent.numPeers}`);
    }
  });

  // Periodic status
  setInterval(() => {
    if (torrent.numPeers > 0) {
      console.log(`[${new Date().toLocaleTimeString()}] Status: ${torrent.numPeers} peers, Uploaded: ${(torrent.uploaded / 1024).toFixed(0)} KB, Speed: ${(torrent.uploadSpeed / 1024).toFixed(1)} KB/s`);
    }
  }, 10000);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('');
  console.log('Shutting down...');
  client.destroy(() => {
    console.log('WebTorrent client destroyed.');
    process.exit(0);
  });
});

process.on('SIGTERM', () => {
  client.destroy(() => {
    process.exit(0);
  });
});
