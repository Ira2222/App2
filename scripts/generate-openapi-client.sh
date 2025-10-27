#!/usr/bin/env bash
set -euo pipefail

# Script to generate OpenAPI specification and TypeScript client
# This script:
# 1. Starts the API in the background
# 2. Exports the OpenAPI spec from Swagger
# 3. Generates the TypeScript client for the React app
# 4. Stops the API

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
API_PROJECT="${PROJECT_ROOT}/src/App2.Api"
OPENAPI_DIR="${PROJECT_ROOT}/openapi"
WEB_DIR="${PROJECT_ROOT}/apps/web"

echo "🚀 Starting OpenAPI client generation process..."
echo ""

# Step 1: Build the API
echo "📦 Building the API..."
cd "$PROJECT_ROOT"
dotnet build --configuration Release
echo ""

# Step 2: Start the API in the background
echo "🌐 Starting the API..."
cd "$API_PROJECT"
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --configuration Release &
API_PID=$!

# Give the API time to start
echo "⏳ Waiting for API to start..."
sleep 10

# Step 3: Health check
echo "🏥 Checking API health..."
for i in {1..10}; do
  if curl -sf http://localhost:5081/healthz/live > /dev/null 2>&1; then
    echo "✅ API is healthy!"
    break
  fi
  if [ $i -eq 10 ]; then
    echo "❌ API failed to start after 10 attempts"
    kill $API_PID 2>/dev/null || true
    exit 1
  fi
  echo "   Attempt $i/10..."
  sleep 2
done
echo ""

# Step 4: Export OpenAPI specification
echo "📋 Exporting OpenAPI specification..."
mkdir -p "$OPENAPI_DIR"

if curl -sf http://localhost:5081/swagger/v1/swagger.json > "${OPENAPI_DIR}/app2.openapi.json"; then
  echo "✅ OpenAPI spec exported to openapi/app2.openapi.json"
else
  echo "❌ Failed to export OpenAPI spec"
  kill $API_PID 2>/dev/null || true
  exit 1
fi
echo ""

# Step 5: Stop the API
echo "🛑 Stopping the API..."
kill $API_PID 2>/dev/null || true
wait $API_PID 2>/dev/null || true
echo ""

# Step 6: Install dependencies if needed
echo "📦 Installing web dependencies..."
cd "$WEB_DIR"
if [ ! -d "node_modules" ] || [ ! -d "node_modules/@openapitools" ]; then
  npm install
else
  echo "   Dependencies already installed"
fi
echo ""

# Step 7: Generate TypeScript client
echo "🔨 Generating TypeScript client..."
npm run generate:client
echo ""

echo "✅ OpenAPI client generation complete!"
echo ""
echo "📁 Generated files:"
echo "   - OpenAPI spec: openapi/app2.openapi.json"
echo "   - TypeScript client: apps/web/src/api/"
echo ""
echo "💡 Next steps:"
echo "   1. Review the generated client in apps/web/src/api/"
echo "   2. Update your React components to use the generated client"
echo "   3. Import the client: import { DefaultApi } from './api'"
echo ""
