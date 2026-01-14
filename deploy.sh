#!/bin/bash

echo "LIMPIEZA TOTAL: Borrando todos los contenedores..."
docker rm -f $(docker ps -a -q) 2>/dev/null || true

# Build images
echo "Building images..."
docker build -f src/CentralBackend/Dockerfile -t firedrone-central-backend .
docker build -f src/ControlBackend/Dockerfile -t firedrone-control-backend .
docker build -f src/CentralFrontend/Dockerfile -t firedrone-frontend .
docker build -f src/DroneController/Dockerfile -t firedrone-drone-controller .

# Create network
docker network create firedrone-network 2>/dev/null || true

# Base de datos de Caché (Redis)
echo "Starting Redis on port 5308..."
docker run -d \
  --name firedrone-redis \
  --network firedrone-network \
  -p 5308:5308 \
  --restart unless-stopped \
  redis:alpine \
  redis-server --port 5308

# Run containers
echo "Starting containers..."
docker run -d --name firedrone-central-backend --network firedrone-network -p 5306:5306 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:5306 \
  -e ControlBackend__Url=http://firedrone-control-backend:5307 \
  -e ConnectionStrings__Redis=firedrone-redis:5308,abortConnect=false \
  -v $(pwd)/data:/app/data --restart unless-stopped firedrone-central-backend

docker run -d --name firedrone-control-backend --network firedrone-network -p 5307:5307 \
  -e ASPNETCORE_ENVIRONMENT=Production -e ASPNETCORE_URLS=http://+:5307 \
  -e RABBITMQ_HOST=156.35.163.122 -e RABBITMQ_USER=admin -e RABBITMQ_PASSWORD=admin \
  -e RABBITMQ_EXCHANGE=drone_exchange -e RABBITMQ_TOPIC=drone.# \
  --restart unless-stopped firedrone-control-backend

docker run -d --name firedrone-frontend --network firedrone-network -p 5305:5305 \
  -e ASPNETCORE_ENVIRONMENT=Production -e ASPNETCORE_URLS=http://+:5305 \
  --restart unless-stopped firedrone-frontend

for i in {1..11}; do
  docker run -d --name firedrone-drone-$i --network firedrone-network \
    -e RABBITMQ_HOST=156.35.163.122 -e RABBITMQ_USER=admin -e RABBITMQ_PASSWORD=admin \
    -e RABBITMQ_EXCHANGE=drone_exchange --restart unless-stopped \
    firedrone-drone-controller $i DroneSimulator
done

echo "Deployment complete!"
docker ps
