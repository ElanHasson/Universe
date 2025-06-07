#!/usr/bin/env python3
import asyncio
import websockets
import json
from datetime import datetime

async def test_create_particle():
    uri = "ws://localhost:5000/ws"
    print(f"Connecting to {uri}...")
    
    async with websockets.connect(uri) as websocket:
        print("Connected!")
        
        # Request initial snapshot
        await websocket.send(json.dumps({"type": "getSnapshot"}))
        response = await websocket.recv()
        data = json.loads(response)
        initial_count = len(data['payload']['particles'])
        print(f"Initial particle count: {initial_count}")
        
        # Create a proton
        print("\nCreating a proton...")
        create_msg = {
            "type": "createParticle",
            "payload": {
                "type": "proton",
                "position": {"x": 5, "y": 5, "z": 5}
            }
        }
        await websocket.send(json.dumps(create_msg))
        
        # Wait for snapshot update
        await asyncio.sleep(0.5)
        
        # Request new snapshot
        await websocket.send(json.dumps({"type": "getSnapshot"}))
        response = await websocket.recv()
        data = json.loads(response)
        new_count = len(data['payload']['particles'])
        print(f"New particle count: {new_count}")
        
        if new_count > initial_count:
            print("✓ Particle created successfully!")
        else:
            print("✗ Particle creation failed")
            
        # Try creating a neutron
        print("\nCreating a neutron...")
        create_msg = {
            "type": "createParticle",
            "payload": {
                "type": "neutron",
                "position": {"x": -5, "y": -5, "z": -5}
            }
        }
        await websocket.send(json.dumps(create_msg))
        
        # Wait for snapshot update
        await asyncio.sleep(0.5)
        
        # Request final snapshot
        await websocket.send(json.dumps({"type": "getSnapshot"}))
        response = await websocket.recv()
        data = json.loads(response)
        final_count = len(data['payload']['particles'])
        print(f"Final particle count: {final_count}")
        
        if final_count > new_count:
            print("✓ Second particle created successfully!")
        else:
            print("✗ Second particle creation failed")

asyncio.run(test_create_particle())