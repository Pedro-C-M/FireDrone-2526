const uri = '/api/flightplans';
let flightplans = [];

function getFlightPlans() {
    fetch(uri)
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

    fetch(uri, {
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
    fetch(`${uri}/${id}`, {
        method: 'DELETE'
    })
        .then(() => getFlightPlans())
        .catch(error => console.error('Unable to delete flight plan.', error));
}

function displayEditForm(id) {
    const flightplan = flightplans.find(fp => fp.id === id);

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

    fetch(`${uri}/${flightplanId}`, {
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
        })
        .catch(error => console.error('Unable to update flight plan.', error));

    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'flight plan' : 'flight plans';
    document.getElementById('counter').innerText = `${itemCount} ${name}`;
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

        const editButton = clone.querySelector('.btn-warning');
        editButton.addEventListener('click', () => displayEditForm(flightplan.id));

        const deleteButton = clone.querySelector('.btn-danger');
        deleteButton.addEventListener('click', () => {
            if (confirm('Are you sure you want to delete this flight plan?')) {
                deleteFlightPlan(flightplan.id);
            }
        });

        tBody.appendChild(clone);
    });

    flightplans = data;
}

function getStatusText(state) {
    switch (state) {
        case 0: return 'On Course';
        case 1: return 'Completed';
        case 2: return 'Cancelled';
        default: return 'Unknown';
    }
}

function formatDateTime(dateTimeString) {
    const date = new Date(dateTimeString);
    return date.toLocaleString();
}

function formatDateTimeLocal(dateTimeString) {
    const date = new Date(dateTimeString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}
