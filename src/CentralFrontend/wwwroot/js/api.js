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
        window.location.href = '/index.html';
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

// === LÓGICA DE ACTUALIZACIÓN Y EDICIÓN ===
function displayEditForm(id) {
    console.log('displayEditForm called with id:', id);

    const flightplan = flightplans.find(fp => fp.id === id);
    if (!flightplan) {
        console.error('Flight plan not found:', id);
        return;
    }

    console.log('Flight plan found:', flightplan);
    // Set form values with null checks
    const editId = document.getElementById('edit-id');
    const editDronId = document.getElementById('edit-dronId');
    const editRutaId = document.getElementById('edit-rutaId');
    const editState = document.getElementById('edit-state');
    const editForm = document.getElementById('editForm');

    if (editId) editId.value = flightplan.id;
    if (editDronId) editDronId.value = flightplan.dronId;
    if (editRutaId) editRutaId.value = flightplan.rutaId;
    if (editState) editState.value = flightplan.state;

    if (editForm) {
        editForm.style.display = 'block';
        // Scroll to the form
        editForm.scrollIntoView({ behavior: 'smooth' });
    } else {
        console.error('Edit form not found');
    }
}

async function updateFlightPlan() {
    const flightplanId = parseInt(document.getElementById('edit-id').value);
    // Get the original flight plan to preserve unchanged fields
    const originalPlan = flightplans.find(fp => fp.id === flightplanId);
    if (!originalPlan) {
        console.error('Original flight plan not found');
        return false;
    }
    const flightplanData = {
        id: flightplanId,
        dronId: parseInt(document.getElementById('edit-dronId').value),
        rutaId: originalPlan.rutaId,
        estControlId: originalPlan.estControlId,
        startingTime: originalPlan.startingTime,
        endingTime: originalPlan.endingTime,
        state: parseInt(document.getElementById('edit-state').value)
    };

    console.log('Updating flight plan with data:', flightplanData);

    try {
        await FlightPlanService.updateFlightPlan(flightplanId, flightplanData);
        alert('Flight plan updated successfully!');
        await getFlightPlans();
        closeInput();
        await loadDronesToDropdowns();
    } catch (error) {
        console.error('Unable to update flight plan.', error);
        alert(`Error updating flight plan: ${error.message}`);
    }
    return false;
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

function closeInput() {
    const form = document.getElementById('editForm');
    if (form) form.style.display = 'none';
}

// === Start Route Functions ===
async function startFlightPlanRoute(planId, droneId, restartFromBeginning = false) {
    if (!droneId) {
        alert(`Flight plan #${planId} has no drone assigned. Please assign a drone first.`);
        return;
    }

    const action = restartFromBeginning ? "restart from beginning" : "resume from last position";

    if (!confirm(`Start route for Flight Plan #${planId} with Drone #${droneId}?\nMode: ${action}`)) {
        return;
    }

    try {
        const result = await FlightPlanService.assignDrone(planId, droneId, restartFromBeginning);
        alert(`Flight Plan #${result.id} started successfully!\nDrone: #${result.dronId}\nMode: ${action}`);
        await getFlightPlans();
    } catch (error) {
        console.error('Error starting flight plan:', error);
        alert(`Failed to start flight plan: ${error.message}`);
    }
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
        td[1].textContent = flightplan.dronId || 'Not assigned';

        // Display route information with type and waypoint count
        const routeInfo = flightplan.ruta || flightplan.Ruta;
        if (routeInfo) {
            const routeType = routeInfo.type === 1 || routeInfo.Type === 1 ? 'Periodic' : 'Simple';
            const waypointCount = routeInfo.coords?.length || routeInfo.Coords?.length || 0;
            const routeTypeBadge = routeType === 'Periodic' ? 'badge-periodic' : 'badge-simple';

            td[2].innerHTML = 
            `<div>
                <strong>Route #${flightplan.rutaId}</strong>
                <br><span class="badge ${routeTypeBadge} mt-1">${routeType}</span>
                <span class="badge bg-secondary mt-1">${waypointCount} waypoints</span>
            </div>`;
        } else {
            td[2].textContent = `Route #${flightplan.rutaId}`;
        }

        td[3].textContent = flightplan.estControlId || 'N/A';
        td[4].textContent = formatDateTime(flightplan.startingTime);
        td[5].textContent = flightplan.endingTime ? formatDateTime(flightplan.endingTime) : 'In Progress';

        // Badge de estado
        const badgeSpan = document.createElement('span');
        badgeSpan.className = `status-badge ${getStatusClass(flightplan.state)}`;
        badgeSpan.textContent = getStatusText(flightplan.state);
        td[6].innerHTML = '';
        td[6].appendChild(badgeSpan);

        //Fila manual
        const manualRow = document.createElement('tr');
        manualRow.style.display = 'none';

        manualRow.innerHTML = `
        <td colspan="8">
            <div class="card-box mt-2">
                    <strong>Manual destination for FlightPlan #${flightplan.id}</strong>
                <div class="d-flex gap-2 mt-2">
                    <input type="number" step="any" class="form-control manual-y" placeholder="Latitude">
                    <input type="number" step="any" class="form-control manual-x" placeholder="Longitude">
                    <button class="btn btn-primary btn-sm send-manual">Send</button>
                    <button class="btn btn-secondary btn-sm cancel-manual">Cancel</button>
                </div>
            </div>
        </td>`;

        // === ZONA DE BOTONES SEGURA ===

        // Función auxiliar: Intenta buscar el botón y añadir el evento.
        const safeAddClick = (selector, action) => {
            const btn = clone.querySelector(selector);
            if (btn) {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    action();
                });
            }
        };

        // Resume button - starts route from last position
        const resumeBtn = clone.querySelector('.btn-resume');
        if (resumeBtn) {
            if (flightplan.dronId) {
                // Flight plan has a drone assigned
                if (flightplan.state !== 0) {
                    // Not currently flying - can resume
                    resumeBtn.addEventListener('click', (e) => {
                        e.preventDefault();
                        startFlightPlanRoute(flightplan.id, flightplan.dronId, false);
                    });
                } else {
                    // Already flying
                    resumeBtn.disabled = true;
                    resumeBtn.style.opacity = '0.5';
                    resumeBtn.style.cursor = 'not-allowed';
                    resumeBtn.title = 'Flight is already in progress';
                }
            } else {
                // No drone assigned
                resumeBtn.disabled = true;
                resumeBtn.style.opacity = '0.5';
                resumeBtn.style.cursor = 'not-allowed';
                resumeBtn.title = 'No drone assigned to this flight plan';
            }
        }

        // Restart button - starts route from beginning
        const restartBtn = clone.querySelector('.btn-restart');
        if (restartBtn) {
            if (flightplan.dronId) {
                // Flight plan has a drone assigned
                if (flightplan.state !== 0) {
                    // Not currently flying - can restart
                    restartBtn.addEventListener('click', (e) => {
                        e.preventDefault();
                        startFlightPlanRoute(flightplan.id, flightplan.dronId, true);
                    });
                } else {
                    // Already flying
                    restartBtn.disabled = true;
                    restartBtn.style.opacity = '0.5';
                    restartBtn.style.cursor = 'not-allowed';
                    restartBtn.title = 'Flight is already in progress';
                }
            } else {
                // No drone assigned
                restartBtn.disabled = true;
                restartBtn.style.opacity = '0.5';
                restartBtn.style.cursor = 'not-allowed';
                restartBtn.title = 'No drone assigned to this flight plan';
            }
        }

        // Edit button
        safeAddClick('.btn-edit', () => displayEditForm(flightplan.id));

        // Delete button
        const deleteBtn = clone.querySelector('.btn-delete');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', async (e) => {
                e.preventDefault();
                e.stopPropagation();

                if (!confirm(`Are you sure you want to delete flight plan #${flightplan.id}?`)) return;

                try {
                    await FlightPlanService.deleteFlightPlan(flightplan.id);
                    alert(`Flight plan #${flightplan.id} deleted successfully`);
                    await getFlightPlans();
                } catch (error) {
                    console.error('Unable to delete flight plan.', error);
                    alert(`Error deleting flight plan: ${error.message}`);
                }
            });
        }

        // Manual button
        const manualBtn = clone.querySelector('.btn-manual');
        if (manualBtn) {
            if (flightplan.state === 0) {
                manualBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    alert(`Manual mode selected for FlightPlan #${flightplan.id}.\n\n` +
                        `The current route will be paused.\n` +
                        `Enter coordinates and press SEND to confirm the change.`);
                    manualRow.style.display =
                        manualRow.style.display === 'none' ? 'table-row' : 'none';
                });
            } else {
                manualBtn.disabled = true;
                manualBtn.style.opacity = '0.5';
                manualBtn.style.cursor = 'not-allowed';
                manualBtn.title = 'Manual mode only available for flights in progress';
            }
        }

        // Stop button
        const stopBtn = clone.querySelector('.btn-stop');
        if (stopBtn) {
            if (flightplan.state === 0) {
                stopBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    stopFlightPlan(flightplan.id);
                });
            } else {
                stopBtn.disabled = true;
                stopBtn.style.opacity = '0.5';
                stopBtn.style.cursor = 'not-allowed';
                stopBtn.title = 'Flight is not in progress';
            }
        }

        tBody.appendChild(clone);
        tBody.appendChild(manualRow);

        // ===== EVENTOS PANEL MANUAL =====
        const sendBtn = manualRow.querySelector('.send-manual');
        const cancelBtn = manualRow.querySelector('.cancel-manual');

        sendBtn.addEventListener('click', async () => {
            // 1. Obtenemos el VALOR EN TEXTO primero
            let lonStr = manualRow.querySelector('.manual-x').value;
            let latStr = manualRow.querySelector('.manual-y').value;

            // 2. TRUCO DE SEGURIDAD: Reemplazar coma por punto
            lonStr = lonStr.replace(',', '.');
            latStr = latStr.replace(',', '.');

            // 3. Convertimos a número
            const longitude = parseFloat(lonStr);
            const latitude = parseFloat(latStr);

            // 4. VALIDACIÓN ESTRICTA
            if (isNaN(longitude) || isNaN(latitude) ||
                Math.abs(latitude) > 90 || Math.abs(longitude) > 180) {

                alert(`Bad coordinates.\n\n` +
                    `Make sure to use decimal format (ej: 43.54).\n` +
                    `Valid range:\nLat: -90 a 90\nLon: -180 a 180`);
                return; // Detiene todo
            }

            try {
                console.log(`Switching to manual mode for plan ${flightplan.id} with coords: ${latitude}, ${longitude}`);

                await FlightPlanService.setManualMode(flightplan.id);
                await FlightPlanService.sendGotoCommand(flightplan.id, latitude, longitude);

                alert(`FlightPlan #${flightplan.id} is now in MANUAL mode. Drone heading to coordinates.`);
                manualRow.style.display = 'none';
                await getFlightPlans();

            } catch (error) {
                console.error('Error activating manual mode:', error);
                alert(`Failed to activate manual mode: ${error.message}`);
            }
        });
        cancelBtn.addEventListener('click', () => {
            manualRow.style.display = 'none';
        });
    });

    flightplans = data;
}

function getStatusClass(state) {
    switch (state) {
        case 0: return 'bg-primary text-white';   // OnCourse
        case 1: return 'bg-success text-white';   // Completed
        case 2: return 'bg-danger text-white';    // Cancelled
        case 3: return 'bg-warning text-dark';    // Manual
        default: return 'bg-secondary text-white';
    }
}

function getStatusText(state) {
    const s = ['On Course', 'Completed', 'Cancelled', 'Manual'];
    return s[state] || 'Unknown';
}
// === EXPOSICIÓN GLOBAL ===
window.addFlightPlan = addFlightPlan;
window.updateFlightPlan = updateFlightPlan;
window.startFlightPlanRoute = startFlightPlanRoute;
window.closeInput = closeInput;
window.getDrones = loadDronesToDropdowns;
window.getFlightPlans = getFlightPlans;
window.displayEditForm = displayEditForm;