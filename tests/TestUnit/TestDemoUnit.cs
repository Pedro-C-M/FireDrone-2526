using System;
using System.Threading.Tasks;
using Giis.Visualassert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestUnit
{
    /// <summary>
    /// Demo de pruebas unitarias para usar como ejemplo para el resto de tests.
    /// Todos los test deben heredar de UnitTestBase que incluye en SetUp y TearDown
    /// la inicializacion de la base de datos (limpiando las tablas para asegurar que se empieza con la bd vacia)
    /// y otras variables para uso general:
    /// -context: el contexto de BD para las pruebas
    /// -util: algunas utilidades para manejar la base de datos
    /// </summary>
    [TestClass()]
    public class TestDemoUnit : UnitTestBase
    {
        /**
         * Si la clase de pruebas necesita acciones adicionales de inicializacion o finalizacion de sus test
         * descomentar estos metodos e incluirlas.
         * Si no, cada test ejecutara la inicializacion/finalizacion establecida en la clase base
        */
        [TestInitialize] public override void SetUp()
        {
            base.SetUp(); //En caso de ser necesario, anyadir el codigo adicional despues de esta linea
        }
        [TestCleanup] public override void TearDown()
        {
            base.TearDown(); //En caso de ser necesario, anyadir el codigo adicional ANTES de esta linea
        }

        // Usar este test mientras se esta configurando la personalizacion para cada proyecto
        /*
        [TestMethod()]
        public virtual void TestTemp()
        {
            util.ExecuteSqlCommand("INSERT INTO Test (value) values ('abc')");
            string actual = util.ExecuteQueryToCsv("select * from Test");
            Assert.AreEqual("abc", actual);
        }
        */
        
        //Un metodo de prueba de ejemplo. En este caso el propio metodo carga los datos que necesita.
        //Si hay datos comunes a toda la clase, se pondria en el SetUp
        [TestMethod()]
        public virtual void TestUnitDemoData()
        {
            //En este caso para establecer los datos iniciales utilizo SQL directamente
            //Se podria hacer con entity framework o ejecutando sentencias en un fichero SQL externo
            util.ExecuteSqlCommand("INSERT INTO ControlStations (Id, Lat, Lon) values ('11', 12, 13)");

            //ejecuta el proceso objeto de esta prueba (simula anyadir una fila en la BD, en este caso usando EF)
            MockProcess();

            //Comparacion de resultados: Como se estan comparando valores en tablas, se utiliza
            //otro metodo de utilidad que ejecuta una query arbitraria y obtiene los resultados en 
            //un formato CSV, que luego es facil de comparar de una sola vez contra otro string CSV
            string actual = util.ExecuteQueryToCsv("select * from ControlStations order by Id");
            Assert.AreEqual("11,12.0,13.0\n21,22.0,23.0", actual);

            //descomentar este assert que contiene un string esperado diferente para comprobar 
            //como se ven las diferencias en html usando el componente VisualAssert
            //(desde visual studio hacer ctrl+click en enlace al fichero de diferencias)
            //la instancia 'va' se ha definido y configurado en la clase base
            //va.AssertEquals("11,xxxx,12.0,13.0\n21,22.0,23.0", actual);
        }

        //Simula un procesamiento determinado que consiste en anyadir una base station
        //En este caso usa los objetos de EF y el contexto de la BD
        private void MockProcess()
        {
            context.Add(new Models.ControlStation
            {
                Id = 21, Lat= 22, Lon=23
            });
            context.SaveChanges();
        }

        //Demo de prueba parametrizada, especificando dos parametros de entrada y la salida esperad
        //el assert comprueba una suma de las entradas
        [TestMethod]
        [DataRow(1, 2, 3)]
        [DataRow(2, 3, 5)]
        [DataRow(3, 4, 7)]
        public void TestUnitDemoParametrized(int first, int second, int expected)
        {
            Assert.AreEqual(expected, first + second);
        }

        //Demo de tratamiento de excepciones.
        //No se deben filtrar excepciones durante la ejecucion de los tests (para eso esta el framework
        //que mostrara que se ha producido una excepcion)
        //Pero a veces el comportamiento esperado es que se produzca una excepcion:
        //Aqui, el primer Assert.Throws... fallara si no se ha producido una excepcion del tipo esperado
        //Luego comprueba el mensaje que debe contener la excepcion
        [TestMethod]
        public void TestUnitDemoException()
        {
            ArithmeticException e = Assert.Throws<ArithmeticException>(() => { MockProcessException(); });
            Assert.AreEqual("Message from exception", e.Message);
        }
        private void MockProcessException()
        {
            throw new ArithmeticException("Message from exception");
        }

        //Si los metodos a probar son async que devuelven su resultado en Task,
        //el metodo de prueba tambien debe ser async Task y usar await
        //para invocar al metodo a probar.
        [TestMethod]
        public async Task TestUnitDemoAsyncMethod()
        {
            string returned = await MockProcessAsync();
            Assert.AreEqual("Returned Message", returned);
        }
        public async Task<string> MockProcessAsync()
        {
            await Task.Delay(500);
            return "Returned Message";
        }

        /////////////////////////////////////////////////////////////
        //Demo de pruebas con mocks (incluye dos servicios y tests)
        /////////////////////////////////////////////////////////////

        //Servicio interno (el que estoy probando).
        //Su unico metodo obtiene una descripcion dado el id de una entidad,
        //Comportamiento: la descripcion es obtenida invocando un servicio externo (que inyecto en el constructor).
        //Si la descripcion corespondiente al id no existe o es string vacio, devuelve "no desc"
        public class InternalService
        {
            private readonly IExternalService _externalService;
            public InternalService(IExternalService externalService)
            {
                _externalService = externalService;
            }
            public string GetExternalDescription(int id)
            {
                string externalDesc = _externalService.GetDescriptionById(id);
                externalDesc = string.IsNullOrEmpty(externalDesc) ? "no desc" : externalDesc;
                return externalDesc;
            }
        }
        //Servicio externo (solo tengo el interfaz), el unico metodo devuelve una descripcion dado su id
        //Comportamiento: Si el id no existe, devuelve null
        public interface IExternalService
        {
            string GetDescriptionById(int id);
        }

        //Situaciones a cubrir emparejados con valores casos de prueba fisicos:
        // id no existe en servicio externo:              id=998   salida: "no desc"
        // id tiene descripcion "" en servicio externo:   id=999   salida: "no desc"
        // id existe en servicio externo:                 id=100   salida: "desc100"
        [TestMethod]
        public void TestUnitDemoMocks()
        {
            //Definicion del mock para el servicio externo, Modela los datos para las situaciones a cubrir.
            //Ejemplo de como retornar valores constantes y dependientes de los parametros
            //Ver mas en: https://github.com/Moq/moq4/wiki/Quickstart
            var mock = new Mock<IExternalService>();
            mock.Setup(x => x.GetDescriptionById(998)).Returns((string)null);
            mock.Setup(x => x.GetDescriptionById(999)).Returns("");
            mock.Setup(x => x.GetDescriptionById(It.Is<int>(i => i < 998))).Returns((int i) => "desc" + i);
            //Adicional: el orden importa, si en esta se pone i>0 falla el test, si se pone i>0 y se pone al principio, no falla
            mock.Setup(x => x.GetDescriptionById(It.Is<int>(i => i > 999))).Returns((int i) => "DESC" + i);

            //Inyecto el servicio externo, pero en vez del real, el mock que acabo de crear
            InternalService internalService = new InternalService(mock.Object);
            Assert.AreEqual("no desc", internalService.GetExternalDescription(998));
            Assert.AreEqual("no desc", internalService.GetExternalDescription(999));
            Assert.AreEqual("desc100", internalService.GetExternalDescription(100));
            Assert.AreEqual("DESC2000", internalService.GetExternalDescription(2000)); //adicional
        }
    }
}
