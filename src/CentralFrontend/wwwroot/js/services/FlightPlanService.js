// js/services/FlightPlanService.js
import { ENDPOINTS } from '../config.js';

/**
 * Obtiene todos los planes de vuelo
 */
export async function getAllFlightPlans() {
    const response = await fetch(ENDPOINTS.FLIGHT_PLANS);
    if (!response.ok) throw new Error(`Error fetching flight plans: ${response.status}`);
    return await response.json();
}

/**
 * Crea un nuevo plan de vuelo
 */
export async function createFlightPlan(flightPlanData) {
    const response = await fetch(ENDPOINTS.FLIGHT_PLANS, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(flightPlanData)
    });
    if (!response.ok) throw new Error(`Error creating flight plan: ${response.status}`);
    return await response.json();
}

/**
 * Actualiza un plan existente
 */
export async function updateFlightPlan(id, flightPlanData) {
    const response = await fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(flightPlanData)
    });
    if (!response.ok) throw new Error(`Error updating flight plan: ${response.status}`);
    // PUT a veces no devuelve contenido, así que devolvemos true si todo fue bien
    return true;
}

/**
 * Elimina un plan de vuelo
 */
export async function deleteFlightPlan(id) {
    const response = await fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    if (!response.ok) throw new Error(`Error deleting flight plan: ${response.status}`);
    return true;
}

/**
 * Detiene un plan de vuelo
 */
export async function stopFlightPlan(id) {
    const response = await fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}/stop`, {
        method: 'PUT',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }
    });
    if (!response.ok) throw new Error(`Error stopping flight plan: ${response.status}`);
    return await response.json();
}

/**
 * Cambia a modo manual
 */
export async function setManualMode(id) {
    const response = await fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}/manual`, {
        method: 'PUT',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }
    });
    if (!response.ok) throw new Error(`Error switching to manual: ${response.status}`);
    return await response.json();
}

/**
 * Asigna un dron a un plan
 */
export async function assignDrone(planId, droneId, restartFromBeginning = false) {
    const response = await fetch(`${ENDPOINTS.FLIGHT_PLANS}/${planId}/assign`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            DronId: parseInt(droneId),
            RestartFromBeginning: restartFromBeginning
        })
    });
    if (!response.ok) throw new Error(`Error assigning drone: ${response.status}`);
    return await response.json();
}

/**
 * Envía comando goto para modo manual
 */
export async function sendGotoCommand(planId, latitude, longitude) {
    const response = await fetch(`${ENDPOINTS.FLIGHT_PLANS}/${planId}/goto`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Latitude: parseFloat(latitude),
            Longitude: parseFloat(longitude)
        })
    });
    if (!response.ok) throw new Error(`Error sending goto command: ${response.status}`);
    return await response.json();
}