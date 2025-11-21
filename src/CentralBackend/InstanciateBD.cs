using Microsoft.EntityFrameworkCore;
using Models;

namespace CentralBackend
{
    public static class InstanciateBD
    {
        public static void FormaBaseDeBD()
        {
            using (var db = new FireDrone())
            {
                // Aplicar migraciones
                db.Database.Migrate();

                db.Database.EnsureDeleted(); // Borra toda la base
                db.Database.EnsureCreated(); // La vuelve a crear con la estructura actual


                // ---------- Sensores ----------
                if (!db.Sensors.Any())
                {
                    db.Sensors.AddRange(
                        new Sensor { Model = "SensorModelX" },
                        new Sensor { Model = "SensorModelY" },
                        new Sensor { Model = "SensorModelZ" },
                        new Sensor { Model = "SensorModelUwU" }
                    );
                    db.SaveChanges();
                }

                // Solo sembrar si no hay drones
                if (db.Drones.Any())
                {
                    Console.WriteLine("La base de datos ya contiene datos.");
                    return;
                }

                Console.WriteLine("Sembrando datos iniciales...");

                // ---------- BaseStation y ControlStation ----------
                var baseStation = new BaseStation();
                var controlStation = new ControlStation
                {
                    Lat = 43.36f,
                    Lon = -5.84f,
                    BaseStations = new List<BaseStation> { baseStation }
                };

                db.BaseStations.Add(baseStation);
                db.ControlStations.Add(controlStation);

                // ---------- Route y RoutePoint ----------
                var route = new Models.Route
                {
                    Type = RouteType.Simple,
                    Perimeter = new Perimeter(),
                    Coords = new List<RoutePoint>()
                };

                var point1 = new RoutePoint { Lat = 43.36f, Long = -5.84f, Height = 10, Route = route };
                var point2 = new RoutePoint { Lat = 43.37f, Long = -5.85f, Height = 12, Route = route };
                route.Coords.Add(point1);
                route.Coords.Add(point2);

                db.Routes.Add(route);
                db.RoutePoints.AddRange(point1, point2);

                // ---------- FlightPlan ----------
                var flightPlan = new FlightPlan
                {
                    Ruta = route,
                    Ctrl = controlStation,
                    StartingTime = DateTime.Now,
                    State = FlightStatus.OnCourse
                };
                flightPlan.ModeChangeHistoric.Add(new ChangeMode { Moment = DateTime.Now, Mode = FlightMode.Auto });
                db.FlightPlans.Add(flightPlan);

                // ---------- Drones ----------
                var dron1 = new Dron
                {
                    Base = baseStation,
                    ControlStation = controlStation,
                    Actual = flightPlan,
                    Lat = 43.36f,
                    Lon = -5.84f,
                    State = DroneState.Landed
                };
                var dronChar1 = new DronCharacteristics
                {
                    Model = "DronX1",
                    Dron = dron1,
                    Sensors = db.Sensors.ToList()
                };
                dron1.DronCharacteristics = dronChar1;

                var dron2 = new Dron
                {
                    Base = baseStation,
                    ControlStation = controlStation,
                    Actual = flightPlan,
                    Lat = 43.36f,
                    Lon = -5.84f,
                    State = DroneState.Landed
                };
                var dronChar2 = new DronCharacteristics
                {
                    Model = "DronX1",
                    Dron = dron2,
                    Sensors = db.Sensors.ToList()
                };
                dron2.DronCharacteristics = dronChar2;

                db.Drones.Add(dron1);
                db.DronCharacteristics.Add(dronChar1);
                db.Drones.Add(dron2);
                db.DronCharacteristics.Add(dronChar2);

                // ---------- Sample ----------
                var sample = new Sample
                {
                    Dron = dron1,
                    File = "sample1.txt",
                    Lat = 43.36f,
                    Lon = -5.84f,
                    Time = DateTime.Now
                };
                db.Samples.Add(sample);

                // ---------- Incidence ----------
                var incidence = new Incidence
                {
                    Actual = flightPlan,
                    Msg = "Test incident",
                    Type = "Warning",
                    Time = DateTime.Now
                };
                db.Incidences.Add(incidence);

                // ---------- Perimeter ----------
                var perimeter = route.Perimeter;
                var coord1 = new Coordinate { Latitude = 43.35, Longitude = -5.83, Perimeter = perimeter };
                var coord2 = new Coordinate { Latitude = 43.36, Longitude = -5.84, Perimeter = perimeter };
                perimeter.Coords = new List<Coordinate> { coord1, coord2 };
                db.Perimeters.Add(perimeter);

                // Guardar todos los cambios
                db.SaveChanges();

                // Mostrar comprobación
                Console.WriteLine("Instancias creadas en la base de datos:");
                Console.WriteLine($"Drones: {db.Drones.Count()}");
                Console.WriteLine($"ControlStations: {db.ControlStations.Count()}");
                Console.WriteLine($"BaseStations: {db.BaseStations.Count()}");
                Console.WriteLine($"Routes: {db.Routes.Count()}");
                Console.WriteLine($"FlightPlans: {db.FlightPlans.Count()}");
                Console.WriteLine($"Samples: {db.Samples.Count()}");
                Console.WriteLine($"Incidences: {db.Incidences.Count()}");
                Console.WriteLine($"Sensors: {db.Sensors.Count()}");
                Console.WriteLine($"Perimeters: {db.Perimeters.Count()}");
            }
        }
    }
}