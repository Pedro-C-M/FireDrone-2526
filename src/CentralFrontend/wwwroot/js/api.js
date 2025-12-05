import { ENDPOINTS } from './config.js';
import * as DroneService from './services/DroneService.js';
import * as FlightPlanService from './services/FlightPlanService.js';
import * as RealTimeService from './services/RealTimeService.js';

let flightplans = [];

// === INICIALIZACIÓN ===
document.addEventListener('DOMContentLoaded', async () => {
    console.log('Page loaded...');

    // 1. Cargamos SignalR
    await RealTimeService.startConnection(refreshDashboard);
    setupRealTimeListeners();

    // 2. Cargamos dropdowns
    await loadDronesToDropdowns();

    // 3. SOLO cargamos la tabla si estamos en el dashboard
    if (document.getElementById('flightplans_table')) {
        await getFlightPlans();
    }
});

function setupRealTimeListeners() {
    // Si tienes lógica de tiempo real, va aquí.
    RealTimeService.onDroneAssigned(() => refreshDashboard());
    RealTimeService.onDroneStateChanged(() => refreshDashboard());
}

async function refreshDashboard() {
    await loadDronesToDropdowns();
    if (document.getElementById('flightplans_table')) {
        await getFlightPlans();
    }
}

// === LÓGICA DE UI PARA DRONES ===
async function loadDronesToDropdowns() {
    try {
        const drones = await DroneService.getAllDrones();

        const selectAssign = document.getElementById('assign-droneId');
        const selectAdd = document.getElementById('add-dronId');
        const selectEdit = document.getElementById('edit-dronId');

        [selectAssign, selectAdd, selectEdit].forEach(select => {
            if (select) {
                while (select.options.length > 1) select.remove(1);
            }
        });

        drones.forEach(drone => {
            const option = document.createElement('option');
            option.value = drone.id;
            option.textContent = `Drone ${drone.id} - ${drone.state || 'Unknown'}`;
            if (selectAssign) selectAssign.appendChild(option.cloneNode(true));
            if (selectAdd) selectAdd.appendChild(option.cloneNode(true));
            if (selectEdit) selectEdit.appendChild(option.cloneNode(true));
        });

    } catch (error) {
        console.error('Error loading drones:', error);
    }
}

// === LÓGICA DE FLIGHT PLANS ===
async function getFlightPlans() {
    try {
        const data = await FlightPlanService.getAllFlightPlans();
        _displayFlightPlans(data);
    } catch (error) {
        console.error('Unable to get flight plans.', error);
    }
}

async function addFlightPlan() {
    const addDronIdInput = document.getElementById('add-dronId');
    const addRutaIdInput = document.getElementById('add-rutaId');
    const addEstControlIdInput = document.getElementById('add-estControlId');
    const addStartingPointIdInput = document.getElementById('add-startingPointId');
    const addStartingTimeInput = document.getElementById('add-startingTime');
    const addStateInput = document.getElementById('add-state');

    if (!addDronIdInput) return;

    const flightplan = {
        dronId: parseInt(addDronIdInput.value.trim()),
        rutaId: parseInt(addRutaIdInput.value.trim()),
        estControlId: addEstControlIdInput.value ? parseInt(addEstControlIdInput.value.trim()) : null,
        startingPointId: addStartingPointIdInput.value ? parseInt(addStartingPointIdInput.value.trim()) : null,
        startingTime: addStartingTimeInput.value,
        state: parseInt(addStateInput.value)
    };

    try {
        await FlightPlanService.createFlightPlan(flightplan);
        alert("Plan created successfully!");
        window.location.href = 'index.html';
    } catch (error) {
        console.error('Unable to add flight plan.', error);
        alert(`Error adding plan: ${error.message}`);
    }
}

async function deleteFlightPlan(id) {
    if (!confirm(`Are you sure you want to delete flight plan #${id}?`)) return;
    try {
        await FlightPlanService.deleteFlightPlan(id);
        alert(`Flight plan #${id} deleted successfully`);
        await getFlightPlans();
    } catch (error) {
        console.error('Unable to delete flight plan.', error);
    }
}

async function stopFlightPlan(id) {
    if (!confirm(`Are you sure you want to stop flight plan #${id}?`)) return;
    try {
        await FlightPlanService.stopFlightPlan(id);
        alert(`Flight plan #${id} stopped successfully!`);
        await getFlightPlans();
    } catch (error) {
        console.error('Error stopping flight plan:', error);
    }
}

async function switchToManualMode(id) {
    if (!confirm(`Switch flight plan #${id} to manual mode?`)) return;
    try {
        await FlightPlanService.setManualMode(id);
        alert(`Flight plan #${id} switched to manual mode!`);
        await getFlightPlans();
    } catch (error) {
        console.error('Error switching manual mode:', error);
    }
}

// === LÓGICA DE ACTUALIZACIÓN Y EDICIÓN ===
function displayEditForm(id) {
    const flightplan = flightplans.find(fp => fp.id === id);
    if (!flightplan) return;

    document.getElementById('edit-id').value = flightplan.id;
    document.getElementById('edit-dronId').value = flightplan.dronId;
    document.getElementById('edit-rutaId').value = flightplan.rutaId;
    // ... resto de campos ...
    document.getElementById('edit-state').value = flightplan.state;
    document.getElementById('editForm').style.display = 'block';
}

