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
        
        # Create a proton
        print("\nCreating a proton...")
        create_msg = {
            "type": "createParticle",
            "payload": {
                "type": "proton",
                "position": {"x": 5, "y": 5, "z": 5}
            }
        }
        print(f"Sending: {json.dumps(create_msg, indent=2)}")
        await websocket.send(json.dumps(create_msg))
        
        # Listen for any response
        print("\nListening for responses...")
        try:
            for i in range(5):
                response = await asyncio.wait_for(websocket.recv(), timeout=1)
                data = json.loads(response)
                print(f"\nReceived: {data['type']}")
                if data['type'] == 'error':
                    print(f"Error: {data.get('payload', {}).get('message', 'Unknown error')}")
                elif data['type'] == 'snapshot':
                    print(f"Snapshot with {len(data['payload']['particles'])} particles")
        except asyncio.TimeoutError:
            print("No more messages")

asyncio.run(test_create_particle())