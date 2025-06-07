#!/bin/bash
# Start Orleans Silo as a daemon without waiting for console input

cd /home/elan/claudes/universe/Universe

# Kill any existing silo process
pkill -f "Universe.Silo" 2>/dev/null || true

# Start the silo with input redirected from /dev/null
nohup dotnet run --project Universe.Silo < /dev/null > silo.log 2>&1 &
SILO_PID=$!

echo "Starting Orleans Silo with PID: $SILO_PID"
sleep 5

# Check if it's still running
if ps -p $SILO_PID > /dev/null; then
    echo "Orleans Silo started successfully"
else
    echo "Orleans Silo failed to start. Check silo.log for details"
    tail -20 silo.log
fi