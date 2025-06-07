#!/usr/bin/env python3
import asyncio
import websockets
import json
from datetime import datetime

async def test_websocket():
    uri = "ws://localhost:5000/ws"
    print(f"Connecting to {uri}...")
    
    async with websockets.connect(uri) as websocket:
        print("Connected!")
        
        # Request initial snapshot
        await websocket.send(json.dumps({"type": "getSnapshot"}))
        print("Requested snapshot")
        
        # Listen for messages for 10 seconds
        try:
            timeout = 10
            print(f"Listening for {timeout} seconds...")
            start_time = datetime.now()
            message_count = 0
            
            while (datetime.now() - start_time).seconds < timeout:
                try:
                    message = await asyncio.wait_for(websocket.recv(), timeout=1)
                    data = json.loads(message)
                    message_count += 1
                    
                    if data.get("type") == "snapshot":
                        payload = data.get("payload", {})
                        particles = payload.get("particles", [])
                        metrics = payload.get("metrics", {})
                        print(f"[{datetime.now().strftime('%H:%M:%S')}] Snapshot received:")
                        print(f"  - Particles: {len(particles)}")
                        print(f"  - Simulation Time: {payload.get('simulationTime', 0):.3f}s")
                        print(f"  - Total Energy: {metrics.get('totalEnergy', 0):.3f} GeV")
                    else:
                        print(f"[{datetime.now().strftime('%H:%M:%S')}] {data.get('type')}: {str(data)[:100]}...")
                        
                except asyncio.TimeoutError:
                    print(".", end="", flush=True)
                    
            print(f"\nReceived {message_count} messages in {timeout} seconds")
            
        except Exception as e:
            print(f"Error: {e}")

asyncio.run(test_websocket())