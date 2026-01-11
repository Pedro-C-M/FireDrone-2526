#!/bin/bash

echo "Stopping FireDrone services..."

# Stop all containers
docker stop firedrone-central-backend firedrone-control-backend firedrone-frontend 2>/dev/null
docker stop firedrone-drone-{1..5} 2>/dev/null

echo "All services stopped"

# Optional: Remove containers
# docker rm firedrone-central-backend firedrone-control-backend firedrone-frontend
# docker rm firedrone-drone-{1..5}
