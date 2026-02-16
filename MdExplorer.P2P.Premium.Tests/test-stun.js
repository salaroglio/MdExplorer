/**
 * Test STUN Server Connectivity
 *
 * Verifica che lo STUN server su errantia.net:3478 risponda correttamente
 * e restituisca i candidati ICE (host, srflx).
 */

// Questo test richiede un browser o wrtc per Node.js
// Per semplicità, usiamo un approccio con UDP socket per verificare la raggiungibilità

import dgram from 'dgram';

const STUN_SERVERS = [
    { host: 'errantia.net', port: 3478, name: 'errantia.net (custom)' },
    { host: 'stun.l.google.com', port: 19302, name: 'Google STUN' }
];

// STUN Binding Request (RFC 5389)
// Magic Cookie: 0x2112A442
const STUN_BINDING_REQUEST = Buffer.from([
    0x00, 0x01,             // Type: Binding Request
    0x00, 0x00,             // Length: 0
    0x21, 0x12, 0xA4, 0x42, // Magic Cookie
    // Transaction ID (12 bytes random)
    0x01, 0x02, 0x03, 0x04,
    0x05, 0x06, 0x07, 0x08,
    0x09, 0x0A, 0x0B, 0x0C
]);

async function testStunServer(server) {
    return new Promise((resolve) => {
        const socket = dgram.createSocket('udp4');
        const startTime = Date.now();
        let responded = false;

        const timeout = setTimeout(() => {
            if (!responded) {
                socket.close();
                resolve({
                    server: server.name,
                    host: server.host,
                    port: server.port,
                    success: false,
                    error: 'Timeout (5s)',
                    latency: null
                });
            }
        }, 5000);

        socket.on('message', (msg) => {
            responded = true;
            clearTimeout(timeout);
            const latency = Date.now() - startTime;

            // Verifica che sia una risposta STUN valida
            const type = msg.readUInt16BE(0);
            const isBindingResponse = type === 0x0101; // Binding Success Response

            socket.close();
            resolve({
                server: server.name,
                host: server.host,
                port: server.port,
                success: isBindingResponse,
                latency: latency,
                responseType: `0x${type.toString(16).padStart(4, '0')}`
            });
        });

        socket.on('error', (err) => {
            responded = true;
            clearTimeout(timeout);
            socket.close();
            resolve({
                server: server.name,
                host: server.host,
                port: server.port,
                success: false,
                error: err.message,
                latency: null
            });
        });

        // Invia STUN Binding Request
        socket.send(STUN_BINDING_REQUEST, server.port, server.host);
    });
}

async function main() {
    console.log('='.repeat(60));
    console.log('STUN Server Connectivity Test');
    console.log('='.repeat(60));
    console.log('');

    for (const server of STUN_SERVERS) {
        console.log(`[TEST] ${server.name} (${server.host}:${server.port})...`);

        const result = await testStunServer(server);

        if (result.success) {
            console.log(`  [OK] Risposta ricevuta in ${result.latency}ms`);
            console.log(`       Response Type: ${result.responseType} (Binding Success)`);
        } else {
            console.log(`  [FAIL] ${result.error || 'Risposta non valida'}`);
            if (result.responseType) {
                console.log(`         Response Type: ${result.responseType}`);
            }
        }
        console.log('');
    }

    console.log('='.repeat(60));
    console.log('Test completato');
    console.log('='.repeat(60));
}

main().catch(console.error);
