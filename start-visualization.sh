#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting Universe Quantum Simulator Visualization${NC}"
echo ""

# Function to check if a port is in use
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null 2>&1; then
        return 0
    else
        return 1
    fi
}

# Start Orleans Silo
echo -e "${YELLOW}1. Starting Orleans Silo...${NC}"
if check_port 30000; then
    echo -e "${GREEN}✓ Orleans Silo already running${NC}"
else
    echo "Starting Orleans Silo in background..."
    dotnet run --project Universe.Silo > silo.log 2>&1 &
    SILO_PID=$!
    echo "Silo PID: $SILO_PID"
    
    # Wait for silo to start
    echo "Waiting for Silo to start..."
    sleep 5
    
    if check_port 30000; then
        echo -e "${GREEN}✓ Orleans Silo started successfully${NC}"
    else
        echo -e "${RED}✗ Failed to start Orleans Silo${NC}"
        echo "Check silo.log for errors"
        exit 1
    fi
fi
echo ""

# Start API Server
echo -e "${YELLOW}2. Starting API Server...${NC}"
if check_port 5000; then
    echo -e "${GREEN}✓ API Server already running${NC}"
else
    echo "Starting API Server in background..."
    dotnet run --project Universe.Api > api.log 2>&1 &
    API_PID=$!
    echo "API PID: $API_PID"
    
    # Wait for API to start
    echo "Waiting for API to start..."
    sleep 3
    
    if check_port 5000; then
        echo -e "${GREEN}✓ API Server started successfully${NC}"
    else
        echo -e "${RED}✗ Failed to start API Server${NC}"
        echo "Check api.log for errors"
        exit 1
    fi
fi
echo ""

# Start Web Visualization
echo -e "${YELLOW}3. Starting Web Visualization...${NC}"
cd Universe.Visualization

# Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo "Installing npm dependencies..."
    npm install
fi

echo "Starting development server..."
npm run dev &
WEB_PID=$!
echo "Web PID: $WEB_PID"
echo ""

# Wait a moment for the web server to start
sleep 3

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Universe Quantum Simulator is running!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "Services:"
echo "  • Orleans Dashboard: http://localhost:8080"
echo "  • API Server: http://localhost:5000"
echo "  • Web Visualization: http://localhost:3000"
echo ""
echo "Logs:"
echo "  • Silo: silo.log"
echo "  • API: api.log"
echo ""
echo -e "${YELLOW}Press Ctrl+C to stop all services${NC}"
echo ""

# Function to cleanup on exit
cleanup() {
    echo ""
    echo -e "${YELLOW}Stopping services...${NC}"
    
    if [ ! -z "$WEB_PID" ]; then
        kill $WEB_PID 2>/dev/null
    fi
    
    if [ ! -z "$API_PID" ]; then
        kill $API_PID 2>/dev/null
    fi
    
    if [ ! -z "$SILO_PID" ]; then
        kill $SILO_PID 2>/dev/null
    fi
    
    # Also kill any remaining dotnet processes for this project
    pkill -f "Universe.Silo" 2>/dev/null
    pkill -f "Universe.Api" 2>/dev/null
    
    echo -e "${GREEN}All services stopped${NC}"
    exit 0
}

# Set up trap to cleanup on Ctrl+C
trap cleanup INT

# Wait forever
while true; do
    sleep 1
done