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
                try
                {
                    // Ensure database is deleted and recreated from scratch
                    db.Database.EnsureDeleted(); // Borra toda la base
                    db.Database.EnsureCreated(); // La vuelve a crear con la estructura actual

                    Console.WriteLine("Sembrando datos iniciales...");

                    // ---------- Sensores ----------
                    db.Sensors.AddRange(
                        new Sensor { Model = "SensorModelX" },
                        new Sensor { Model = "SensorModelY" },
                        new Sensor { Model = "SensorModelZ" },
                        new Sensor { Model = "SensorModelUwU" }
                    );
                    db.SaveChanges();

                    // ---------- BaseStation y ControlStation ----------
                    // Control Station at Universidad de Oviedo - Campus de Gijón
                    var baseStation = new BaseStation();
                    var controlStation = new ControlStation
                    {
                        Lat = 43.5267f,  // Campus Universitario de Gijón
                        Lon = -5.6445f,
                        BaseStations = new List<BaseStation> { baseStation }
                    };

                    db.BaseStations.Add(baseStation);
                    db.ControlStations.Add(controlStation);

                    // ---------- Route 1 ----------
                    var route1 = new Models.Route
                    {
                        Type = RouteType.Simple,
                        Perimeter = new Perimeter(),
                        Coords = new List<RoutePoint>()
                    };

                    var point1_1 = new RoutePoint { Lat = 43.5408f, Long = -5.6615f, Height = 50, Route = route1 };  // Playa San Lorenzo
                    var point1_2 = new RoutePoint { Lat = 43.5450f, Long = -5.6660f, Height = 55, Route = route1 };  // Hacia Cerro de Santa Catalina
                    route1.Coords.Add(point1_1);
                    route1.Coords.Add(point1_2);

                    db.Routes.Add(route1);
                    db.RoutePoints.AddRange(point1_1, point1_2);

                    // ---------- Route 2 ----------
                    var route2 = new Models.Route
                    {
                        Type = RouteType.Simple,
                        Perimeter = new Perimeter(),
                        Coords = new List<RoutePoint>()
                    };

                    var point2_1 = new RoutePoint { Lat = 43.5472f, Long = -5.6682f, Height = 45, Route = route2 };  // Puerto Deportivo
                    var point2_2 = new RoutePoint { Lat = 43.5500f, Long = -5.6700f, Height = 50, Route = route2 };  // Hacia Museo del Ferrocarril
                    route2.Coords.Add(point2_1);
                    route2.Coords.Add(point2_2);

                    db.Routes.Add(route2);
                    db.RoutePoints.AddRange(point2_1, point2_2);

                    // ---------- FlightPlan 1 ----------
                    var flightPlan1 = new FlightPlan
                    {
                        Ruta = route1,
                        Ctrl = controlStation,
                        StartingTime = DateTime.Now,
                        State = FlightStatus.OnCourse
                    };
                    flightPlan1.ModeChangeHistoric.Add(new ChangeMode { Moment = DateTime.Now, Mode = FlightMode.Auto });

                    // ---------- FlightPlan 2 ----------
                    var flightPlan2 = new FlightPlan
                    {
                        Ruta = route2,
                        Ctrl = controlStation,
                        StartingTime = DateTime.Now,
                        State = FlightStatus.OnCourse
                    };
                    flightPlan2.ModeChangeHistoric.Add(new ChangeMode { Moment = DateTime.Now, Mode = FlightMode.Auto });

                    // ---------- Drone 1 - Patrullando Playa de San Lorenzo ----------
                    var dron1 = new Dron
                    {
                        Base = baseStation,
                        ControlStation = controlStation,
                        Actual = flightPlan1,
                        Lat = 43.5408f,  // Playa de San Lorenzo (zona central)
                        Lon = -5.6615f,
                        State = DroneState.Flying,
                        Altitude = 50,
                        Speed = 15,
                        Battery = 85
                    };
                    var dronChar1 = new DronCharacteristics
                    {
                        Model = "DronX1-FireWatch",
                        Dron = dron1,
                        Sensors = db.Sensors.ToList()
                    };
                    dron1.DronCharacteristics = dronChar1;
                    flightPlan1.Dron = dron1;

                    // ---------- Drone 2 - En Puerto Deportivo ----------
                    var dron2 = new Dron
                    {
                        Base = baseStation,
                        ControlStation = controlStation,
                        Actual = flightPlan2,
                        Lat = 43.5472f,  // Puerto Deportivo de Gijón
                        Lon = -5.6682f,
                        State = DroneState.Landed,
                        Altitude = 0,
                        Speed = 0,
                        Battery = 95
                    };
                    var dronChar2 = new DronCharacteristics
                    {
                        Model = "DronX2-Coastal",
                        Dron = dron2,
                        Sensors = db.Sensors.ToList()
                    };
                    dron2.DronCharacteristics = dronChar2;
                    flightPlan2.Dron = dron2;

                    db.FlightPlans.Add(flightPlan1);
                    db.FlightPlans.Add(flightPlan2);
                    db.Drones.Add(dron1);
                    db.DronCharacteristics.Add(dronChar1);
                    db.Drones.Add(dron2);
                    db.DronCharacteristics.Add(dronChar2);

                    // ---------- Sample ----------
                    var sample = new Sample
                    {
                        Dron = dron1,
                        File = "sample_playa_san_lorenzo.jpg",
                        Lat = 43.5408f,
                        Lon = -5.6615f,
                        Time = DateTime.Now
                    };
                    db.Samples.Add(sample);

                    // ---------- Incidence ----------
                    var incidence = new Incidence
                    {
                        Actual = flightPlan1,
                        Msg = "Rutina de patrullaje en Playa de San Lorenzo",
                        Type = "Info",
                        Time = DateTime.Now
                    };
                    db.Incidences.Add(incidence);

                    // ---------- Perimeter 1 - Área Playa San Lorenzo ----------
                    var perimeter1 = route1.Perimeter;
                    var coord1_1 = new Coordinate { Latitude = 43.5390, Longitude = -5.6600, Perimeter = perimeter1 };  // Inicio playa
                    var coord1_2 = new Coordinate { Latitude = 43.5420, Longitude = -5.6630, Perimeter = perimeter1 };  // Mitad playa
                    var coord1_3 = new Coordinate { Latitude = 43.5450, Longitude = -5.6660, Perimeter = perimeter1 };  // Hacia Cerro
                    var coord1_4 = new Coordinate { Latitude = 43.5430, Longitude = -5.6640, Perimeter = perimeter1 };  // Regreso
                    perimeter1.Coords = new List<Coordinate> { coord1_1, coord1_2, coord1_3, coord1_4 };
                    db.Perimeters.Add(perimeter1);

                    // ---------- Perimeter 2 - Área Puerto Deportivo ----------
                    var perimeter2 = route2.Perimeter;
                    var coord2_1 = new Coordinate { Latitude = 43.5460, Longitude = -5.6670, Perimeter = perimeter2 };  // Entrada puerto
                    var coord2_2 = new Coordinate { Latitude = 43.5472, Longitude = -5.6682, Perimeter = perimeter2 };  // Centro puerto
                    var coord2_3 = new Coordinate { Latitude = 43.5485, Longitude = -5.6695, Perimeter = perimeter2 };  // Salida puerto
                    var coord2_4 = new Coordinate { Latitude = 43.5500, Longitude = -5.6700, Perimeter = perimeter2 };  // Zona museo
                    perimeter2.Coords = new List<Coordinate> { coord2_1, coord2_2, coord2_3, coord2_4 };
                    db.Perimeters.Add(perimeter2);

                    // Guardar todos los cambios
                    db.SaveChanges();

                    // Mostrar comprobación
                    Console.WriteLine("Instancias creadas en la base de datos:");
                    Console.WriteLine($"Drones: {db.Drones.Count()}");
                    Console.WriteLine($"  - Drone 1 (DronX1-FireWatch): Flying at Playa de San Lorenzo ({dron1.Lat}, {dron1.Lon})");
                    Console.WriteLine($"  - Drone 2 (DronX2-Coastal): Landed at Puerto Deportivo ({dron2.Lat}, {dron2.Lon})");
                    Console.WriteLine($"ControlStations: {db.ControlStations.Count()} - Campus Universitario de Gijón");
                    Console.WriteLine($"BaseStations: {db.BaseStations.Count()}");
                    Console.WriteLine($"Routes: {db.Routes.Count()}");
                    Console.WriteLine($"FlightPlans: {db.FlightPlans.Count()}");
                    Console.WriteLine($"Samples: {db.Samples.Count()}");
                    Console.WriteLine($"Incidences: {db.Incidences.Count()}");
                    Console.WriteLine($"Sensors: {db.Sensors.Count()}");
                    Console.WriteLine($"Perimeters: {db.Perimeters.Count()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
        }
    }
}