#!/bin/bash

echo "========================================"
echo "ELIMINANDO TODOS LOS CONTENEDORES DE DRONES"
echo "========================================"

# Busca todos los contenedores que contengan el nombre "firedrone-drone-" y los fuerza a borrarse (-f)
docker rm -f $(docker ps -a -q --filter "name=firedrone-drone-") 2>/dev/null

echo "========================================"
echo "¡Todos los contenedores de drones han sido eliminados!"
echo ""
echo "SIGUIENTES PASOS (RECORDATORIO):"
echo "  1. Limpia la Base de Datos ejecutando el proyecto TestLoad (Opción 2)."
echo "  2. Vuelve a levantar el entorno base ejecutando:"
echo "     sudo docker-compose up -d"
echo "========================================"