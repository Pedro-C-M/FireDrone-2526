const uri = 'http://localhost:5178/api/FlightPlan'; //cambio para conexión con controller
const droneUri = 'http://localhost:5178/api/Drone';
let flightplans = [];


// Load data when page loads
window.onload = async () => {
    console.log('Page loaded, fetching data...');
    await getDrones();
  getFlightPlans();
};

async function getDrones() {
  try {
        console.log('Fetching drones from:', droneUri);
        const response = await fetch(droneUri);
        
        if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
        }
      
  const drones = await response.json();
    console.log('Drones loaded:', drones);

        const selectAssign = document.getElementById('assign-droneId');
  const selectAdd = document.getElementById('add-dronId');
        const selectEdit = document.getElementById('edit-dronId');

        // Clear existing options (except the first "Select..." option)
     [selectAssign, selectAdd, selectEdit].forEach(select => {
     if (select) {
  while (select.options.length > 1) {
            select.remove(1);
                }
  }
        });

      // Add drone options
        drones.forEach(drone => {
            const option = document.createElement('option');
      option.value = drone.id;
    option.textContent = `Drone ${drone.id} - ${drone.state || 'Unknown'}`;

            if (selectAssign) selectAssign.appendChild(option.cloneNode(true));
       if (selectAdd) selectAdd.appendChild(option.cloneNode(true));
         if (selectEdit) selectEdit.appendChild(option.cloneNode(true));
});

    console.log(`Loaded ${drones.length} drones into dropdowns`);
 } catch (error) {
  console.error('Error loading drones:', error);
 alert('Failed to load drones. Please check if the backend is running on http://localhost:5178');
    }
}

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
    console.log(`Deleting flight plan ${id}...`);

    fetch(`${uri}/${id}`, {
    method: 'DELETE',
        headers: {
    'Accept': 'application/json',
     'Content-Type': 'application/json'
        }
    })
        .then(response => {
  if (!response.ok) {
    if (response.status === 404) {
       throw new Error(`Flight plan ${id} not found`);
        } else if (response.status === 500) {
  throw new Error('Server error occurred while deleting');
             } else {
                    throw new Error(`Failed to delete flight plan: ${response.status}`);
                }
  }
            return response;
  })
        .then(() => {
    // Show success message
        alert(`Flight plan #${id} deleted successfully`);
          // Refresh the list
    getFlightPlans();
 })
        .catch(error => {
            console.error('Unable to delete flight plan.', error);
            alert(`Error deleting flight plan: ${error.message}`);
 });
}

function stopFlightPlan(id) {
    console.log(`Stopping flight plan ${id}...`);

    if (!confirm(`Are you sure you want to stop flight plan #${id}?`)) {
        return;
    }

    fetch(`${uri}/${id}/stop`, {
        method: 'PUT',
        headers: {
     'Accept': 'application/json',
       'Content-Type': 'application/json'
 }
    })
      .then(response => {
    if (!response.ok) {
 throw new Error(`Failed to stop flight plan: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
     console.log('Stop flight plan successful:', data);
    alert(`Flight plan #${id} stopped successfully!`);
 getFlightPlans(); // Refresh table
        })
        .catch(error => {
    console.error('Error stopping flight plan:', error);
            alert(`Failed to stop flight plan: ${error.message}`);
        });
}

function switchToManualMode(id) {
    console.log(`Switching flight plan ${id} to manual mode...`);

    if (!confirm(`Switch flight plan #${id} to manual mode?`)) {
        return;
    }

fetch(`${uri}/${id}/manual`, {
      method: 'PUT',
        headers: {
            'Accept': 'application/json',
        'Content-Type': 'application/json'
        }
    })
        .then(response => {
     if (!response.ok) {
    throw new Error(`Failed to switch to manual mode: ${response.status}`);
            }
  return response.json();
    })
        .then(data => {
     console.log('Switch to manual mode successful:', data);
          alert(`Flight plan #${id} switched to manual mode successfully!`);
            getFlightPlans(); // Refresh table
        })
        .catch(error => {
      console.error('Error switching to manual mode:', error);
        alert(`Failed to switch to manual mode: ${error.message}`);
        });
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
   if (confirm(`Are you sure you want to delete flight plan #${flightplan.id}? This action cannot be undone.`)) {
     deleteFlightPlan(flightplan.id);
            }
   });

        const stopButton = clone.querySelector('.btn-stop');
        if (stopButton) {
            stopButton.addEventListener('click', () => {
                if (confirm(`Are you sure you want to stop flight plan #${flightplan.id}?`)) {
                    stopFlightPlan(flightplan.id);
                }
            });
        }

        const manualButton = clone.querySelector('.btn-manual');
        if (manualButton) {
            manualButton.addEventListener('click', () => {
                if (confirm(`Switch flight plan #${flightplan.id} to manual mode?`)) {
                    switchToManualMode(flightplan.id);
                }
            });
        }

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


function assignDronToPlan() {
    console.log('=== assignDronToPlan called ===');
    
    const planId = document.getElementById('assign-planId').value;
    const droneId = document.getElementById('assign-droneId').value;

    console.log('Plan ID:', planId);
    console.log('Drone ID:', droneId);

    if (!planId || !droneId) {
        alert("Please enter both a flight plan ID and select a drone.");
        console.error('Missing planId or droneId');
      return;
    }

    const payload = { DronId: parseInt(droneId) };
    const url = `${uri}/${planId}/assign`;
    
    console.log('Making request to:', url);
    console.log('Payload:', payload);

    fetch(url, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
 'Content-Type': 'application/json'
        },
  body: JSON.stringify(payload)
    })
        .then(response => {
   console.log('Response status:', response.status);
    console.log('Response ok:', response.ok);
 
            if (!response.ok) {
      throw new Error(`Assignment failed with status: ${response.status}`);
     }
            return response.json();
     })
        .then(data => {
    console.log('Assignment successful:', data);
          alert(`Drone ${data.dronId} assigned to flight plan ${data.id} successfully!`);
      getFlightPlans(); // Refresh table
  
       // Clear the form
    document.getElementById('assign-planId').value = '';
    document.getElementById('assign-droneId').value = '';
        })
        .catch(error => {
            console.error('Error assigning drone:', error);
         alert(`Failed to assign drone: ${error.message}`);
     });
}