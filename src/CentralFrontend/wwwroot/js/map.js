// Map.js - Real-time drone tracking on OpenStreetMap (following example pattern)
// Gijón coordinates
const GIJON_CENTER = [43.5322, -5.6611];
const DEFAULT_ZOOM = 13;

// API endpoint
import { ENDPOINTS } from './config.js';

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
document.addEventListener('DOMContentLoaded', async function () {
    console.log('Initializing drone map...');
    initMap();
  
    // Initialize SignalR for real-time updates
    console.log('[Map] Initializing SignalR connection...');
  await initializeSignalR();
    
    // Load initial drone data
    loadDrones();
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
        const response = await fetch(ENDPOINTS.DRONES);

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

/**
 * Initialize SignalR connection for real-time drone updates
 */
let signalRConnection = null;

async function initializeSignalR() {
 try {
     // Create SignalR connection
  signalRConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5306/droneHub", {
    withCredentials: true
     })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
     .configureLogging(signalR.LogLevel.Information)
   .build();

        // Set up event handlers
      signalRConnection.on("ReceiveDroneUpdate", (droneData) => {
console.log('[SignalR] Received real-time drone update:', droneData);
       // Update the drone on the map immediately
    updateSingleDroneOnMap(droneData);
        });

    signalRConnection.on("ReceiveAllDrones", (drones) => {
       console.log('[SignalR] Received all drones:', drones.length);
   // Update all drones on the map
          updateAllDronesData(drones);
   });

     signalRConnection.on("DroneAssigned", (data) => {
     console.log('[SignalR] Drone assigned:', data);
      showNotification(`Drone ${data.droneId} assigned to flight plan ${data.flightPlanId}`);
       // Reload drones after assignment
       setTimeout(() => loadDrones(), 1000);
   });

        signalRConnection.on("DroneStateChanged", (data) => {
  console.log('[SignalR] Drone state changed:', data);
  showNotification(`Drone ${data.droneId} state: ${getStateText(data.state)}`);
     });

     // Handle reconnecting
 signalRConnection.onreconnecting((error) => {
     console.warn('[SignalR] Reconnecting...', error);
       updateConnectionStatus(false, 'Reconnecting...');
 });

// Handle reconnected
   signalRConnection.onreconnected((connectionId) => {
        console.log('[SignalR] Reconnected. ConnectionId:', connectionId);
   updateConnectionStatus(true);
        loadDrones();
 });

    // Handle closed connection
        signalRConnection.onclose((error) => {
    console.error('[SignalR] Connection closed:', error);
   updateConnectionStatus(false, 'Disconnected');
        });

   // Start the connection
   await signalRConnection.start();
 console.log('[SignalR] Connected successfully. ConnectionId:', signalRConnection.connectionId);
    updateConnectionStatus(true);

  } catch (error) {
 console.error('[SignalR] Failed to initialize:', error);
     updateConnectionStatus(false, 'Connection failed');
  // Fallback to polling if SignalR fails
      console.warn('[SignalR] Falling back to polling every 3 seconds');
       updateInterval = setInterval(() => {
   loadDrones();
 }, 3000);
    }
}

/**
 * Update a single drone on the map with real-time data
 */
function updateSingleDroneOnMap(droneData) {
    console.log(`[Map] Updating drone ${droneData.id} in real-time`);
    
    // Update lastPositions
    if (lastPositions[droneData.id]) {
        // Store previous position for drawing line
const prevPosition = { ...lastPositions[droneData.id] };
    }
    
    lastPositions[droneData.id] = droneData;
    
// Redraw all drones (simplest approach)
    loadDrones();
}

/**
 * Update all drones data received from SignalR
 */
function updateAllDronesData(drones) {
    console.log(`[Map] Updating ${drones.length} drones from SignalR`);
    // This would update internal state, then redraw
   loadDrones();
}

/**
 * Update connection status indicator
 */
function updateConnectionStatus(connected, message) {
 const statusElement = document.getElementById('connection-status');
   if (!statusElement) return;

  if (connected) {
statusElement.innerHTML = '<span class="badge bg-success">🟢 Connected</span>';
    } else {
   const msg = message || 'Disconnected';
   statusElement.innerHTML = `<span class="badge bg-danger">🔴 ${msg}</span>`;
}
}

/**
 * Show a notification to the user
 */
function showNotification(message) {
  console.log('[Notification]', message);
  
    // Create a simple toast notification
    const notification = document.createElement('div');
    notification.className = 'alert alert-info alert-dismissible fade show position-fixed top-0 end-0 m-3';
    notification.style.zIndex = '9999';
    notification.innerHTML = `
     ${message}
   <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
  `;

    document.body.appendChild(notification);

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
    notification.remove();
    }, 5000);
}
