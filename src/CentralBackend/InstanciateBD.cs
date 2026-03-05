using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace CentralBackend
{
    public static class InstanciateBD
    {
        // Configuración de la cantidad de datos a generar
        private const int NUM_SENSORS = 10;
        private const int NUM_BASE_STATIONS = 3;
        private const int NUM_ROUTES = 5;
        private const int NUM_DRONES = 11; // Generaremos 11 drones (1 sin plan de vuelo)

        // Coordenadas base (Gijón) para generar variaciones
        private const float BASE_LAT = 43.5322f;
        private const float BASE_LON = -5.6611f;

        public static void FormaBaseDeBD(int nDrones = NUM_DRONES)
        {
            using (var db = new FireDrone())
            {
                try
                {
                    ResetDatabase(db);

                    SeedSensors(db);
                    SeedInfrastructure(db);
                    var routes = SeedRoutes(db);
                    var drones = SeedDrones(db, nDrones);

                    SeedFlightPlansAndSamples(db, drones, routes, nDrones);

                    PrintSummary(db);
                }
                catch (Exception ex)
                {
                    LogDatabaseError(ex);
                    throw;
                }
            }
        }

        private static void ResetDatabase(FireDrone db)
        {
            Console.WriteLine("Asegurando estructura de base de datos...");
            db.Database.EnsureCreated();
            Console.WriteLine("Limpiando datos antiguos...");

            // Usamos ExecuteSqlRaw para vaciar los datos sin corromper el archivo .db en Docker
            db.Database.ExecuteSqlRaw("DELETE FROM Samples");
            db.Database.ExecuteSqlRaw("DELETE FROM FlightPlans");
            db.Database.ExecuteSqlRaw("DELETE FROM Drones"); // Usamos el nombre real de tu tabla
            db.Database.ExecuteSqlRaw("DELETE FROM DronCharacteristics");
            db.Database.ExecuteSqlRaw("DELETE FROM RoutePoints");
            db.Database.ExecuteSqlRaw("DELETE FROM Routes");
            db.Database.ExecuteSqlRaw("DELETE FROM BaseStations");
            db.Database.ExecuteSqlRaw("DELETE FROM ControlStations");
            db.Database.ExecuteSqlRaw("DELETE FROM Sensors");
        }

        private static void SeedSensors(FireDrone db)
        {
            var sensors = new List<Sensor>();
            for (int i = 1; i <= NUM_SENSORS; i++)
            {
                sensors.Add(new Sensor { Model = $"SensorModel-{char.ConvertFromUtf32(65 + (i % 26))}{i}" });
            }
            db.Sensors.AddRange(sensors);
            db.SaveChanges();
        }

        private static void SeedInfrastructure(FireDrone db)
        {
            var controlStation = new ControlStation
            {
                Lat = 43.5267f,
                Lon = -5.6445f,
                BaseStations = new List<BaseStation>()
            };

            for (int i = 1; i <= NUM_BASE_STATIONS; i++)
            {
                var baseStation = new BaseStation();
                controlStation.BaseStations.Add(baseStation);
            }

            db.ControlStations.Add(controlStation);
            db.SaveChanges();
        }

        private static List<Models.Route> SeedRoutes(FireDrone db)
        {
            var routes = new List<Models.Route>();

            for (int i = 1; i <= NUM_ROUTES; i++)
            {
                var route = CreateRoute(i);
                routes.Add(route);
            }

            db.Routes.AddRange(routes);
            db.SaveChanges();
            return routes;
        }

        private static Models.Route CreateRoute(int index)
        {
            var route = new Models.Route
            {
                Type = index % 2 == 0 ? RouteType.Simple : RouteType.Periodic,
                Perimeter = new Perimeter(),
                Coords = new List<RoutePoint>()
            };

            int numPoints = RandomNumberGenerator.GetInt32(3, 6);
            for (int j = 0; j < numPoints; j++)
            {
                route.Coords.Add(new RoutePoint
                {
                    Lat = BASE_LAT + (float)(NextSecureDouble() * 0.02 - 0.01),
                    Long = BASE_LON + (float)(NextSecureDouble() * 0.02 - 0.01)
                });
            }
            return route;
        }

        // Este método reemplaza a random.NextDouble() usando criptografía
        private static double NextSecureDouble()
        {
            // Generamos un entero aleatorio entre 0 y el máximo valor posible
            // y lo dividimos por el máximo para obtener un valor entre 0.0 y 1.0
            return (double)RandomNumberGenerator.GetInt32(0, int.MaxValue) / int.MaxValue;
        }
        private static List<Dron> SeedDrones(FireDrone db, int nDrones)
        {
            var drones = new List<Dron>();

            for (int i = 1; i <= nDrones; i++)
            {
                var drone = CreateSpecificDrone(i);
                drones.Add(drone);
                db.Drones.Add(drone);
            }

            db.SaveChanges();
            return drones;
        }

        private static Dron CreateSpecificDrone(int i)
        {
            // Lógica de batería y estado extraída para reducir anidamiento
            int battery = i switch
            {
                1 => 110,
                2 => 50,
                _ => RandomNumberGenerator.GetInt32(300, 1001)
            };

            var state = (i == 1 || i == 2) ? (DroneState)1 : (DroneState)0;

            var newDrone = new Dron
            {
                Battery = battery,
                State = state
            };
            var dronChar = new DronCharacteristics();
            newDrone.DronCharacteristics= dronChar;

            return newDrone;
        }

        private static void SeedFlightPlansAndSamples(FireDrone db, List<Dron> drones, List<Models.Route> routes, int nPlans)
        {
            if (!drones.Any() || !routes.Any()) return;

            for (int i = 0; i < nPlans; i++)
            {
                var assignedDron = drones[i % drones.Count];
                var assignedRoute = routes[i % routes.Count];

                var plan = new FlightPlan { Dron = assignedDron, Ruta = assignedRoute };
                db.FlightPlans.Add(plan);

                if (i % 2 == 0) // Añadir muestras solo a la mitad
                {
                    db.Samples.Add(new Sample { Dron = assignedDron, File = $"sample_{i}.jpg" });
                }
            }
            db.SaveChanges();
        }

        private static void PrintSummary(FireDrone db)
        {
            Console.WriteLine("--- Resumen de siembra ---");
            Console.WriteLine($"Drones: {db.Drones.Count()}");
            Console.WriteLine($"Flight Plans: {db.FlightPlans.Count()}");
            Console.WriteLine($"Samples: {db.Samples.Count()}");
            Console.WriteLine("--------------------------");
        }

        private static void LogDatabaseError(Exception ex)
        {
            Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }
}