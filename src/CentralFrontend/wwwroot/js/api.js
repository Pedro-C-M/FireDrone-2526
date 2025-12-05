// js/api.js
import { ENDPOINTS } from './config.js';           
import * as DroneService from './services/DroneService.js'; 

let flightplans = [];

// === INICIALIZACIÓN ===
// Usamos DOMContentLoaded en lugar de window.onload para módulos
document.addEventListener('DOMContentLoaded', async () => {
    console.log('Page loaded, fetching data...');
    await loadDronesToDropdowns(); // Nombre más descriptivo
    getFlightPlans();
});

// === LÓGICA DE UI PARA DRONES ===

/**
 * Llama al servicio de drones y rellena los selectores HTML
 */
async function loadDronesToDropdowns() {
    try {
        // 1. LLAMADA AL SERVICIO (Separación de datos)
        const drones = await DroneService.getAllDrones();
        console.log('Drones loaded via Service:', drones);

        // 2. LÓGICA DE UI (Manipulación del DOM)
        const selectAssign = document.getElementById('assign-droneId');
        const selectAdd = document.getElementById('add-dronId');
        const selectEdit = document.getElementById('edit-dronId');

        // Limpiar opciones existentes (menos la primera)
        [selectAssign, selectAdd, selectEdit].forEach(select => {
            if (select) {
                while (select.options.length > 1) {
                    select.remove(1);
                }
            }
        });

        // Añadir nuevas opciones
        drones.forEach(drone => {
            const option = document.createElement('option');
            option.value = drone.id;
            option.textContent = `Drone ${drone.id} - ${drone.state || 'Unknown'}`;

            if (selectAssign) selectAssign.appendChild(option.cloneNode(true));
            if (selectAdd) selectAdd.appendChild(option.cloneNode(true));
            if (selectEdit) selectEdit.appendChild(option.cloneNode(true));
        });

        console.log(`UI Updated: ${drones.length} drones in dropdowns`);

    } catch (error) {
        console.error('Error loading drones:', error);
        alert('Failed to load drones. Please check if the backend is running on http://localhost:5306');
    }
}

// === LÓGICA DE FLIGHT PLANS (Se mantiene igual, pero usando la nueva función de drones) ===

function getFlightPlans() {
    fetch(ENDPOINTS.FLIGHT_PLANS)
        .then(response => response.json())
        .then(data => _displayFlightPlans(data))
        .catch(error => console.error('Unable to get flight plans.', error));
}

function addFlightPlan() {
    const addDronIdInput = document.getElementById('add-dronId');
    const addRutaIdInput = document.getElementById('add-rutaId');
    const addEstControlIdInput = document.getElementById('add-estControlId');
    const addStartingPointIdInput = document.getElementById('add-startingPointId');
    const addStartingTimeInput = document.getElementById('add-startingTime');
    const addStateInput = document.getElementById('add-state');

    const flightplan = {
        dronId: parseInt(addDronIdInput.value.trim()),
        rutaId: parseInt(addRutaIdInput.value.trim()),
        estControlId: addEstControlIdInput.value ? parseInt(addEstControlIdInput.value.trim()) : null,
        startingPointId: addStartingPointIdInput.value ? parseInt(addStartingPointIdInput.value.trim()) : null,
        startingTime: addStartingTimeInput.value,
        state: parseInt(addStateInput.value)
    };

    fetch(ENDPOINTS.FLIGHT_PLANS, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(flightplan)
    })
        .then(response => response.json())
        .then(() => {
            getFlightPlans();
            loadDronesToDropdowns(); // CAMBIO AQUÍ: Usamos la nueva función UI

            // Limpiar formulario
            addDronIdInput.value = '';
            addRutaIdInput.value = '';
            addEstControlIdInput.value = '';
            addStartingPointIdInput.value = '';
            addStartingTimeInput.value = '';
            addStateInput.value = '0';
        })
        .catch(error => console.error('Unable to add flight plan.', error));
}

