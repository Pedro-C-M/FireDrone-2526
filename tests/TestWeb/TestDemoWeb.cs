using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Giis.Selema.Framework.Mstest4;

namespace TestWeb
{
    /// <summary>
    /// Ejemplo de pruebas del frontend.
    /// Todos los test deben heredar de UnitTestBase que incluye en SetUp y TearDown,
    /// la inicializacion de la base de datos (limpiando las tablas para asegurar que se empieza con la bd vacia)
    /// y otras variables para uso general
    /// </summary>
    [TestClass()]
    public class TestDemoWeb : WebTestBase
    {
        /**
         * Si la clase de pruebas necesita acciones adicionales de inicializacion o finalizacion de sus test
         * descomentar estos metodos e incluirlas.
         * Si no, cada test ejecutara la inicializacion/finalizacion establecida en la clase base
        */ 
        [TestInitialize] public override void SetUp()
        {
            base.SetUp(); //En caso de ser necesario, anyadir el codigo adicional despues de esta linea
            FillTestDatabase();
        }
        /**
        [TestCleanup] public override void TearDown()
        {
            base.TearDown(); //En caso de ser necesario, anyadir el codigo adicional ANTES de esta linea
        }
        */
        private void FillTestDatabase() //solo a modo de ejemplo
        {
//            util.ExecuteSqlCommand("insert into BaseStations VALUES (1, 100.0, 100.1)");
//            util.ExecuteSqlCommand("insert into ControlStations VALUES (1, 100.0, 100.1)");
//            util.ExecuteSqlCommand("insert into Drones VALUES (10, 1, 1),(11, 1, 1),(12, 1, 1)");
        }

        //Prueba de humo, solo comprueba que la pagina de inicio es la correcta
        //Ejemplo de assert simples sobre el contenido raw de una pagina
        [TestMethod()]
        public virtual void TestDemoWebSmoke()
        {
            sm.GetLogger().Info("-- Call url: " + WebTestBase.WebMain);
            sm.Driver.Url = WebTestBase.WebMain; //webRoot se ha configurado en AssemblySetUp
            sm.Watermark(); //pone el nombre del test en la pagina
            System.Threading.Thread.Sleep(1500);
            sm.Screenshot("TestDemoWeb-SampleImage"); //definida en la clase base para tomar una imagen de la pantalla

            //comprobacion del contenido general de parte del contenido de una pagina (en body)
            IWebElement pageBody = sm.Driver.FindElement(By.TagName("body"));
            string TextBody = pageBody.Text;
            StringAssert.Contains(TextBody, "Flight Plans");

            //idem buscando un tag (head)
            IWebElement pageHead = sm.Driver.FindElement(By.TagName("head"));
            string TextHead = pageHead.GetAttribute("innerText");
            StringAssert.Contains(TextHead, "FireDrone");

            //una espera solo para que se pueda ver el contenido de la pagina en el video.
            //Quitar estas esperas en los tests
            System.Threading.Thread.Sleep(1500);
        }

        //Ejemplo de un test que falla la primera vez que se ejecuta (simulando un flaky test)
        //utilizando un atributo especifico que se encarga de controlar las repeticiones
        private static int repeat = 0; //hara que falle la primera y segunda vez que se ejecuta

        //[RetryTestMethod(3)]
        [Retry(3)][TestMethod()]
        public virtual void TestDemoWebFlaky()
        {
            repeat++;
            sm.Driver.Url = "https://epigijon.uniovi.es";
            string actual = sm.Driver.FindElement(By.TagName("body")).Text;
            string substring = repeat == 2 ? "EPI" : "string no contenido en pagina"; //la tercera vez no falla
            sm.GetLogger().Info("-- Este test debera fallar la primera vez y luego pasar");
            StringAssert.Contains(actual, substring);
        }

    }
}
