#!/bin/sh

# Run our three programs concurrently with some formatting
yarn dlx -q concurrently -ir --default-input-target 0 \
  "cd api && dotnet watch" \
  "cd ui && yarn dev" \
  "caddy run"