async function updateFlightPlan() {
    const flightplanId = parseInt(document.getElementById('edit-id').value);
    // ... Recogida de datos simplificada para el ejemplo ...
    const flightplanData = {
        id: flightplanId,
        dronId: parseInt(document.getElementById('edit-dronId').value),
        state: parseInt(document.getElementById('edit-state').value)
        // Añade el resto de campos si es necesario
    };

    try {
        await FlightPlanService.updateFlightPlan(flightplanId, flightplanData);
        await getFlightPlans();
        closeInput();
        await loadDronesToDropdowns();
    } catch (error) {
        console.error('Unable to update flight plan.', error);
    }
    return false;
}

async function assignDronToPlan() {
    const planId = document.getElementById('assign-planId').value;
    const droneId = document.getElementById('assign-droneId').value;

    if (!planId || !droneId) {
        alert("Please enter both a flight plan ID and select a drone.");
        return;
    }

    try {
        const result = await FlightPlanService.assignDrone(planId, droneId);
        alert(`Drone ${result.dronId} assigned to flight plan ${result.id} successfully!`);
        await getFlightPlans();

        // Limpiar inputs si existen
        const pInput = document.getElementById('assign-planId');
        const dInput = document.getElementById('assign-droneId');
        if (pInput) pInput.value = '';
        if (dInput) dInput.value = '';

    } catch (error) {
        console.error('Error assigning drone:', error);
        alert(`Failed to assign drone: ${error.message}`);
    }
}

function closeInput() {
    const form = document.getElementById('editForm');
    if (form) form.style.display = 'none';
}

function formatDateTime(str) { return new Date(str).toLocaleString(); }
function formatDateTimeLocal(str) {
    if (!str) return '';
    const d = new Date(str);
    const pad = n => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function _displayCount(count) {
    const counter = document.getElementById('counter');
    if (counter) counter.innerText = `${count} flight plans`;
}

// === FUNCIÓN DE VISUALIZACIÓN CORREGIDA ===
function _displayFlightPlans(data) {
    const tBody = document.getElementById('flightplans_tbody');

    // 1. Protección inicial: Si no hay tabla, no hacemos nada
    if (!tBody) return;

    tBody.innerHTML = '';
    _displayCount(data.length);

    // 2. Protección de template: Aseguramos que el template existe
    const template = document.getElementById('flightplan_row');
    if (!template) {
        console.error("Error: No se encuentra el template 'flightplan_row' en el HTML");
        return;
    }

    data.forEach(flightplan => {
        const clone = template.content.cloneNode(true);
        const td = clone.querySelectorAll('td');

        // Rellenar celdas básicas
        td[0].textContent = flightplan.id;
        td[1].textContent = flightplan.dronId;
        td[2].textContent = flightplan.rutaId;
        td[3].textContent = flightplan.estControlId || 'N/A';
        td[4].textContent = formatDateTime(flightplan.startingTime);
        td[5].textContent = flightplan.endingTime ? formatDateTime(flightplan.endingTime) : 'In Progress';

        // Badge de estado
        const badgeSpan = document.createElement('span');
        badgeSpan.className = `status-badge ${getStatusClass(flightplan.state)}`;
        badgeSpan.textContent = getStatusText(flightplan.state);
        td[6].innerHTML = '';
        td[6].appendChild(badgeSpan);

        // === ZONA DE BOTONES SEGURA ===

        // Función auxiliar: Intenta buscar el botón y añadir el evento.
        // Si no lo encuentra, no explota, solo lo ignora silenciosamente.
        const safeAddClick = (selector, action) => {
            const btn = clone.querySelector(selector);
            if (btn) {
                btn.addEventListener('click', (e) => {
                    e.preventDefault(); // Evita recargas raras
                    action();
                });
            } else {
                // Descomenta esto si quieres ver en consola qué botón falta
                // console.warn(`Aviso: Botón ${selector} no encontrado en el template para el plan ${flightplan.id}`);
            }
        };

        // Asignamos los eventos usando la función segura
        safeAddClick('.btn-edit', () => displayEditForm(flightplan.id));
        safeAddClick('.btn-delete', () => deleteFlightPlan(flightplan.id));
        safeAddClick('.btn-stop', () => stopFlightPlan(flightplan.id));
        safeAddClick('.btn-manual', () => switchToManualMode(flightplan.id));

        tBody.appendChild(clone);
    });

    flightplans = data;
}

function getStatusClass(state) {
    switch (state) {
        case 0: return 'bg-primary text-white';
        case 1: return 'bg-success text-white';
        case 2: return 'bg-danger text-white';
        default: return 'bg-secondary text-white';
    }
} // <--- ¡AQUÍ FALTABA EL CIERRE!

function getStatusText(state) {
    const s = ['On Course', 'Completed', 'Cancelled'];
    return s[state] || 'Unknown';
}

// === EXPOSICIÓN GLOBAL ===
window.addFlightPlan = addFlightPlan;
window.updateFlightPlan = updateFlightPlan;
window.assignDronToPlan = assignDronToPlan;
window.closeInput = closeInput;
window.getDrones = loadDronesToDropdowns;