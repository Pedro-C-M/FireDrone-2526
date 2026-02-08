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

        private static MemoryStream GenerateStreamFromFileName(string fileName)
        {
            string csvContent = GeneralTestUtil.GetCsvContent(fileName);
            return new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        }


        [TestMethod]
        [DataRow("cp1.1.csv","0;43.5450;-5.6600;50.0;10.0", 1)]
        [DataRow("cp1.2.csv", "0;43.5450;-5.6600;50.0;10.0\n0;43.6450;-5.7600;50.0;10.0\n0;43.7450;-5.8600;50.0;10.0", 3)]
        public async Task Import_DifferentNPoints_Correct(string fileName,string expectedString , int expectedCreatedRoutes)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            int count = await _routeService.ImportFromCsvAsync(stream, fileName);
            Assert.AreEqual(1, count, "Deberia haber importado "+ expectedCreatedRoutes +" ruta/s");

            string sql = @"
                SELECT r.Type, printf('%.4f', p.Lat), printf('%.4f', p.Long), ROUND(p.Height,2), ROUND(p.Velocity,2) as Velocity
                FROM Routes r
                JOIN  RoutePoints p on r.Id = RouteId
                ORDER BY p.id";

            string actualData = util.ExecuteQueryToCsv(sql,";");

            actualData = actualData.Replace("\r", "");
            expectedString = expectedString.Replace("\r", "");

            Assert.AreEqual(expectedString, actualData, "Los datos importados no coinciden con lo esperado");
            //Console.WriteLine("Datos de la bd: "+ actualData);

        }

        [TestMethod]
        [DataRow("cp2.csv", "No hay puntos en la ruta.")]
        public async Task Import_NoRoutePoints_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.AreEqual(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp3.csv", "se definio antes con otro TIPO.")]
        public async Task Import_OneDifferetnTypePoint_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp4.1.csv", "El 'Nombre' de la ruta no puede estar vacio.")]
        [DataRow("cp4.2.csv", "El 'Tipo' debe ser")]
        [DataRow("cp4.3.csv", "'Latitud' invalida")]
        [DataRow("cp4.4.csv", "'Longitud' invalida")]
        [DataRow("cp4.5.csv", "'Altura' invalida")]
        [DataRow("cp4.6.csv", "'Velocidad' invalida")]
        public async Task Import_NullValues_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp5.csv", "El 'Tipo' debe ser")]
        public async Task Import_TypeNotInteger_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp6.1.csv", "'Latitud' invalida")]
        [DataRow("cp6.2.csv", "'Longitud' invalida")]
        [DataRow("cp6.3.csv", "'Altura' invalida")]
        [DataRow("cp6.4.csv", "'Velocidad' invalida")]
        public async Task Import_NotFloat_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp7.csv", "El 'Tipo' debe ser")]
        public async Task Import_TypeNotCorrect_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp8.1.csv", "'Velocidad' invalida")]
        [DataRow("cp8.2.csv", "'Altura' invalida")]
        public async Task Import_NegativeValue_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp9.1.csv", "'Latitud' invalida")]
        [DataRow("cp9.2.csv", "'Longitud' invalida")]
        public async Task Import_CoordBadFormat_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp10.1.csv", "Se esperaban 6 valores")]
        [DataRow("cp10.2.csv", "Se esperaban 6 valores")]
        public async Task Import_BadCsvFields_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp11.txt", "Formato no valido")]
        public async Task Import_DifferentFileType_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }

        [TestMethod]
        [DataRow("cp12", "Formato no valido")]        
        public async Task Import_NoFileType_Fail(string fileName, string expectedExceptionMessage)
        {
            var stream = GenerateStreamFromFileName(fileName);

            util.CleanTables(new[] { "RoutePoints", "Routes" }, true);//Esto puede ser quitable

            Exception e = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _routeService.ImportFromCsvAsync(stream, fileName);
            });

            Assert.Contains(expectedExceptionMessage, e.Message);
        }
    }
}