function deleteFlightPlan(id) {
    console.log(`Deleting flight plan ${id}...`);
    fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (!response.ok) throw new Error(`Failed to delete flight plan: ${response.status}`);
            return response;
        })
        .then(() => {
            alert(`Flight plan #${id} deleted successfully`);
            getFlightPlans();
        })
        .catch(error => {
            console.error('Unable to delete flight plan.', error);
            alert(`Error deleting flight plan: ${error.message}`);
        });
}

// ... [stopFlightPlan y switchToManualMode se mantienen igual] ...

function stopFlightPlan(id) {
    console.log(`Stopping flight plan ${id}...`);
    if (!confirm(`Are you sure you want to stop flight plan #${id}?`)) return;

    fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}/stop`, {
        method: 'PUT',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }
    })
        .then(response => {
            if (!response.ok) throw new Error(`Failed: ${response.status}`);
            return response.json();
        })
        .then(data => {
            alert(`Flight plan #${id} stopped successfully!`);
            getFlightPlans();
        })
        .catch(error => alert(error.message));
}

function switchToManualMode(id) {
    if (!confirm(`Switch flight plan #${id} to manual mode?`)) return;
    fetch(`${ENDPOINTS.FLIGHT_PLANS}/${id}/manual`, {
        method: 'PUT',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' }
    })
        .then(res => res.ok ? res.json() : Promise.reject(res))
        .then(() => {
            alert(`Manual mode activated for #${id}`);
            getFlightPlans();
        })
        .catch(err => console.error(err));
}

// ... [displayEditForm se mantiene igual] ...

function displayEditForm(id) {
    const flightplan = flightplans.find(fp => fp.id === id);
    if (!flightplan) return;

    document.getElementById('edit-id').value = flightplan.id;
    document.getElementById('edit-dronId').value = flightplan.dronId;
    document.getElementById('edit-rutaId').value = flightplan.rutaId;
    document.getElementById('edit-estControlId').value = flightplan.estControlId || '';
    document.getElementById('edit-startingPointId').value = flightplan.startingPointId || '';
    document.getElementById('edit-startingTime').value = formatDateTimeLocal(flightplan.startingTime);
    document.getElementById('edit-endingTime').value = flightplan.endingTime ? formatDateTimeLocal(flightplan.endingTime) : '';
    document.getElementById('edit-state').value = flightplan.state;
    document.getElementById('editForm').style.display = 'block';
}

