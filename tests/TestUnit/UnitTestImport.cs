using CentralBackend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TestUnit.util.TestUnit;

namespace TestUnit
{

    //Voy a usar la nomenclatura de pruebas recomendada en c# que es Acción_QuéPruebo_ResultadoEsperado
    [TestClass()]
    public class UnitTestImport : UnitTestBase
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
        [DataRow("cp1.1.csv","0;43.5450;-5.6600;50.0;10.0", 1)]
        [DataRow("cp1.2.csv", "0;43.5450;-5.6600;50.0;10.0\n0;43.6450;-5.7600;50.0;10.0\n0;43.7450;-5.8600;50.0;10.0", 3)]
        public async Task PruebaImport(string fileName,string expectedString , int expectedCreatedRoutes)
        {
            string csvContent = GeneralTestUtil.GetCsvContent(fileName);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            int count = await _routeService.ImportFromCsvAsync(stream, fileName);
            Assert.AreEqual(1, count, "Debería haber importado "+ expectedCreatedRoutes +" ruta/s");

            string sql = @"
                SELECT r.Type, printf('%.4f', p.Lat), printf('%.4f', p.Long), ROUND(p.Height,2), ROUND(p.Velocity,2) as Velocity
                FROM Routes r
                JOIN  RoutePoints p on r.Id = RouteId
                ORDER BY p.id";

            string actualData = util.ExecuteQueryToCsv(sql,";");
            Assert.AreEqual(expectedString, actualData, "Los datos importados no coinciden con lo esperado");
            //Console.WriteLine("Datos de la bd: "+ actualData);
        }
    }
}
