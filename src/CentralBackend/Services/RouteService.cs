using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Text.Json;

namespace CentralBackend.Services
{
    public class RouteService
    {
        private readonly FireDrone _context; // Tu DbContext
        private readonly IDatabase _cache;

        private const string ROUTES_CACHE_KEY = "routes_all";

        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

        public RouteService(FireDrone context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        public async Task<List<Models.Route>> GetAllAsync()
        {
            /*
            // Es vital usar .Include para traer las coordenadas al frontend
            return await _context.Routes
                .Include(r => r.Coords)
                .ToListAsync();
            */

            /*PRUEBA DE REDIS DE AQUÍ HACIA ABAJO EN EL MÉTODO*/

            //Se intenta obtener de Redis
            var cachedRoutes = await _cache.StringGetAsync(ROUTES_CACHE_KEY);

            if (!cachedRoutes.IsNullOrEmpty)
            {
                Console.WriteLine("[RouteService] Returning routes from Redis cache");
                //return JsonSerializer.Deserialize<List<Models.Route>>(cachedRoutes)!;
                return JsonSerializer.Deserialize<List<Models.Route>>(cachedRoutes!, _jsonOptions)!;
            }

            //Si no se puede, se obtiene de la BD
            Console.WriteLine("[RouteService] Cache miss. Loading routes from database");

            var routes = await _context.Routes
                .Include(r => r.Coords)
                .ToListAsync();

            //Y se guardan en Redis (5 minutos)
            await _cache.StringSetAsync(
                ROUTES_CACHE_KEY,
                //JsonSerializer.Serialize(routes),
                JsonSerializer.Serialize(routes, _jsonOptions),
                TimeSpan.FromMinutes(5)
            );

            return routes;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // 1. Comprobar si algún Plan de Vuelo usa esta ruta
            var routeInUse = await _context.FlightPlans.AnyAsync(fp => fp.Ruta.Id == id);
            if (routeInUse)
            {
                // Opcional: Podrías lanzar una excepción personalizada aquí
                throw new Exception("No se puede borrar la ruta porque está asignada a un Plan de Vuelo activo.");
            }

            var route = await _context.Routes
                .Include(r => r.Coords) // Traemos los puntos para asegurarnos de que se borran
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null) return false;

            /*PRUEBA DE REDIS DE AQUÍ HACIA ABAJO EN EL MÉTODO*/

            /* 
            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return true;
            */

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            //Se invalida caché
            await _cache.KeyDeleteAsync(ROUTES_CACHE_KEY);

            Console.WriteLine("[RouteService] Routes cache invalidated after delete");

            return true;


        }
        public async Task<int> ImportFromCsvAsync(Stream fileStream, string fileName)
        {
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Formato no válido. El archivo '{fileName}' no es un CSV.");
            }

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
                    if (values.Length < 7)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: Faltan columnas. Se esperaban 7 valores (Nombre;Tipo;Orden;Lat;Lon;Altura;Velocidad).");
                    }

                    // 2. PARSEO Y VALIDACIÓN DE TIPOS
                    string nombre = values[0].Trim();
                    if (string.IsNullOrEmpty(nombre)) throw new Exception($"Error en línea {numeroLinea}: El 'Nombre' de la ruta no puede estar vacío.");

                    // Validar Tipo (0 o 1)
                    if (!int.TryParse(values[1], out int tipoInt) || (tipoInt != 0 && tipoInt != 1))
                    {
                        throw new Exception($"Error en línea {numeroLinea}: El 'Tipo' debe ser 0 (Simple) o 1 (Periódica). Valor encontrado: '{values[1]}'");
                    }

                    // Validar Orden (Entero)
                    if (!int.TryParse(values[2], out int orden))
                    {
                        throw new Exception($"Error en línea {numeroLinea}: El 'Orden' debe ser un número entero válido.");
                    }

                    // Validar Floats (Lat, Lon, Alt, Vel) con CultureInfo.InvariantCulture
                    if (!float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float lat) || lat < -90 || lat > 90)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Latitud' inválida ({values[3]}). Debe estar entre -90 y 90.");
                    }

                    if (!float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float lon) || lon < -180 || lon > 180)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Longitud' inválida ({values[4]}). Debe estar entre -180 y 180.");
                    }

                    if (!float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float altura))
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Altura' inválida ({values[5]}).");
                    }

                    if (!float.TryParse(values[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float velocidad) || velocidad < 0)
                    {
                        throw new Exception($"Error en línea {numeroLinea}: 'Velocidad' inválida ({values[6]}). No puede ser negativa.");
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
            }

            /*PRUEBA REDIS*/
            //Invalidar caché tras importar rutas
            await _cache.KeyDeleteAsync(ROUTES_CACHE_KEY);

            Console.WriteLine("[RouteService] Routes cache invalidated after CSV import");
            /*FIN PRUEBA*/

            return rutasParaGuardar.Count;
        }
    }
}