using CentralBackend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit
{

    //Voy a usar la nomenclatura de pruebas recomendada en c# que es Acción_QuéPruebo_ResultadoEsperado
    [TestClass()]
    public class UnitTestExport : UnitTestBase
    {
        private RouteService _routeService;
        private Mock<RedisCacheService> _mockCache;
        private Mock<ILogger<RouteService>> _mockLogger;

        [TestInitialize] public override void SetUp()
        {
            base.SetUp();

            //Preparo los mocks para poder crear el servicio de rutas, realmente no se necesita lógica en ellos
            _mockCache = new Mock<RedisCacheService>();
            _mockLogger = new Mock<ILogger<RouteService>>();

            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            _routeService = new RouteService(base.context, _mockCache.Object, _mockLogger.Object);
        }
        [TestCleanup] public override void TearDown()
        {
            base.TearDown();
        }


        [TestMethod]
        public async Task Export_OneRouteOnePoint_Correct()
        {
            string expectedCsv = "Nombre;Tipo;Lat;Lon;Altura;Velocidad\r\nRuta_1;0;43.500000;-5.500000;10.00;5.00\r\n";

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);

            //CREACIÓN DE LA RUTA EN BD
            var nuevaRuta = new Models.Route
            {
                Type = Models.RouteType.Simple,
                Perimeter = new Models.Perimeter(),
                Coords = new List<Models.RoutePoint>()
            };

            nuevaRuta.Coords.Add(new Models.RoutePoint
            {
                Lat = 43.50f,
                Long = -5.50f,
                Height = 10,
                Velocity = 5
            });
            context.Routes.Add(nuevaRuta);
            context.SaveChanges();
            //EXPORTAR Y COMPROBAR
            int routeId = nuevaRuta.Id;

            byte[] resultBytes = await _routeService.ExportRouteToCsvAsync(routeId);

            string resultCsv = Encoding.UTF8.GetString(resultBytes);

            Assert.AreEqual(expectedCsv, resultCsv, "Los datos importados no coinciden con lo esperado");
        }

        [TestMethod]
        public async Task Export_WithSeveralRouteSeveralPoint_Correct()
        {
            string expectedCsv = "Nombre;Tipo;Lat;Lon;Altura;Velocidad\r\nRuta_2;1;53.500000;-15.500000;10.00;5.00\r\nRuta_2;1;54.500000;-16.500000;10.00;5.00\r\nRuta_2;1;55.500000;-17.500000;10.00;5.00\r\n";

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);

            //CREACIÓN RUTAS EN BD
            var nuevaRuta1 = new Models.Route
            {
                Type = Models.RouteType.Simple,
                Perimeter = new(),
                Coords = new List<Models.RoutePoint>
                { 
                    new() { Lat = 43.50f, Long = -5.50f, Height = 10, Velocity = 5 },
                    new() { Lat = 44.50f, Long = -6.50f, Height = 10, Velocity = 5 },
                    new() { Lat = 45.50f, Long = -7.50f, Height = 10, Velocity = 5 }
                }
            };
            context.Routes.Add(nuevaRuta1);

            var nuevaRuta2 = new Models.Route
            {
                Type = Models.RouteType.Periodic,
                Perimeter = new(),
                Coords = new List<Models.RoutePoint>
                {
                    new() { Lat = 53.50f, Long = -15.50f, Height = 10, Velocity = 5 },
                    new() { Lat = 54.50f, Long = -16.50f, Height = 10, Velocity = 5 },
                    new() { Lat = 55.50f, Long = -17.50f, Height = 10, Velocity = 5 }
                }
            };
            context.Routes.Add(nuevaRuta2);

            var nuevaRuta3 = new Models.Route
            {
                Type = Models.RouteType.Simple,
                Perimeter = new(),
                Coords = new List<Models.RoutePoint>
                {
                    new() { Lat = 63.50f, Long = -25.50f, Height = 10, Velocity = 5 },
                    new() { Lat = 64.50f, Long = -26.50f, Height = 10, Velocity = 5 },
                    new() { Lat = 65.50f, Long = -27.50f, Height = 10, Velocity = 5 }
                }
            };
            context.Routes.Add(nuevaRuta3);

            context.SaveChanges();
            //EXPORTAR Y COMPROBAR
            int routeId = nuevaRuta2.Id;

            byte[] resultBytes = await _routeService.ExportRouteToCsvAsync(routeId);

            string resultCsv = Encoding.UTF8.GetString(resultBytes);

            Assert.AreEqual(expectedCsv, resultCsv, "Los datos importados no coinciden con lo esperado");
        }

        [TestMethod]
        public async Task Export_NoRoutes_Fail()
        {
            string expectedExceptionMessage = "La ruta con ID 1 no existe.";

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);

            int routeId = 1;

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ExportRouteToCsvAsync(1);
            });

            Assert.AreEqual(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow(-1, "La ruta con ID -1 no existe.")]
        [DataRow(null, "La ruta con ID 0 no existe.")]
        public async Task Export_BadIds_Fail(int paramId, string expectedExceptionMessage)
        {
            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);

            var nuevaRuta = new Models.Route
            {
                Type = Models.RouteType.Simple,
                Perimeter = new Models.Perimeter(),
                Coords = new List<Models.RoutePoint>()
            };

            nuevaRuta.Coords.Add(new Models.RoutePoint
            {
                Lat = 43.50f,
                Long = -5.50f,
                Height = 10,
                Velocity = 5
            });
            context.Routes.Add(nuevaRuta);
            context.SaveChanges();

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ExportRouteToCsvAsync(paramId);
            });

            Assert.AreEqual(expectedExceptionMessage, e.Message);
        }
    }
}
