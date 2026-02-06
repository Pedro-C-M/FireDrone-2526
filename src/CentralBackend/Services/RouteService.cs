using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralBackend.Services
{
    public class RouteService
    {
        private readonly FireDrone _context;
        private readonly RedisCacheService _cache;
        private readonly ILogger<RouteService> _logger;

        // Cache configuration constants
        private const string ROUTES_CACHE_KEY = "routes:all";
        private const string ROUTE_CACHE_KEY_PREFIX = "route:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

        public RouteService(FireDrone context, RedisCacheService cache, ILogger<RouteService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// Obtiene todas las rutas con caché Redis
        public async Task<List<Models.Route>> GetAllAsync()
        {
            // Intentar obtener desde caché
            var cachedRoutes = await _cache.GetAsync<List<Models.Route>>(ROUTES_CACHE_KEY);
            if (cachedRoutes != null)
            {
                _logger.LogInformation("✅ [RouteService] Routes retrieved from Redis Cache ({Count} routes)", cachedRoutes.Count);
                Console.WriteLine($"✅ [RouteService] Routes retrieved from Redis Cache ({cachedRoutes.Count} routes)");
                return cachedRoutes;
            }

            // Si no está en caché, consultar BD
            _logger.LogInformation("⚠️ [RouteService] Cache MISS - Querying database for routes");
            Console.WriteLine("⚠️ [RouteService] Cache MISS - Querying database for routes");
            var routes = await _context.Routes
                .Include(r => r.Coords)
                .ToListAsync();

            // Guardar en caché
            if (routes.Any())
            {
                await _cache.SetAsync(ROUTES_CACHE_KEY, routes, CacheExpiration);
                _logger.LogInformation("✅ [RouteService] Routes stored in Redis Cache ({Count} routes, expires in {Minutes}min)",
                    routes.Count, CacheExpiration.TotalMinutes);
                Console.WriteLine($"✅ [RouteService] Routes stored in Redis Cache ({routes.Count} routes, expires in {CacheExpiration.TotalMinutes}min)");
            }

            return routes;
        }

        /// Elimina una ruta e invalida la caché
        public async Task<bool> DeleteAsync(int id)
        {
            // 1. Comprobar si algún Plan de Vuelo usa esta ruta
            var routeInUse = await _context.FlightPlans.AnyAsync(fp => fp.Ruta.Id == id);
            if (routeInUse)
            {
                throw new Exception("No se puede borrar la ruta porque está asignada a un Plan de Vuelo activo.");
            }

            var route = await _context.Routes
                .Include(r => r.Coords) // Traemos los puntos para asegurarnos de que se borran
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null) return false;

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            // Invalidar caché
            await InvalidateRoutesCache("Route deleted");
            _logger.LogInformation("Route {RouteId} deleted and cache invalidated", id);

            return true;
        }

        /// Importa rutas desde CSV e invalida la caché
        public async Task<int> ImportFromCsvAsync(Stream fileStream, string fileName)
        {
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Formato no válido. El archivo '{fileName}' no es un CSV.");
            }

            var puntosTemp = new Dictionary<int, Models.RoutePoint>();
            var rutasDict = new Dictionary<string, Models.Route>();
            int numeroLinea = 1; // Para decirle al usuario dónde falló

            using (var reader = new StreamReader(fileStream))
            {
                // Leer cabecera
                var header = await reader.ReadLineAsync();
                if (header == null) throw new Exception("El archivo CSV está vacío.");

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    numeroLinea++; // Empezamos contando desde la línea 2 (datos)

                    // Ignorar líneas vacías
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(';');

                    // 1. VALIDACIÓN DE COLUMNAS
                    if (values.Length < 6)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: Faltan columnas. Se esperaban 6 valores (Nombre;Tipo;Lat;Lon;Altura;Velocidad).");
                    }

                    // 2. PARSEO Y VALIDACIÓN DE TIPOS
                    string nombre = values[0].Trim();
                    if (string.IsNullOrEmpty(nombre)) throw new Exception($"Error en línea {numeroLinea}: El 'Nombre' de la ruta no puede estar vacío.");

                    // Validar Tipo (0 o 1)
                    if (!int.TryParse(values[1], out int tipoInt) || (tipoInt != 0 && tipoInt != 1))
                    {
                        throw new Exception($"Error en línea {numeroLinea}: El 'Tipo' debe ser 0 (Simple) o 1 (Periódica). Valor encontrado: '{values[1]}'");
                    }

                    // Validar Floats (Lat, Lon, Alt, Vel) con CultureInfo.InvariantCulture
                    if (!float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float lat) || lat < -90 || lat > 90)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Latitud' inválida ({values[2]}). Debe estar entre -90 y 90.");
                    }

                    if (!float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float lon) || lon < -180 || lon > 180)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Longitud' inválida ({values[3]}). Debe estar entre -180 y 180.");
                    }

                    if (!float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float altura))
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Altura' inválida ({values[4]}).");
                    }

                    if (!float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float velocidad) || velocidad < 0)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Velocidad' inválida ({values[5]}). No puede ser negativa.");
                    }

                    // 3. LOGICA DE NEGOCIO (Agrupar)
                    if (!rutasDict.ContainsKey(nombre))
                    {
                        var nuevaRuta = new Models.Route
                        {
                            // Name = nombre, // Descomenta si añadiste la propiedad Name
                            Type = (RouteType)tipoInt,
                            Coords = new List<RoutePoint>(),
                            Perimeter = new Perimeter()
                        };
                        rutasDict.Add(nombre, nuevaRuta);
                    }
                    else
                    {
                        // Validación extra: Si la ruta ya existe, ¿el tipo coincide?
                        if ((int)rutasDict[nombre].Type != tipoInt)
                        {
                            throw new Exception($"Error en línea {numeroLinea}: La ruta '{nombre}' se definió antes con otro TIPO. Todas las filas de una misma ruta deben tener el mismo tipo.");
                        }
                    }

                    rutasDict[nombre].Coords.Add(new RoutePoint
                    {
                        Lat = lat,
                        Long = lon,
                        Height = altura,
                        Velocity = velocidad,
                        Route = rutasDict[nombre]
                    });
                }
            }

            // 4. GUARDADO FINAL
            var rutasParaGuardar = rutasDict.Values.ToList();
            if (rutasParaGuardar.Any())
            {
                _context.Routes.AddRange(rutasParaGuardar);
                await _context.SaveChangesAsync();

                // Invalidar caché tras importar
                await InvalidateRoutesCache($"Imported {rutasParaGuardar.Count} routes from CSV");
                _logger.LogInformation("{Count} routes imported from CSV and cache invalidated", rutasParaGuardar.Count);
            }

            return rutasParaGuardar.Count;
        }

        public async Task<byte[]> ExportRouteToCsvAsync(int id)
        {
            var route = await _context.Routes
                .Include(r => r.Coords)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
            {
                throw new Exception($"La ruta con ID {id} no existe.");
            }

            var sb = new StringBuilder(); //Esto escribe el CSV

            sb.AppendLine("Nombre;Tipo;Lat;Lon;Altura;Velocidad");

            string routeName = $"Ruta_{route.Id}";

            // 4. Iterar sobre los puntos de ESA ruta
            foreach (var point in route.Coords)
            {
                var line = string.Format(CultureInfo.InvariantCulture, "{0};{1};{2:F6};{3:F6};{4:F2};{5:F2}",
                    routeName,           
                    (int)route.Type,                  
                    point.Lat,           
                    point.Long,          
                    point.Height,        
                    point.Velocity       
                );

                sb.AppendLine(line);
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// Invalida todas las cachés relacionadas con rutas
        private async Task InvalidateRoutesCache(string reason)
        {
            await _cache.RemoveAsync(ROUTES_CACHE_KEY);
            _logger.LogInformation("🗑️ [RouteService] Routes cache invalidated. Reason: {Reason}", reason);
            Console.WriteLine($"🗑️ [RouteService] Routes cache invalidated. Reason: {reason}");
        }
    }
}