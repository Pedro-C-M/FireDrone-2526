// Map.js - Real-time drone tracking on OpenStreetMap (following example pattern)
// Gijón coordinates
const GIJON_CENTER = [43.5322, -5.6611];
const DEFAULT_ZOOM = 13;

// API endpoint
import { ENDPOINTS, HUB_URL } from './config.js';

let map;
let droneLayer;
let lineLayer;
let alarmLayer; // Layer for alarm markers
let updateInterval;
let lastPositions = {};
let droneMarkers = {};
let alarmMarkers = {}; // Track alarm markers

// Drone state enum values (matching backend C# enum)
const DroneState = {
    Stopped: 0,
  Flying: 1,
    Landed: 2
};

// Alarm type enum values (matching backend C# enum)
const AlarmType = {
    None: 0,
    LowBattery: 1,
    BatteryDepleted: 2
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

  // Layer to show drones
    droneLayer = L.layerGroup().addTo(map);

    // Layer to show flight paths
    lineLayer = L.layerGroup().addTo(map);

    // Layer to show alarms
    alarmLayer = L.layerGroup().addTo(map);

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


        // Get drone list container
   const list = document.getElementById('drone-list');
        list.replaceChildren();

        drones.forEach(drone => {
            updateOrCreateMarker(drone);
    addDroneToList(drone, list);
        });

        console.log('Drones updated on map');
    } catch (error) {
  console.error('Error loading drones:', error);
        document.getElementById('drone-count').textContent = 'Error loading';
    }
}

function updateOrCreateMarker(drone) {
    if (!drone.lat || !drone.lon) return;
    if (droneMarkers[drone.id]) {
        const marker = droneMarkers[drone.id];

        marker.setLatLng([drone.lat, drone.lon]);
      marker.setIcon(createDroneIcon(drone.state, drone.battery));
     marker.setPopupContent(createPopupContent(drone));

        updateTrail(drone);

    } else {
        const marker = L.marker([drone.lat, drone.lon], {
  icon: createDroneIcon(drone.state, drone.battery),
            draggable: false
        }).addTo(droneLayer);
        marker.bindPopup(createPopupContent(drone));
        marker.on('click', () => {
      map.setView([drone.lat, drone.lon], 15);
 });
        droneMarkers[drone.id] = marker;
        updateTrail(drone);
    }
  lastPositions[drone.id] = drone;
}

function updateTrail(drone) {
    if (lastPositions[drone.id] !== undefined) {
        if (lastPositions[drone.id].lat !== drone.lat || lastPositions[drone.id].lon !== drone.lon) {
       const pts = [
          [lastPositions[drone.id].lat, lastPositions[drone.id].lon],
         [drone.lat, drone.lon]
            ];
 L.polyline(pts, {
        color: "#3388ff",
 weight: 3,
       opacity: 0.8,
       dashArray: '10, 10',
      lineJoin: 'round',   
   lineCap: 'round'  
            }).addTo(lineLayer);
        }
    }
}


/**
 * Create drone icon with color based on state and battery
 */
function createDroneIcon(state, battery) {
    const color = getDroneColor(state, battery);
    const pulseClass = (state === DroneState.Flying) ? 'drone-marker-pulse' : '';
    const isLowBattery = battery !== undefined && battery !== null && (battery / 1000) * 100 <= 10;
    const alarmClass = isLowBattery ? 'drone-alarm-pulse' : '';

    return L.divIcon({
      className: 'drone-marker',
        html: `<div class="${pulseClass} ${alarmClass}" style="
        background-color: ${color}; 
        width: 28px; 
  height: 28px; 
border-radius: 50%; 
      border: 3px solid ${isLowBattery ? '#ff0000' : 'white'};
     box-shadow: 0 2px 6px rgba(0,0,0,0.4);
        display: flex;
        align-items: center;
    justify-content: center;
        font-size: 14px;">
        🚁</div>`,
        iconSize: [28, 28],
        iconAnchor: [14, 14],
        popupAnchor: [0, -14]
    });
}

/**
 * Get drone color based on state and battery (numeric enum)
 */
