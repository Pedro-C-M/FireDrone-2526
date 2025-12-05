import * as DroneService from './services/DroneService.js';
import * as FlightPlanService from './services/FlightPlanService.js';

// Variable de estado local
let flightplans = [];

// === INICIALIZACIÓN ===
document.addEventListener('DOMContentLoaded', async () => {
    console.log('Page loaded, fetching data...');
    // Carga inicial en paralelo para ser más rápidos
    await Promise.all([
        loadDronesToDropdowns(),
        getFlightPlans()
    ]);
});

// === LÓGICA DE UI PARA DRONES ===
async function loadDronesToDropdowns() {
    try {
        const drones = await DroneService.getAllDrones();
        console.log('Drones loaded via Service:', drones);

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
        alert('Failed to load drones.');
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

        // Refrescar UI
        await getFlightPlans();
        await loadDronesToDropdowns();

        // Limpiar formulario
        addDronIdInput.value = '';
        addRutaIdInput.value = '';
        addEstControlIdInput.value = '';
        addStartingPointIdInput.value = '';
        addStartingTimeInput.value = '';
        addStateInput.value = '0';
    } catch (error) {
        console.error('Unable to add flight plan.', error);
        alert(`Error adding plan: ${error.message}`);
    }
}

async function deleteFlightPlan(id) {
    if (!confirm(`Are you sure you want to delete flight plan #${id}?`)) return;

    try {
        console.log(`Deleting flight plan ${id}...`);
        await FlightPlanService.deleteFlightPlan(id);

        alert(`Flight plan #${id} deleted successfully`);
        await getFlightPlans();
    } catch (error) {
        console.error('Unable to delete flight plan.', error);
        alert(`Error deleting flight plan: ${error.message}`);
    }
}

async function stopFlightPlan(id) {
    if (!confirm(`Are you sure you want to stop flight plan #${id}?`)) return;

    try {
        console.log(`Stopping flight plan ${id}...`);
        await FlightPlanService.stopFlightPlan(id);

        alert(`Flight plan #${id} stopped successfully!`);
        await getFlightPlans();
    } catch (error) {
        console.error('Error stopping flight plan:', error);
        alert(`Failed to stop: ${error.message}`);
    }
}

async function switchToManualMode(id) {
    if (!confirm(`Switch flight plan #${id} to manual mode?`)) return;

    try {
        console.log(`Switching plan ${id} to manual...`);
        await FlightPlanService.setManualMode(id);

        alert(`Flight plan #${id} switched to manual mode!`);
        await getFlightPlans();
    } catch (error) {
        console.error('Error switching manual mode:', error);
        alert(`Failed to switch: ${error.message}`);
    }
}

// === LÓGICA DE ACTUALIZACIÓN Y EDICIÓN ===

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

async function updateFlightPlan() {
    const flightplanId = parseInt(document.getElementById('edit-id').value);
    const flightplanData = {
        id: flightplanId,
        dronId: parseInt(document.getElementById('edit-dronId').value.trim()),
        rutaId: parseInt(document.getElementById('edit-rutaId').value.trim()),
        estControlId: document.getElementById('edit-estControlId').value ? parseInt(document.getElementById('edit-estControlId').value.trim()) : null,
        startingPointId: document.getElementById('edit-startingPointId').value ? parseInt(document.getElementById('edit-startingPointId').value.trim()) : null,
        startingTime: document.getElementById('edit-startingTime').value,
        endingTime: document.getElementById('edit-endingTime').value || null,
        state: parseInt(document.getElementById('edit-state').value)
    };

    try {
        await FlightPlanService.updateFlightPlan(flightplanId, flightplanData);

        await getFlightPlans();
        closeInput();
        await loadDronesToDropdowns();
    } catch (error) {
        console.error('Unable to update flight plan.', error);
        alert(`Error updating plan: ${error.message}`);
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

        document.getElementById('assign-planId').value = '';
        document.getElementById('assign-droneId').value = '';
    } catch (error) {
        console.error('Error assigning drone:', error);
        alert(`Failed to assign drone: ${error.message}`);
    }
}

// === UTILIDADES UI ===
function closeInput() { document.getElementById('editForm').style.display = 'none'; }
function formatDateTime(str) { return new Date(str).toLocaleString(); }
function formatDateTimeLocal(str) {
    if (!str) return '';
    const d = new Date(str);
    const pad = n => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
function getStatusText(state) {
    const s = ['On Course', 'Completed', 'Cancelled'];
    return s[state] || 'Unknown';
}

function _displayCount(count) {
    document.getElementById('counter').innerText = `${count} flight plans`;
}

function _displayFlightPlans(data) {
    const tBody = document.getElementById('flightplans_tbody');
    tBody.innerHTML = '';
    _displayCount(data.length);
    const template = document.getElementById('flightplan_row');

    data.forEach(flightplan => {
        const clone = template.content.cloneNode(true);
        const td = clone.querySelectorAll('td');

        td[0].textContent = flightplan.id;
        td[1].textContent = flightplan.dronId;
        td[2].textContent = flightplan.rutaId;
        td[3].textContent = flightplan.estControlId || 'N/A';
        td[4].textContent = formatDateTime(flightplan.startingTime);
        td[5].textContent = flightplan.endingTime ? formatDateTime(flightplan.endingTime) : 'In Progress';
        td[6].textContent = getStatusText(flightplan.state);

        // Eventos de botones
        clone.querySelector('.btn-warning').onclick = () => displayEditForm(flightplan.id);
        clone.querySelector('.btn-danger').onclick = () => deleteFlightPlan(flightplan.id);

        const stopBtn = clone.querySelector('.btn-stop');
        if (stopBtn) stopBtn.onclick = () => stopFlightPlan(flightplan.id);

        const manualBtn = clone.querySelector('.btn-manual');
        if (manualBtn) manualBtn.onclick = () => switchToManualMode(flightplan.id);

        tBody.appendChild(clone);
    });
    flightplans = data;
}

// === EXPOSICIÓN GLOBAL (Necesaria para los onsubmit del HTML) ===
window.addFlightPlan = addFlightPlan;
window.updateFlightPlan = updateFlightPlan;
window.assignDronToPlan = assignDronToPlan;
window.closeInput = closeInput;
window.getDrones = loadDronesToDropdowns; // Por si acaso