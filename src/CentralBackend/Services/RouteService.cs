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
            if (routes.Count != 0)
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
            var routeInUse = await _context.FlightPlans.AnyAsync(fp => fp.Ruta != null && fp.Ruta.Id == id);
            if (routeInUse)
            {
                throw new ArgumentException("No se puede borrar la ruta porque está asignada a un Plan de Vuelo activo.");
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
                throw new ArgumentException($"Formato no valido. El archivo '{fileName}' no es un CSV.");

            var rutasDict = new Dictionary<string, Models.Route>();
            int numeroLinea = 1; // Para decirle al usuario dónde falló

            using (var reader = new StreamReader(fileStream))
            {
                // Leer cabecera
                var header = await reader.ReadLineAsync();
                if (header == null) throw new ArgumentException("El archivo CSV esta vacio.");

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    numeroLinea++; // Empezamos contando desde la línea 2 (datos)
                    if (string.IsNullOrWhiteSpace(line)) continue;// Ignorar líneas vacías
                    // 1. Extraemos validación y parseo
                    var data = ParseCsvLine(line, numeroLinea);
                    // 2. Extraemos la lógica de negocio a otro método
                    ProcessRouteData(rutasDict, data, numeroLinea);
                }
            }

            if (numeroLinea <= 1) throw new InvalidOperationException("No hay puntos en la ruta.");

            // 3. Extraemos el guardado final
            return await SaveImportedRoutesAsync(rutasDict.Values.ToList());

        }
        /**
         * Metodo auxiliar para parsear y validar una línea del CSV, 
         * devolviendo una tupla con los datos ya convertidos.
         * Ayuda en la refactorizacion de ImportFromCsvAsync 
         */
        private static  (string Nombre, int Tipo, float Lat, float Lon, float Alt, float Vel)  ParseCsvLine(string line, int lineNum)
        {
            var values = line.Split(';');
            if (values.Length != 6)
                throw new ArgumentException($"Error en linea {lineNum}: Faltan columnas. Se esperaban 6 valores.");

            string nombre = values[0].Trim();
            if (string.IsNullOrEmpty(nombre))
                throw new ArgumentException($"Error en linea {lineNum}: El 'Nombre' no puede estar vacio.");

            if (!int.TryParse(values[1], out int tipo) || (tipo != 0 && tipo != 1))
                throw new ArgumentException($"Error en linea {lineNum}: El 'Tipo' debe ser 0 o 1.");

            // Usamos un helper para no repetir el TryParse 4 veces
            float lat = ParseFloat(values[2], -90, 90, "Latitud", lineNum);
            float lon = ParseFloat(values[3], -180, 180, "Longitud", lineNum);
            float alt = ParseFloat(values[4], 0, float.MaxValue, "Altura", lineNum);
            float vel = ParseFloat(values[5], 0, float.MaxValue, "Velocidad", lineNum);

            return (nombre, tipo, lat, lon, alt, vel);
        }

        /**
         * Metodo helper para procesar cada línea del CSV 
         * y agregar los puntos a las rutas correspondientes en el diccionario.
         * Ayuda en la refactorizacion de ImportFromCsvAsync 
         */
        private void ProcessRouteData(Dictionary<string, Models.Route> rutasDict,
        (string Nombre, int Tipo, float Lat, float Lon, float Alt, float Vel) data, int lineNum)
        {
            if (!rutasDict.TryGetValue(data.Nombre, out var rutaExistente))
            {
                rutaExistente = new Models.Route
                {
                    Type = (RouteType)data.Tipo,
                    Coords = new List<RoutePoint>(),
                    Perimeter = new Perimeter()
                };
                rutasDict.Add(data.Nombre, rutaExistente);
            }
            else if ((int)rutaExistente.Type != data.Tipo)
            {
                throw new InvalidOperationException($"Error en linea {lineNum}: La ruta '{data.Nombre}' se definio antes con otro TIPO.");
            }

            rutaExistente.Coords.Add(new RoutePoint
            {
                Lat = data.Lat,
                Long = data.Lon,
                Height = data.Alt,
                Velocity = data.Vel,
                Route = rutaExistente
            });
        }
        /**
         * Metodo helper para guardar rutas en la base de datos e invalidar caché después.
         * Ayuda en la refactorizacion de ImportFromCsvAsync 
         */
        private async Task<int> SaveImportedRoutesAsync(List<Models.Route> rutasParaGuardar)
        {
            if (!rutasParaGuardar.Any()) return 0;

            _context.Routes.AddRange(rutasParaGuardar);
            await _context.SaveChangesAsync();

            await InvalidateRoutesCache($"Imported {rutasParaGuardar.Count} routes from CSV");
            _logger.LogInformation("{Count} routes imported from CSV and cache invalidated", rutasParaGuardar.Count);

            return rutasParaGuardar.Count;
        }

        /**
         * Método helper para parsear y validar un float con un rango específico. 
         * Lanza una excepción con mensaje claro si el valor no es válido.
         * Ayuda en la refactorizacion de ImportFromCsvAsync 
         */
        private static float ParseFloat(string value, float min, float max, string fieldName, int lineNum)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) || result < min || result > max)
                throw new ArgumentException($"Error en linea {lineNum}: '{fieldName}' invalida ({value}). Rango permitido: [{min}, {max}].");

            return result;
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