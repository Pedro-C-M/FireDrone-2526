#!/bin/bash

# 1. Comprobar parámetro
if [ -z "$1" ]; then
    echo "Error: Debes indicar el número TOTAL de drones a desplegar."
    echo "Uso: ./lanzar_drones.sh <numero_total>"
    exit 1
fi

END=$1
IMAGE="firedrone-drone-controller"
NETWORK="firedrone-2526-3_firedrone-network"

echo "========================================"
echo "REINICIANDO Y DESPLEGANDO $END DRONES"
echo "========================================"

for i in $(seq 1 $END)
do
   NAME="firedrone-drone-$i"
   echo "   -> Configurando $NAME..."
   
   # Borra el contenedor a la fuerza si ya existe. 
   docker rm -f $NAME > /dev/null 2>&1
   
   # Crear el contenedor nuevo y limpio
   docker run -d \
        --name $NAME \
        --network $NETWORK \
        -e RABBITMQ_HOST=156.35.163.122 \
        -e RABBITMQ_USER=equipo3-777 \
        -e RABBITMQ_PASSWORD=shhh-777 \
        -e RABBITMQ_EXCHANGE=drone_exchange \
        $IMAGE "$i" "DroneSimulator" > /dev/null
done

echo "========================================"
echo "¡$END drones han sido recreados y están listos!"
echo "========================================"