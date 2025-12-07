using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CentralBackend
{
    public static class InstanciateBD
    {
        // Configuración de la cantidad de datos a generar
        private const int NUM_SENSORS = 10;
        private const int NUM_BASE_STATIONS = 3;
        private const int NUM_ROUTES = 5;
        private const int NUM_DRONES = 10; // Generaremos 10 drones
        private const int NUM_FLIGHT_PLANS = 10; // Un plan por dron

        // Coordenadas base (Gijón) para generar variaciones
        private const float BASE_LAT = 43.5322f;
        private const float BASE_LON = -5.6611f;

        public static void FormaBaseDeBD()
        {
            using (var db = new FireDrone())
            {
                try
                {
                    // 1. Limpiar y recrear la base de datos
                    Console.WriteLine("Eliminando base de datos antigua...");
                    db.Database.EnsureDeleted();
                    Console.WriteLine("Creando nueva estructura de base de datos...");
                    db.Database.EnsureCreated();

                    Console.WriteLine("Sembrando datos masivos...");
                    var random = new Random();

                    // 2. Generar Sensores
                    var sensors = new List<Sensor>();
                    for (int i = 1; i <= NUM_SENSORS; i++)
                    {
                        sensors.Add(new Sensor { Model = $"SensorModel-{char.ConvertFromUtf32(65 + (i % 26))}{i}" });
                    }
                    db.Sensors.AddRange(sensors);
                    db.SaveChanges(); // Guardamos para tener IDs si fueran necesarios

                    // 3. Generar Control Station (Principal)
                    var controlStation = new ControlStation
                    {
                        Lat = 43.5267f,  // Campus Universitario de Gijón
                        Lon = -5.6445f,
                        BaseStations = new List<BaseStation>()
                    };
                    db.ControlStations.Add(controlStation);

                    // 4. Generar Base Stations
                    for (int i = 1; i <= NUM_BASE_STATIONS; i++)
                    {
                        var baseStation = new BaseStation(); // Aquí podrías añadir propiedades si BaseStation las tuviera
                        controlStation.BaseStations.Add(baseStation);
                        db.BaseStations.Add(baseStation);
                    }
                    db.SaveChanges();

                    // 5. Generar Rutas y Puntos de Ruta
                    var routes = new List<Models.Route>();
                    for (int i = 1; i <= NUM_ROUTES; i++)
                    {
                        var route = new Models.Route
                        {
                            Type = i % 2 == 0 ? RouteType.Simple : RouteType.Periodic, // Alternar tipos
                            Perimeter = new Perimeter(),
                            Coords = new List<RoutePoint>()
                        };

                        // Crear 3-5 puntos aleatorios cercanos a Gijón para cada ruta
                        int numPoints = random.Next(3, 6);
                        for (int j = 0; j < numPoints; j++)
                        {
                            var point = new RoutePoint
                            {
                                Lat = BASE_LAT + (float)(random.NextDouble() * 0.02 - 0.01), // Variación +/- 0.01 grados
                                Long = BASE_LON + (float)(random.NextDouble() * 0.02 - 0.01),
                                Height = random.Next(30, 100),
                                Route = route
                            };
                            route.Coords.Add(point);
                            db.RoutePoints.Add(point);
                        }

                        // Crear Perímetro para la ruta
                        var perimeter = route.Perimeter;
                        perimeter.Coords = new List<Coordinate>();

                        var firstPoint = route.Coords.First();
                        var centerLat = firstPoint.Lat;
                        var centerLon = firstPoint.Long;

                        double offset = 0.002;

                        perimeter.Coords.Add(new Coordinate { Latitude = (double)(centerLat + offset), Longitude = (double)(centerLon + offset), Perimeter = perimeter });
                        perimeter.Coords.Add(new Coordinate { Latitude = (double)(centerLat + offset), Longitude = (double)(centerLon + offset), Perimeter = perimeter });
                        perimeter.Coords.Add(new Coordinate { Latitude = (double)(centerLat + offset), Longitude = (double)(centerLon + offset), Perimeter = perimeter });
                        perimeter.Coords.Add(new Coordinate { Latitude = (double)(centerLat + offset), Longitude = (double)(centerLon + offset), Perimeter = perimeter });

                        db.Perimeters.Add(perimeter);
                        routes.Add(route);
                        db.Routes.Add(route);
                    }
                    db.SaveChanges();

                    // 6. Generar Drones y sus Características
                    var drones = new List<Dron>();
                    var baseStationsList = controlStation.BaseStations.ToList();

                    for (int i = 1; i <= NUM_DRONES; i++)
                    {
                        var dron = new Dron
                        {
                            Base = baseStationsList[i % baseStationsList.Count],

                            ControlStation = controlStation,
                            Lat = BASE_LAT + (float)(random.NextDouble() * 0.03 - 0.015),
                            Lon = BASE_LON + (float)(random.NextDouble() * 0.03 - 0.015),
                            State = (DroneState)random.Next(0, 3),
                            Altitude = random.Next(0, 120),
                            Speed = random.Next(0, 60),
                            Battery = random.Next(10, 100)
                        };
                        var dronChar = new DronCharacteristics
                        {
                            Model = $"FireWatch-X{i}",
                            Dron = dron,
                            Sensors = sensors.OrderBy(x => random.Next()).Take(2).ToList()
                        };
                        dron.DronCharacteristics = dronChar;

                        drones.Add(dron);
                        db.Drones.Add(dron);
                        db.DronCharacteristics.Add(dronChar);
                    }
                    db.SaveChanges();

                    // 7. Generar Planes de Vuelo (Asignar 1 a cada dron para simplificar, o aleatorio)
                    for (int i = 0; i < NUM_FLIGHT_PLANS; i++)
                    {
                        // Asegurarnos de no salirnos del índice si hay menos rutas/drones que planes
                        var assignedDron = drones[i % drones.Count];
                        var assignedRoute = routes[i % routes.Count];

                        var flightPlan = new FlightPlan
                        {
                            Ruta = assignedRoute,
                            Ctrl = controlStation,
                            StartingTime = DateTime.Now.AddMinutes(-random.Next(0, 120)), // Empezó hace un rato
                            State = (FlightStatus)random.Next(0, 3),
                            Dron = assignedDron // Asignamos el dron al plan
                        };

                        // Actualizar la referencia circular en el dron (el dron conoce su plan actual)
                        assignedDron.Actual = flightPlan;

                        // Histórico de cambios de modo
                        flightPlan.ModeChangeHistoric = new List<ChangeMode>
                        {
                            new ChangeMode { Moment = DateTime.Now.AddMinutes(-30), Mode = FlightMode.Auto }
                        };

                        db.FlightPlans.Add(flightPlan);

                        // 8. Generar Muestras (Samples) e Incidencias para este plan
                        if (i % 2 == 0) // Solo generar para la mitad de los planes
                        {
                            db.Samples.Add(new Sample
                            {
                                Dron = assignedDron,
                                File = $"sample_plan_{i}.jpg",
                                Lat = assignedDron.Lat,
                                Lon = assignedDron.Lon,
                                Time = DateTime.Now
                            });

                            db.Incidences.Add(new Incidence
                            {
                                Actual = flightPlan,
                                Msg = $"Reporte rutinario del plan {i}",
                                Type = "Info",
                                Time = DateTime.Now
                            });
                        }
                    }

                    // Guardar todos los cambios finales
                    db.SaveChanges();

                    // Mostrar resumen en consola
                    Console.WriteLine("\n--- Resumen de Datos Generados ---");
                    Console.WriteLine($"Sensores: {db.Sensors.Count()}");
                    Console.WriteLine($"ControlStations: {db.ControlStations.Count()}");
                    Console.WriteLine($"BaseStations: {db.BaseStations.Count()}");
                    Console.WriteLine($"Routes: {db.Routes.Count()}");
                    Console.WriteLine($"Drones: {db.Drones.Count()}");
                    Console.WriteLine($"FlightPlans: {db.FlightPlans.Count()}");
                    Console.WriteLine($"Samples: {db.Samples.Count()}");
                    Console.WriteLine($"Incidences: {db.Incidences.Count()}");
                    Console.WriteLine($"Perimeters: {db.Perimeters.Count()}");
                    Console.WriteLine("----------------------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
        }
    }
}