function updateFlightPlan() {
    const flightplanId = parseInt(document.getElementById('edit-id').value);
    const flightplan = {
        id: flightplanId,
        dronId: parseInt(document.getElementById('edit-dronId').value.trim()),
        rutaId: parseInt(document.getElementById('edit-rutaId').value.trim()),
        estControlId: document.getElementById('edit-estControlId').value ? parseInt(document.getElementById('edit-estControlId').value.trim()) : null,
        startingPointId: document.getElementById('edit-startingPointId').value ? parseInt(document.getElementById('edit-startingPointId').value.trim()) : null,
        startingTime: document.getElementById('edit-startingTime').value,
        endingTime: document.getElementById('edit-endingTime').value || null,
        state: parseInt(document.getElementById('edit-state').value)
    };

    fetch(`${ENDPOINTS.FLIGHT_PLANS}/${flightplanId}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(flightplan)
    })
        .then(() => {
            getFlightPlans();
            closeInput();
            loadDronesToDropdowns(); // CAMBIO AQUÍ
        })
        .catch(error => console.error('Unable to update flight plan.', error));

    return false;
}

function assignDronToPlan() {
    const planId = document.getElementById('assign-planId').value;
    const droneId = document.getElementById('assign-droneId').value;

    if (!planId || !droneId) {
        alert("Please enter both a flight plan ID and select a drone.");
        return;
    }

    const payload = { DronId: parseInt(droneId) };
    const url = `${ENDPOINTS.FLIGHT_PLANS}/${planId}/assign`;

    fetch(url, {
        method: 'PUT',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(response => {
            if (!response.ok) throw new Error(`Assignment failed: ${response.status}`);
            return response.json();
        })
        .then(data => {
            alert(`Drone ${data.dronId} assigned to flight plan ${data.id} successfully!`);
            getFlightPlans();
            document.getElementById('assign-planId').value = '';
            document.getElementById('assign-droneId').value = '';
        })
        .catch(error => {
            console.error('Error assigning drone:', error);
            alert(`Failed to assign drone: ${error.message}`);
        });
}

// === UTILIDADES ===
function closeInput() { document.getElementById('editForm').style.display = 'none'; }
function formatDateTime(dateTimeString) { return new Date(dateTimeString).toLocaleString(); }
function formatDateTimeLocal(dateTimeString) {
    if (!dateTimeString) return '';
    const date = new Date(dateTimeString);
    const pad = (n) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
function getStatusText(state) {
    const states = ['On Course', 'Completed', 'Cancelled'];
    return states[state] || 'Unknown';
}

function _displayFlightPlans(data) {
    // ... (Tu función _displayFlightPlans original) ...
    // Se mantiene casi igual, solo asegúrate de llamar a las funciones globales en los onclick
    // Como estamos en un módulo, necesitamos exponer las funciones al window o usar addEventListener
    // Para simplificar, he añadido la exposición al window abajo.

    // NOTA: He resumido esta parte para no ocupar tanto espacio, 
    // pero copia tu _displayFlightPlans original aquí.
    const tBody = document.getElementById('flightplans_tbody');
    tBody.innerHTML = '';
    _displayCount(data.length);
    const template = document.getElementById('flightplan_row');

    data.forEach(flightplan => {
        const clone = template.content.cloneNode(true);
        const td = clone.querySelectorAll('td');
        // ... (resto del mapeo de celdas) ...
        td[0].textContent = flightplan.id;
        td[1].textContent = flightplan.dronId;
        td[2].textContent = flightplan.rutaId;
        td[3].textContent = flightplan.estControlId || 'N/A';
        td[4].textContent = formatDateTime(flightplan.startingTime);
        td[5].textContent = flightplan.endingTime ? formatDateTime(flightplan.endingTime) : 'In Progress';
        td[6].textContent = getStatusText(flightplan.state);

        // BOTONES: Asignamos eventos directamente en JS para evitar problemas de scope
        clone.querySelector('.btn-warning').onclick = () => displayEditForm(flightplan.id);
        clone.querySelector('.btn-danger').onclick = () => { if (confirm('Delete?')) deleteFlightPlan(flightplan.id) };
        const stopBtn = clone.querySelector('.btn-stop');
        if (stopBtn) stopBtn.onclick = () => { if (confirm('Stop?')) stopFlightPlan(flightplan.id) };
        const manualBtn = clone.querySelector('.btn-manual');
        if (manualBtn) manualBtn.onclick = () => { if (confirm('Manual mode?')) switchToManualMode(flightplan.id) };

        tBody.appendChild(clone);
    });
    flightplans = data;
}

function _displayCount(count) {
    document.getElementById('counter').innerText = `${count} flight plans`;
}


// === ¡IMPORTANTE! EXPOSICIÓN GLOBAL ===
// Como api.js ahora es un módulo (tiene import), sus funciones son PRIVADAS.
// El HTML (onsubmit="addFlightPlan()") no puede verlas a menos que las hagamos globales así:

window.addFlightPlan = addFlightPlan;
window.updateFlightPlan = updateFlightPlan;
window.assignDronToPlan = assignDronToPlan;
window.closeInput = closeInput;
// getDrones ya no es necesaria exponerla, pero si quieres:
window.getDrones = loadDronesToDropdowns;