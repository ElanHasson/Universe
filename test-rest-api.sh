#!/bin/bash

echo "Testing REST API particle creation..."

# Get initial snapshot
echo -e "\n1. Getting initial snapshot:"
curl -s http://localhost:5000/api/snapshot | jq '.particles | length'

# Create a proton
echo -e "\n2. Creating a proton:"
curl -s -X POST http://localhost:5000/api/particle \
  -H "Content-Type: application/json" \
  -d '{
    "type": "proton",
    "position": {"x": 10, "y": 10, "z": 10}
  }' | jq '.'

# Get snapshot after creation
echo -e "\n3. Getting snapshot after creation:"
curl -s http://localhost:5000/api/snapshot | jq '.particles | length'

# Create a neutron
echo -e "\n4. Creating a neutron:"
curl -s -X POST http://localhost:5000/api/particle \
  -H "Content-Type: application/json" \
  -d '{
    "type": "neutron",
    "position": {"x": -10, "y": -10, "z": -10}
  }' | jq '.'

# Get final snapshot
echo -e "\n5. Getting final snapshot:"
curl -s http://localhost:5000/api/snapshot | jq '.particles | length'