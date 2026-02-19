========================================================
CHULETA DE COMANDOS - PRUEBAS DE CARGA FIREDRONE
========================================================

1. DAR PERMISOS A LOS SCRIPTS (Si no tienen):
--------------------------------------------------------
sudo chmod +x lanzar_drones.sh
sudo chmod +x borrar_contenedores.sh


2. PREPARAR TODO (Despliegue de Drones):
--------------------------------------------------------
Asegúrate de haber poblado la BD con TestLoad primero con n drones.
Luego, ejecuta el script pasándole ese n de drones a lanzar que creaste antes.
(Ejemplo para 50 drones)

./lanzar_drones.sh 50


3. LIMPIAR DESPUÉS DE LA PRUEBA (Borrar Drones):
--------------------------------------------------------
Elimina de Docker todos los contenedores de drones creados.

./borrar_contenedores.sh


4. VOLVER A LA NORMALIDAD (Entorno de Desarrollo):
--------------------------------------------------------
Limpia la BD de drones y planes de vuelo extra con la aplicación de consola TestLoad
 
Levanta de nuevo los 11 drones estándar configurados en tu YAML.

docker-compose up -d