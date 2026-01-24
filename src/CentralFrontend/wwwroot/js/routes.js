// js/routes.js
import { ENDPOINTS } from './config.js';
import { getAllRoutes, importRoutes, deleteRoute } from './services/RouteService.js';

let map;
let currentLayer = null;

document.addEventListener('DOMContentLoaded', () => {
    initMap();
    loadRoutesData();
    setupImportHandler();
});

function initMap() {
    // Coordenadas iniciales (Gijón)
    map = L.map('map').setView([43.5322, -5.6611], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);
}

// Función para cargar datos usando el Servicio
async function loadRoutesData() {
    const tbody = document.getElementById('routesTableBody');
    tbody.innerHTML = '<tr><td colspan="3" class="text-center">Loading...</td></tr>';

    try {
        // LLAMADA AL SERVICIO
        const routes = await getAllRoutes();
        renderTable(routes);
    } catch (error) {
        console.error("Error cargando rutas:", error);
        tbody.innerHTML = '<tr><td colspan="3" class="text-center text-danger">Error loading routes (Check console)</td></tr>';
    }
}

function renderTable(routes) {
    const tbody = document.getElementById('routesTableBody');
    tbody.innerHTML = '';

    if (!routes || routes.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No routes found</td></tr>';
        return;
    }

    routes.forEach(route => {
        const tr = document.createElement('tr');
        tr.className = 'route-row';

        // Evento para seleccionar la ruta al hacer clic en la fila
        tr.onclick = () => selectRoute(route, tr);

        const isPeriodic = route.type === 1 || route.type === 'Periodic';
        const badgeClass = isPeriodic ? 'badge-periodic' : 'badge-simple';
        const typeName = isPeriodic ? 'Periodic' : 'Simple';
        const name = route.id ? `Route #${route.id}` : 'Unnamed Route'; // O route.name si lo tienes

        // Añadimos el botón de borrar y exportar 
        tr.innerHTML = `
            <td class="fw-bold">${name}</td>
            <td><span class="badge ${badgeClass}">${typeName}</span></td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-success border me-1 btn-export" title="Download CSV">⬇️</button>
                <button class="btn btn-sm btn-light border me-1 btn-view" title="Ver">👁️</button> 
                <button class="btn btn-sm btn-outline-danger btn-delete" title="Borrar">🗑️</button>
            </td>
        `;

        //Lógica del botón EXPORTAR
        const exportBtn = tr.querySelector('.btn-export');
        exportBtn.onclick = (e) => {
            e.stopPropagation();//Que no se pinte la ruta al hacer clic en el botón
            const exportUrl = `${ENDPOINTS.ROUTES}/export/${route.id}`;
            // Forzamos al navegador a ir a esa URL, lo que iniciará la descarga automáticamente
            window.location.href = exportUrl;
        };

        // Lógica del botón Borrar
        const deleteBtn = tr.querySelector('.btn-delete');
        deleteBtn.onclick = async (e) => {
            // "stopPropagation" evita que al borrar se seleccione la ruta en el mapa (el evento del tr)
            e.stopPropagation();

            if (confirm(`Are you sure you want to delete route #${route.id}?`)) {
                try {
                    await deleteRoute(route.id);
                    // Si todo va bien, recargamos la tabla y limpiamos el mapa
                    loadRoutesData();
                    if (currentLayer) map.removeLayer(currentLayer);
                    document.getElementById('mapTitle').textContent = "Select a route to visualize";
                } catch (error) {
                    alert("Error deleting route: " + error.message);
                }
            }
        };

        tbody.appendChild(tr);
    });
}

function selectRoute(route, rowElement) {
    // 1. Resaltar visualmente en la tabla
    document.querySelectorAll('.route-row').forEach(r => r.classList.remove('selected-row'));
    rowElement.classList.add('selected-row');

    // 2. Actualizar textos
    const isPeriodic = route.type === 1 || route.type === 'Periodic';
    const infoBadge = document.getElementById('routeInfo');

    document.getElementById('mapTitle').textContent = `Route #${route.id}`;
    if (infoBadge) {
        infoBadge.textContent = isPeriodic ? "Periodic Cycle" : "Simple Path";
        infoBadge.className = `badge ${isPeriodic ? 'badge-periodic' : 'badge-simple'}`;
    }

    // 3. Limpiar capa anterior (Borra todo lo que esté en el grupo)
    if (currentLayer) {
        map.removeLayer(currentLayer);
    }

    // Mapeo seguro de coordenadas
    const points = route.coords || route.Coords || [];

    if (points.length === 0) {
        alert("Esta ruta no tiene puntos definidos.");
        return;
    }

    // Convertir a [lat, lng] para Leaflet
    const latlngs = points.map(p => [p.lat || p.Lat, p.long || p.Long]);
    const color = isPeriodic ? '#6610f2' : '#0d6efd';

    // Creamos un grupo para guardar todas las líneas de esta ruta (sea 1 o 2)
    const routeGroup = L.featureGroup();

    // 1. Añadimos la línea principal al grupo
    L.polyline(latlngs, { color: color, weight: 4 }).addTo(routeGroup);

    // 2. Si es periódica, añadimos la línea discontinua al MISMO grupo
    if (isPeriodic && latlngs.length > 1) {
        const closingLine = [latlngs[latlngs.length - 1], latlngs[0]];
        L.polyline(closingLine, { color: color, weight: 2, dashArray: '5, 10' }).addTo(routeGroup);
    }

    // Añadimos el grupo entero al mapa y lo guardamos en currentLayer
    routeGroup.addTo(map);
    currentLayer = routeGroup;

    // Centrar mapa usando los límites del grupo
    map.fitBounds(routeGroup.getBounds());
}

function setupImportHandler() {
    const form = document.getElementById('importForm');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const fileInput = document.getElementById('csvFile');
        const file = fileInput.files[0];

        if (!file) {
            alert("Por favor selecciona un archivo.");
            return;
        }
        if (!file.name.toLowerCase().endsWith('.csv')) {
            alert("❌ Error: Solo se admiten archivos con extensión .csv");

            // Limpiamos el input para obligar a elegir otro
            fileInput.value = '';
            return;
        }
        // Crear FormData para enviar el archivo
        const formData = new FormData();
        formData.append('file', file);

        const btn = form.querySelector('button[type="submit"]');
        const originalText = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = "Uploading...";

        try {
            await importRoutes(formData);

            alert("Rutas importadas correctamente!");
            form.reset();
            loadRoutesData();
        } catch (err) {
            console.error(err);
            alert("Error al importar: " + err.message);
        } finally {
            btn.disabled = false;
            btn.innerHTML = originalText;
        }
    });
}