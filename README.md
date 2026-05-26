# 🚁 FireDrone

**Sistema avanzado de monitorización y control automatizado de flotas de drones**

## 📖 Descripción del Proyecto

**FireDrone** es un prototipo operativo completo diseñado como proyecto coordinado del Máster en Ingeniería Informática. El sistema permite controlar automáticamente el vuelo de múltiples drones, facilitando la monitorización en tiempo real de telemetría de los drones (estado del dron, batería, posición 3D, alarmas) y la visualización gráfica de su ubicación en el espacio.

## ✨ Características Principales

* 🤖 **Vuelo Automático y Rutas Optimizadas:** Los drones siguen de forma automática planes de vuelo basados en rutas predefinidas en la base de datos. Estas rutas están diseñadas para optimizar la cobertura de vigilancia del área asignada, esto se ha hecho con algoritmos genéticos.
* 🎮 **Control Manual y Override:** El operador puede interrumpir cualquier plan de vuelo en curso tomando el control manual del dron y ordenandole ir a una coordenada de forma inmediata. Al finalizar la intervención, el dron es capaz de retomar su plan original.
* 📡 **Monitorización en Tiempo Real:** El sistema recopila y muestra telemetría constante, incluyendo: posición 3D (latitud, longitud, altitud), velocidad, nivel de batería, datos de sensores e incidencias.
* 🔄 **Simulación y Entorno Real:** El proyecto soporta tanto la simulación de enjambres de drones como el control de drones reales, permitiendo escalar las pruebas de software de forma segura antes de los vuelos físicos que se han llevado a cabo en el master con un dron real comunicado con MavLink con el sistema.
* 🏗️ **Arquitectura Distribuida:** * **Estación Central:** Proporciona órdenes y planes de vuelo mediante una API REST.
* 👨‍💻 **Estación de Control:** Recibe las órdenes y controla físicamente a los drones asignados comunicándose a través de colas de mensajes y protocolos específicos.

---

## 📸 Vistas del Sistema

### Dashboard de Operaciones y Planes de Vuelo
Panel central para la gestión integral. Permite la monitorización del estado de todos los planes de vuelo, así como acciones rápidas para cancelar, reanudar o reiniciar operaciones en curso.

![Dashboard de Vuelos]<img width="1703" height="824" alt="dasshboard" src="https://github.com/user-attachments/assets/671bd72a-721d-41da-ba04-e53bb4e6a36c" />


### Mapa en Vivo (Live Map)
Vista de monitorización geográfica en tiempo real que muestra la posición exacta de cada unidad, su estado actual, batería restante y la ruta de vuelo asignada.

![Mapa en Vivo]<img width="635" height="713" alt="mapa" src="https://github.com/user-attachments/assets/562a99fe-c3a3-41dd-9874-ec5d2ff86eb5" />

### Mapa en Vivo (Live Map)
Vista 3d del dron usando Cesium y OpenGL para visualizar escenarios 3d en JavaScript.

![Mapa en 3d]<img width="580" height="379" alt="3d-dron" src="https://github.com/user-attachments/assets/6afb9caf-b52b-4972-8fec-e516c8231d7e" />

---

## 🚀 Despliegue y Ejecución

El proyecto está preparado para ser desplegado ágilmente utilizando contenedores, de hecho se encontraba continuamente desplegado en un servidor universitario con integración continua, sin embargo el servicio está abandonado por lo que no se incluirá ningún enlace o IP de acceso.
