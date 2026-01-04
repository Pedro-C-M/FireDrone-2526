using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CentralBackend.Services
{
    public class RouteService
    {
        private readonly FireDrone _context; // Tu DbContext

        public RouteService(FireDrone context)
        {
            _context = context;
        }

        public async Task<List<Models.Route>> GetAllAsync()
        {
            // Es vital usar .Include para traer las coordenadas al frontend
            return await _context.Routes
                .Include(r => r.Coords)
                .ToListAsync();
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

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<int> ImportFromCsvAsync(Stream fileStream)
        {
            // Usamos un Diccionario para agrupar. 
            // Clave: Nombre de la ruta (String) -> Valor: Objeto Ruta
            var rutasDict = new Dictionary<string, Models.Route>();

            using (var reader = new StreamReader(fileStream))
            {
                // Saltamos la cabecera
                await reader.ReadLineAsync();

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var values = line.Split(';');
                    if (values.Length < 7) continue;

                    // 1. LEER DATOS CON CULTURA INVARIANTE (Arregla lo de "Alaska")
                    string nombre = values[0].Trim();
                    int tipoInt = int.Parse(values[1]);
                    int orden = int.Parse(values[2]);

                    // Usamos CultureInfo.InvariantCulture para que el "." se lea como decimal
                    float lat = float.Parse(values[3], CultureInfo.InvariantCulture);
                    float lon = float.Parse(values[4], CultureInfo.InvariantCulture);
                    float altura = float.Parse(values[5], CultureInfo.InvariantCulture);
                    float velocidad = float.Parse(values[6], CultureInfo.InvariantCulture);

                    // 2. AGRUPAR CORRECTAMENTE (Arregla lo de las 7 rutas)
                    if (!rutasDict.ContainsKey(nombre))
                    {
                        // Si no existe en el diccionario, la creamos
                        var nuevaRuta = new Models.Route
                        {
                            // Si tu modelo Route tiene propiedad 'Name', asígnala aquí: Name = nombre,
                            Type = (RouteType)tipoInt,
                            Coords = new List<RoutePoint>(),
                            Perimeter = new Perimeter()
                        };
                        rutasDict.Add(nombre, nuevaRuta);
                    }

                    // Añadimos el punto a la ruta existente en el diccionario
                    rutasDict[nombre].Coords.Add(new RoutePoint
                    {
                        Lat = lat,
                        Long = lon, // Asegúrate de que coincida con tu modelo (Lon/Long)
                        Height = altura,
                        Velocity = velocidad,
                        Route = rutasDict[nombre] // Vinculamos la referencia
                    });
                }
            }

            // 3. GUARDAR EN BD
            var rutasParaGuardar = rutasDict.Values.ToList();
            if (rutasParaGuardar.Any())
            {
                _context.Routes.AddRange(rutasParaGuardar);
                await _context.SaveChangesAsync();
            }

            return rutasParaGuardar.Count;
        }

    }
}