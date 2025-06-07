const WebSocket = require('ws');

const ws = new WebSocket('ws://localhost:5000/ws');

ws.on('open', function open() {
    console.log('Connected to WebSocket');
    // Request initial snapshot
    ws.send(JSON.stringify({ type: 'getSnapshot' }));
});

ws.on('message', function message(data) {
    const msg = JSON.parse(data);
    if (msg.type === 'snapshot') {
        console.log(`[${new Date().toISOString()}] Received snapshot:`);
        console.log(`  - Simulation Time: ${msg.payload.simulationTime}`);
        console.log(`  - Particles: ${msg.payload.particles.length}`);
        console.log(`  - Total Energy: ${msg.payload.metrics.totalEnergy}`);
        console.log('---');
    }
});

ws.on('error', function error(err) {
    console.error('WebSocket error:', err);
});

ws.on('close', function close() {
    console.log('Disconnected from WebSocket');
});

// Keep the script running
setTimeout(() => {
    console.log('Test complete');
    ws.close();
    process.exit(0);
}, 10000);