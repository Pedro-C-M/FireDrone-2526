// js/services/RouteService.js
import { ENDPOINTS } from '../config.js';

/**
 * Obtiene todas las rutas disponibles
 */
export async function getAllRoutes() {
    const response = await fetch(ENDPOINTS.ROUTES);
    if (!response.ok) throw new Error(`Error fetching routes: ${response.status}`);
    return await response.json();
}

/**
 * Importa rutas desde un archivo CSV
 * @param {FormData} formData - Objeto FormData que contiene el archivo
 */
export async function importRoutes(formData) {
    const response = await fetch(`${ENDPOINTS.ROUTES}/import`, {
        method: 'POST',
        // NO poner headers de Content-Type aquí, fetch lo gestiona al ver FormData
        body: formData
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Error importing routes: ${response.status} - ${errorText}`);
    }

    return await response.json();
}
/**
 * Elimina una ruta por su ID
 */
export async function deleteRoute(id) {
    const response = await fetch(`${ENDPOINTS.ROUTES}/${id}`, {
        method: 'DELETE'
    });

    if (!response.ok) {
        throw new Error(`Error deleting route: ${response.status}`);
    }

    return true; // Éxito
}