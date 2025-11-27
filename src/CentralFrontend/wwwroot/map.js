// Map.js - Real-time drone tracking on OpenStreetMap (following example pattern)
// Gijón coordinates
const GIJON_CENTER = [43.5322, -5.6611];
const DEFAULT_ZOOM = 13;

// API endpoint
const droneUri = 'http://localhost:5306/api/Drone'; // CAMBIAR IP AQUI si es necesario

let map;
let droneLayer;
let lineLayer;
let updateInterval;
let lastPositions = {};

// Drone state enum values (matching backend C# enum)
const DroneState = {
    Stopped: 0,
    Flying: 1,
    Landed: 2
};

// Initialize map when page loads
document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing drone map...');
    initMap();
    loadDrones();

    // Update drone positions every 3 seconds
    updateInterval = setInterval(() => {
        loadDrones();
    }, 3000);
});

// Clean up on page unload
window.addEventListener('beforeunload', function () {
    if (updateInterval) {
        clearInterval(updateInterval);
    }
});

/**
 * Initialize the OpenStreetMap using Leaflet (following example pattern)
 */
function initMap() {
    // Create map centered on Gijón
    map = L.map('mapid').setView(GIJON_CENTER, DEFAULT_ZOOM);

    // Add OpenStreetMap tiles (or Google Satellite like the example)
    // Using OpenStreetMap for simplicity
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 20,
        minZoom: 10
    }).addTo(map);

    // Alternative: Google Satellite (like the example)
    // L.tileLayer('http://{s}.google.com/vt/lyrs=s&x={x}&y={y}&z={z}', {
    //     maxZoom: 20,
    //     subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
    // }).addTo(map);

    // Layer to show drones
    droneLayer = L.layerGroup().addTo(map);

    // Layer to show flight paths
    lineLayer = L.layerGroup().addTo(map);

    // Add Gijón control center marker
    const gijonMarker = L.marker(GIJON_CENTER)
        .addTo(map)
        .bindPopup('<div style="text-align: center;"><strong>Gijón Control Center</strong></div>');

    // Initialize side panel
    const sidepanelLeft = L.control.sidepanel('mySidepanelLeft', {
        tabsPosition: 'left',
        startTab: 'tab-1'
    }).addTo(map);

    console.log('Map initialized successfully');
}

/**
 * Load drones from API and update markers (following example pattern)
 */
async function loadDrones() {
    try {
        const response = await fetch(droneUri);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const drones = await response.json();
        console.log(`Loaded ${drones.length} drones from API`);

        // Update drone count
        updateDroneCount(drones.length);

        // Clear current markers (like the example)
        droneLayer.clearLayers();

        // Get drone list container
        const list = document.getElementById('drone-list');
        list.replaceChildren();

        // Draw drones on the map
        drones.forEach(drone => {
            if (drone.lat && drone.lon) {
                // Draw icon over drone position
                const marker = L.marker([drone.lat, drone.lon], {
                    icon: createDroneIcon(drone.state),
                    draggable: false
                })
                    .addTo(droneLayer)
                    .bindPopup(createPopupContent(drone));

                // Add click event to center on drone
                marker.on('click', () => {
                    map.setView([drone.lat, drone.lon], 15);
                });

                // Draw line from last known position (like the example)
                if (lastPositions[drone.id] !== undefined) {
                    const pts = [
                        [lastPositions[drone.id].lat, lastPositions[drone.id].lon],
                        [drone.lat, drone.lon]
                    ];
                    const polyline = L.polyline(pts, {
                        color: "#3388ff",
                        weight: 2,
                        opacity: 0.7
                    }).addTo(lineLayer);
                }

                // Store current position
                lastPositions[drone.id] = drone;

                // Add to sidebar list
                addDroneToList(drone, list);
            }
        });

        console.log('Drones updated on map');
    } catch (error) {
        console.error('Error loading drones:', error);
        document.getElementById('drone-count').textContent = 'Error loading';
    }
}

/**
 * Create drone icon with color based on state
 */
