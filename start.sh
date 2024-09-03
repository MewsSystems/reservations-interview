#!/bin/sh

# Run our three programs concurrently with some formatting
npx -q concurrently -ir --default-input-target 0 \
  "cd backend/api && dotnet watch" \
  "cd ui && npm start" \
  "caddy run"
