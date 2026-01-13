// wwwroot/js/services/RealTimeService.js
import { HUB_URL } from '../config.js';

let connection = null;

/**
 * Inicia la conexión con SignalR
 * @param {Function} onStatusChange Callback opcional al conectar/reconectar
 */
export async function startConnection(onStatusChange) {
    // Verificamos si la librería de Microsoft está cargada en el HTML
    if (typeof signalR === 'undefined') {
        console.error('SignalR library not found. Check your HTML scripts.');
        return false;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, { withCredentials: true })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    // Eventos de conexión
    connection.onreconnecting(() => console.warn('SignalR: Reconnecting...'));
    connection.onreconnected(() => {
        console.log('SignalR: Reconnected');
        if (onStatusChange) onStatusChange();
    });

    try {
        await connection.start();
        console.log('SignalR: Connected');
        return true;
    } catch (err) {
        console.error('SignalR Connection Error:', err);
        return false;
    }
}

// === SUSCRIPCIONES A EVENTOS ===

/**
 * Suscribirse a actualizaciones individuales de drones en tiempo real
 */
export function onDroneUpdate(callback) {
    if (connection) connection.on("ReceiveDroneUpdate", callback);
}

export function onDroneAssigned(callback) {
    if (connection) connection.on("DroneAssigned", callback);
}

export function onDroneStateChanged(callback) {
    if (connection) connection.on("DroneStateChanged", callback);
}

export function onAllDronesUpdate(callback) {
    if (connection) connection.on("ReceiveAllDrones", callback);
}

/**
 * Obtener el estado de la conexión
 */
export function getConnectionState() {
    return connection ? connection.state : 'Disconnected';
}