function createDroneIcon(state) {
    const color = getDroneColor(state);
    const pulseClass = (state === DroneState.Flying) ? 'drone-marker-pulse' : '';

    return L.divIcon({
        className: 'drone-marker',
        html: `<div class="${pulseClass}" style="
            background-color: ${color}; 
       width: 28px; 
      height: 28px; 
            border-radius: 50%; 
 border: 3px solid white;
            box-shadow: 0 2px 6px rgba(0,0,0,0.4);
   display: flex;
            align-items: center;
        justify-content: center;
        font-size: 14px;
        ">
        🚁
    </div>`,
        iconSize: [28, 28],
        iconAnchor: [14, 14],
        popupAnchor: [0, -14]
    });
}

/**
 * Get drone color based on state (numeric enum)
 */
function getDroneColor(state) {
    if (state === undefined || state === null) return '#6c757d'; // Gray

    switch (state) {
        case DroneState.Stopped:
            return '#dc3545'; // Red - stopped/inactive
        case DroneState.Flying:
            return '#007bff'; // Blue - flying/active
        case DroneState.Landed:
            return '#28a745'; // Green - landed/available
        default:
            return '#6c757d'; // Gray - unknown
    }
}

/**
 * Get human-readable state text
 */
function getStateText(state) {
    if (state === undefined || state === null) return 'Unknown';

    switch (state) {
        case DroneState.Stopped:
            return 'Stopped';
        case DroneState.Flying:
            return 'Flying';
        case DroneState.Landed:
            return 'Landed';
        default:
            return 'Unknown';
    }
}

/**
 * Create popup content for drone
 */
function createPopupContent(drone) {
    const statusClass = getStatusClass(drone.state);
    const stateText = getStateText(drone.state);

    return `
        <div style="min-width: 200px;">
    <h6 style="color: #d32f2f; font-size: 16px; margin-bottom: 10px; border-bottom: 2px solid #d32f2f; padding-bottom: 5px;">
      Drone #${drone.id}
       </h6>
          <div style="font-size: 13px;">
       <div style="margin: 5px 0;">
      <strong>Status:</strong> 
      <span class="status-badge ${statusClass}">${stateText}</span>
                </div>
         <div style="margin: 5px 0;">
     <strong>Position:</strong> ${drone.lat?.toFixed(5)}, ${drone.lon?.toFixed(5)}
      </div>
          <div style="margin: 5px 0;">
         <strong>Flight Plan:</strong> ${drone.flightPlanId ? `#${drone.flightPlanId}` : 'None'}
           </div>
            </div>
        </div>
    `;
}

/**
 * Add drone to sidebar list (following example pattern)
 */
function addDroneToList(drone, list) {
    const statusClass = getStatusClass(drone.state);
    const stateText = getStateText(drone.state);

    const droneItem = document.createElement('div');
    droneItem.className = 'drone-item';
    droneItem.onclick = () => {
        map.setView([drone.lat, drone.lon], 16);
        // Find and open popup for this drone
        droneLayer.eachLayer(layer => {
            if (layer instanceof L.Marker) {
                const latLng = layer.getLatLng();
                if (latLng.lat === drone.lat && latLng.lng === drone.lon) {
                    layer.openPopup();
                }
            }
        });
    };

    droneItem.innerHTML = `
        <h5>Drone #${drone.id}</h5>
        <p><strong>Lat:</strong> ${drone.lat?.toFixed(6)}, <strong>Lon:</strong> ${drone.lon?.toFixed(6)}</p>
      <span class="status-badge ${statusClass}">${stateText}</span>
    `;

    list.appendChild(droneItem);
}

/**
 * Get status CSS class (numeric enum)
 */
function getStatusClass(state) {
    if (state === undefined || state === null) return 'status-error';

    switch (state) {
        case DroneState.Stopped:
            return 'status-error'; // Red
        case DroneState.Flying:
            return 'status-flying'; // Blue
        case DroneState.Landed:
            return 'status-idle'; // Green
        default:
            return 'status-error';
    }
}

/**
 * Update drone count display
 */
function updateDroneCount(count) {
    const countElement = document.getElementById('drone-count');
    countElement.textContent = `${count} drone${count !== 1 ? 's' : ''} active`;
}

/**
 * Center map on a specific drone
 */
function centerOnDrone(droneId) {
    const drone = Object.values(lastPositions).find(d => d.id === droneId);
    if (drone && drone.lat && drone.lon) {
        map.setView([drone.lat, drone.lon], 16);
    }
}

// Export functions for external use
window.centerOnDrone = centerOnDrone;