function getDroneColor(state, battery) {
    // Check for low battery first (critical condition)
    if (battery !== undefined && battery !== null) {
        const batteryPercent = (battery / 1000) * 100;
        if (batteryPercent <= 10) {
   return '#ff0000'; // Red - critical battery
        }
    }

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

// ...existing code for getStateText, getBatteryDisplay, getBatteryClass...

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
 * Get battery display string with icon
 * Battery values range from 0-1000, convert to percentage
 */
function getBatteryDisplay(battery) {
    if (battery === undefined || battery === null) return '❓ Unknown';
    
    const batteryPercent = Math.round((battery / 1000) * 100);
    let icon = '🔋';
 
    if (batteryPercent <= 10) {
        icon = '🪫';
    }
  
    return `${icon} ${batteryPercent}%`;
}

/**
 * Get battery CSS class based on level
 * Battery values range from 0-1000
 */
function getBatteryClass(battery) {
    if (battery === undefined || battery === null) return 'battery-unknown';
  
    const batteryPercent = (battery / 1000) * 100;
    
    if (batteryPercent <= 10) {
     return 'battery-critical';
    } else if (batteryPercent <= 25) {
        return 'battery-low';
    } else if (batteryPercent <= 50) {
      return 'battery-medium';
    } else {
        return 'battery-high';
    }
}

/**
 * Create popup content for drone
 */
function createPopupContent(drone) {
    const statusClass = getStatusClass(drone.state);
    const stateText = getStateText(drone.state);
    const batteryDisplay = getBatteryDisplay(drone.battery);
    const batteryClass = getBatteryClass(drone.battery);
    const isLowBattery = drone.battery !== undefined && drone.battery !== null && (drone.battery / 1000) * 100 <= 10;

    return `
    <div style="min-width: 200px;">
        <h6 style="color: #d32f2f; font-size: 16px; margin-bottom: 10px; border-bottom: 2px solid #d32f2f; padding-bottom: 5px;">Drone #${drone.id}</h6>
        ${isLowBattery ? '<div class="alert alert-danger" style="padding: 5px; margin-bottom: 10px; font-size: 12px;">⚠️ LOW BATTERY WARNING!</div>' : ''}
        <div style="font-size: 13px;">
   <div style="margin: 5px 0;">
        <strong>Status:</strong> 
                <span class="status-badge ${statusClass}">${stateText}</span>
   </div>
            <div style="margin: 5px 0;">
       <strong>Position:</strong> ${drone.lat?.toFixed(5)}, ${drone.lon?.toFixed(5)}
            </div>
            <div style="margin: 5px 0;">
        <strong>Battery:</strong> <span class="${batteryClass}">${batteryDisplay}</span>
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
    if (document.getElementById(`drone-item-${drone.id}`)) {
      updateDroneInSidePanel(drone);
        return;
    }

    const statusClass = getStatusClass(drone.state);
    const stateText = getStateText(drone.state);
    const batteryDisplay = getBatteryDisplay(drone.battery);
    const batteryClass = getBatteryClass(drone.battery);
    const isLowBattery = drone.battery !== undefined && drone.battery !== null && (drone.battery / 1000) * 100 <= 10;

    const droneItem = document.createElement('div');
    droneItem.className = `drone-item ${isLowBattery ? 'drone-item-alarm' : ''}`;
    droneItem.id = `drone-item-${drone.id}`;

    droneItem.onclick = () => {
        centerOnDrone(drone.id);
        if (droneMarkers[drone.id]) {
   droneMarkers[drone.id].openPopup();
   }
    };

    droneItem.innerHTML = `
        ${isLowBattery ? '<div class="alarm-badge">⚠️ ALARM</div>' : ''}
        <h5>Drone #${drone.id}</h5>
   <p><strong>Lat:</strong> ${drone.lat?.toFixed(6)}, <strong>Lon:</strong> ${drone.lon?.toFixed(6)}</p>
   <p><strong>Battery:</strong> <span class="${batteryClass}">${batteryDisplay}</span></p>
        <span class="status-badge ${statusClass}">${stateText}</span>`;

    list.appendChild(droneItem);
}

function updateDroneInSidePanel(drone) {
    const item = document.getElementById(`drone-item-${drone.id}`);

    if (!item) {
  const list = document.getElementById('drone-list');
      if (list) addDroneToList(drone, list);
   return;
    }

    const statusClass = getStatusClass(drone.state);
    const stateText = getStateText(drone.state);
    const batteryDisplay = getBatteryDisplay(drone.battery);
    const batteryClass = getBatteryClass(drone.battery);
    const isLowBattery = drone.battery !== undefined && drone.battery !== null && (drone.battery / 1000) * 100 <= 10;

    // Update class for alarm styling
 item.className = `drone-item ${isLowBattery ? 'drone-item-alarm' : ''}`;

    item.innerHTML = `
        ${isLowBattery ? '<div class="alarm-badge">⚠️ ALARM</div>' : ''}
        <h5>Drone #${drone.id}</h5>
        <div class="drone-info-content">
            <p><strong>Lat:</strong> <span class="val-lat">${drone.lat?.toFixed(6)}</span>, 
      <strong>Lon:</strong> <span class="val-lon">${drone.lon?.toFixed(6)}</span></p>
   <p><strong>Battery:</strong> <span class="val-bat ${batteryClass}">${batteryDisplay}</span></p>
          <span class="val-status status-badge ${statusClass}">${stateText}</span>
  </div>`;
}

/**
 * Get status CSS class (numeric enum)
 */
function getStatusClass(state) {
    if (state === undefined || state === null) return 'status-error';

    switch (state) {
        case DroneState.Stopped:
    return 'status-error';
  case DroneState.Flying:
            return 'status-flying';
  case DroneState.Landed:
            return 'status-idle';
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
     signalRConnection = new signalR.HubConnectionBuilder()
          .withUrl(HUB_URL, {
    withCredentials: true
      })
       .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
            .build();

        // Set up event handlers
        signalRConnection.on("ReceiveDroneUpdate", (droneData) => {
            console.log('[SignalR] Received real-time drone update:', droneData);
            updateSingleDroneOnMap(droneData);
    });

        signalRConnection.on("ReceiveAllDrones", (drones) => {
            console.log('[SignalR] Received all drones:', drones.length);
            updateAllDronesData(drones);
    });

 signalRConnection.on("DroneAssigned", (data) => {
        console.log('[SignalR] Drone assigned:', data);
     showNotification(`Drone ${data.droneId} assigned to flight plan ${data.flightPlanId}`, 'info');
   setTimeout(() => loadDrones(), 1000);
     });

     signalRConnection.on("DroneStateChanged", (data) => {
        console.log('[SignalR] Drone state changed:', data);
            showNotification(`Drone ${data.droneId} state: ${getStateText(data.state)}`, 'info');
     });

        // Handle drone alarms
        signalRConnection.on("ReceiveDroneAlarm", (alarmData) => {
            console.log('[SignalR] Received ALARM:', alarmData);
            handleDroneAlarm(alarmData);
        });

        signalRConnection.onreconnecting((error) => {
    console.warn('[SignalR] Reconnecting...', error);
    updateConnectionStatus(false, 'Reconnecting...');
        });

        signalRConnection.onreconnected((connectionId) => {
     console.log('[SignalR] Reconnected. ConnectionId:', connectionId);
            updateConnectionStatus(true);
       loadDrones();
     });

        signalRConnection.onclose((error) => {
            console.error('[SignalR] Connection closed:', error);
         updateConnectionStatus(false, 'Disconnected');
        });

        await signalRConnection.start();
console.log('[SignalR] Connected successfully. ConnectionId:', signalRConnection.connectionId);
     updateConnectionStatus(true);

    } catch (error) {
        console.error('[SignalR] Failed to initialize:', error);
        updateConnectionStatus(false, 'Connection failed');
        console.warn('[SignalR] Falling back to polling every 3 seconds');
        updateInterval = setInterval(() => {
            loadDrones();
        }, 3000);
    }
}

/**
 * Handle incoming drone alarm
 */
function handleDroneAlarm(alarmData) {
    console.log(`[Alarm] Processing alarm for Drone ${alarmData.droneId}: ${alarmData.message}`);

    // Show alarm notification
    const severity = alarmData.severity || 'warning';
    showAlarmNotification(alarmData);

    // Add alarm marker on map
    addAlarmMarker(alarmData);

    // Play alarm sound (if available)
    playAlarmSound(severity);

    // Update the alarm list in sidebar
    addAlarmToList(alarmData);

    // Center map on alarm location
    if (alarmData.lat && alarmData.lon) {
      map.setView([alarmData.lat, alarmData.lon], 15);
    }
}

/**
 * Show alarm notification with appropriate styling
 */
function showAlarmNotification(alarmData) {
    const alertClass = alarmData.severity === 'critical' ? 'alert-danger' : 'alert-warning';
    const icon = alarmData.severity === 'critical' ? '🚨' : '⚠️';
 
    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed alarm-notification`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 10000; min-width: 350px; box-shadow: 0 4px 12px rgba(0,0,0,0.3);';
    notification.innerHTML = `
        <div style="display: flex; align-items: center;">
       <span style="font-size: 24px; margin-right: 10px;">${icon}</span>
            <div>
            <strong>Drone #${alarmData.droneId} - ${alarmData.alarmTypeName}</strong><br>
      <small>${alarmData.message}</small><br>
     <small>Battery: ${alarmData.battery !== null ? Math.round((alarmData.battery / 1000) * 100) + '%' : 'N/A'}</small>
            </div>
     </div>
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    // Auto-dismiss after 10 seconds for warnings, 30 seconds for critical
    const dismissTime = alarmData.severity === 'critical' ? 30000 : 10000;
    setTimeout(() => {
     if (notification.parentNode) {
          notification.remove();
        }
 }, dismissTime);
}

/**
 * Add alarm marker on the map
 */
function addAlarmMarker(alarmData) {
    if (!alarmData.lat || !alarmData.lon) return;

    // Remove existing alarm marker for this drone if any
    if (alarmMarkers[alarmData.droneId]) {
    alarmLayer.removeLayer(alarmMarkers[alarmData.droneId]);
 }

    const icon = alarmData.severity === 'critical' ? '🚨' : '⚠️';
 const color = alarmData.severity === 'critical' ? '#dc3545' : '#ffc107';

    const alarmIcon = L.divIcon({
        className: 'alarm-marker',
        html: `<div class="alarm-marker-pulse" style="
      background-color: ${color};
            width: 40px;
            height: 40px;
        border-radius: 50%;
            border: 3px solid white;
            box-shadow: 0 0 20px ${color};
      display: flex;
         align-items: center;
            justify-content: center;
            font-size: 20px;
            animation: alarm-pulse 1s infinite;">
       ${icon}
        </div>`,
      iconSize: [40, 40],
        iconAnchor: [20, 20],
        popupAnchor: [0, -20]
    });

    const marker = L.marker([alarmData.lat, alarmData.lon], {
    icon: alarmIcon,
  zIndexOffset: 1000
    }).addTo(alarmLayer);

    marker.bindPopup(`
        <div style="min-width: 200px;">
            <h6 style="color: ${color}; font-size: 16px; margin-bottom: 10px;">
                ${icon} ALARM - Drone #${alarmData.droneId}
            </h6>
            <div style="font-size: 13px;">
         <div style="margin: 5px 0;"><strong>Type:</strong> ${alarmData.alarmTypeName}</div>
        <div style="margin: 5px 0;"><strong>Message:</strong> ${alarmData.message}</div>
            <div style="margin: 5px 0;"><strong>Battery:</strong> ${alarmData.battery !== null ? Math.round((alarmData.battery / 1000) * 100) + '%' : 'N/A'}</div>
        <div style="margin: 5px 0;"><strong>Time:</strong> ${new Date(alarmData.timestamp).toLocaleTimeString()}</div>
       </div>
       </div>
    `);

    alarmMarkers[alarmData.droneId] = marker;

    // Auto-remove alarm marker after 60 seconds
    setTimeout(() => {
        if (alarmMarkers[alarmData.droneId] === marker) {
            alarmLayer.removeLayer(marker);
        delete alarmMarkers[alarmData.droneId];
      }
    }, 60000);
}

/**
 * Add alarm to the sidebar alarm list
 */
function addAlarmToList(alarmData) {
    let alarmList = document.getElementById('alarm-list');
    
    // Create alarm list if it doesn't exist
    if (!alarmList) {
        const container = document.querySelector('.sidepanel-content') || document.body;
        const alarmSection = document.createElement('div');
        alarmSection.id = 'alarm-section';
        alarmSection.innerHTML = `
        <h5 style="color: #dc3545; margin-top: 20px;">🚨 Active Alarms</h5>
            <div id="alarm-list"></div>
        `;
        container.appendChild(alarmSection);
        alarmList = document.getElementById('alarm-list');
    }

    const icon = alarmData.severity === 'critical' ? '🚨' : '⚠️';
    const bgColor = alarmData.severity === 'critical' ? '#f8d7da' : '#fff3cd';
    const borderColor = alarmData.severity === 'critical' ? '#f5c6cb' : '#ffeeba';

    const alarmItem = document.createElement('div');
    alarmItem.className = 'alarm-item';
    alarmItem.id = `alarm-${alarmData.droneId}-${Date.now()}`;
    alarmItem.style.cssText = `
        background-color: ${bgColor};
        border: 1px solid ${borderColor};
        border-radius: 5px;
        padding: 10px;
        margin-bottom: 10px;
        cursor: pointer;
    `;
    alarmItem.onclick = () => {
      if (alarmData.lat && alarmData.lon) {
            map.setView([alarmData.lat, alarmData.lon], 16);
            if (alarmMarkers[alarmData.droneId]) {
                alarmMarkers[alarmData.droneId].openPopup();
            }
       }
    };

    alarmItem.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center;">
         <strong>${icon} Drone #${alarmData.droneId}</strong>
         <small>${new Date(alarmData.timestamp).toLocaleTimeString()}</small>
        </div>
        <div style="font-size: 12px; margin-top: 5px;">${alarmData.message}</div>
    `;

    alarmList.insertBefore(alarmItem, alarmList.firstChild);

    // Auto-remove from list after 5 minutes
    setTimeout(() => {
        if (alarmItem.parentNode) {
            alarmItem.remove();
        }
    }, 300000);
}

/**
 * Play alarm sound
 */
function playAlarmSound(severity) {
    try {
        // Create a simple beep sound using Web Audio API
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        // Different frequency for different severity
        oscillator.frequency.value = severity === 'critical' ? 800 : 400;
        oscillator.type = 'square';

        gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.5);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.5);

        // For critical alarms, play twice
        if (severity === 'critical') {
            setTimeout(() => {
                const osc2 = audioContext.createOscillator();
                const gain2 = audioContext.createGain();
                osc2.connect(gain2);
                gain2.connect(audioContext.destination);
                osc2.frequency.value = 1000;
                osc2.type = 'square';
                gain2.gain.setValueAtTime(0.3, audioContext.currentTime);
                gain2.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.5);
                osc2.start(audioContext.currentTime);
                osc2.stop(audioContext.currentTime + 0.5);
            }, 600);
        }
    } catch (error) {
        console.log('[Alarm] Could not play alarm sound:', error.message);
    }
}

/**
 * Update a single drone on the map with real-time data
 */
function updateSingleDroneOnMap(droneData) {
    console.log(`[Map] Updating drone ${droneData.id} in real-time`);
    updateOrCreateMarker(droneData);
    updateDroneInSidePanel(droneData);
}

/**
 * Update all drones data received from SignalR
 */
function updateAllDronesData(drones) {
    console.log(`[Map] Updating ${drones.length} drones from SignalR`);
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
function showNotification(message, type = 'info') {
  console.log('[Notification]', message);

    const alertClass = type === 'danger' ? 'alert-danger' : 
 type === 'warning' ? 'alert-warning' : 'alert-info';

    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed top-0 end-0 m-3`;
    notification.style.zIndex = '9999';
 notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 5000);
}
