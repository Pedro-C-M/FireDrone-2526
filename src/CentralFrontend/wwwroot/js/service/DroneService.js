// js/services/DroneService.js
import { ENDPOINTS } from '../config.js';

/**
 * Obtiene todos los drones del backend
 * @returns {Promise<Array>} Lista de drones
 */
export async function getAllDrones() {
    console.log('Service: Fetching drones from:', ENDPOINTS.DRONES);
    const response = await fetch(ENDPOINTS.DRONES);

    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data;
}