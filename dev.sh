#!/bin/bash
# ClaimIQ Development Server

set -e

echo "🚀 Starting ClaimIQ development servers..."

# Kill any existing processes
make kill

# Function to cleanup on exit
cleanup() {
    echo "🛑 Shutting down development servers..."
    make kill
    exit 0
}

# Set trap to catch Ctrl+C
trap cleanup INT TERM

# Start API server in background
echo "📦 Starting API server on http://localhost:5234..."
cd ClaimIq.Api
dotnet watch run --urls="http://localhost:5234" &
API_PID=$!
cd ..

# Wait for API to start
sleep 3

# Start Web server in background  
echo "🎨 Starting Web server on http://localhost:5001..."
cd ClaimIq.Web
dotnet watch run --urls="http://localhost:5001" &
WEB_PID=$!
cd ..

echo "✅ Development servers running!"
echo "📱 Web: http://localhost:5001"
echo "🔌 API: http://localhost:5234"
echo "🛑 Press Ctrl+C to stop both servers"

# Wait for either process to exit
wait $API_PID $WEB_PID