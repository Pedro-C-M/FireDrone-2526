//Para centralizar las URLs de la API

const protocol = window.location.protocol;//http: o https:
const hostname = window.location.hostname; 
const port = 5306;

console.log(`Protocol: ${protocol}, Hostname: ${hostname}`);

export const API_BASE_URL = `${protocol}//${hostname}:${port}/api`;//156.35.163.122
export const HUB_URL = `${protocol}//${hostname}:${port}/droneHub`;

export const ENDPOINTS = {
    DRONES: `${API_BASE_URL}/Drone`,
    FLIGHT_PLANS: `${API_BASE_URL}/FlightPlan`,
    ROUTES: `${API_BASE_URL}/Route`
};