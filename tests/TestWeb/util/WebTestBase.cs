using Giis.Portable.Util;
using Giis.Selema.Framework.Mstest4;
using Giis.Selema.Manager;
using Giis.Selema.Services.Browser;
using Giis.Selema.Services.Impl;
using Giis.Visualassert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TestUnit;

namespace TestWeb
{
    /// <summary>
    /// Clase base para todos los test ent-to-end realizados con selenium.
    /// Provee de metodos estandar de inicializacion y finalizacion de las pruebas que gestionan el contexto e
    /// instancian objetos de utilidad (al igual que UnitTestBase):
    /// -context: el contexto de BD para las pruebas
    /// -util: algunas utilidades para manejar la base de datos
    /// -sm: componente SeleniumManager para gestionar el ciclo de vida de del driver y proporcionar logs 
    ///  (ver https://github.com/javiertuya/selema)
    ///
    /// Ademas, esta clase base habra definido:
    /// -log: el logger NLog que se guardara junto con los reports
    /// -WebRoot: url de la raiz de la aplicacion
    /// -WebMain: url de la pagina principal de la aplicacion
    /// 
    /// Incorpora metodos adicionales de utilidad con algunas acciones de Selenium
    /// </summary>
    public class WebTestBase : LifecycleMstest4
    {
        //Valores leidos de la configuracion en app.properties (configurados en AssemblySetUp)
        public static string WebHost { get; set; } = "localhost"; //host donde esta la aplicacion
        public static string WebPort { get; set; } //puerto en el que escucha
        public static string WebPath { get; set; } //path de la pagina principal
        public static string WebRoot { get; set; } //url raiz de la aplicacion (host+puerto)
        public static string WebMain { get; set; } //url de la pagina principal


        protected DbTestContext context; //el contexto de la BD que debe usarse en todas las subclases
        protected DbTestUtil util; //otras utilidades de acceso a la base de datos para uso en las subclases
        protected static SeleManager sm; //getiona el ciclo de vida del driver
        protected VisualAssert va; //para comparaciones de strings largos que muestren las diferencias html

        ///<summary>
        ///Control global de la configuracion e inicializacion SeleniumManager, ejecutado automaticamente una unica vez
        ///antes de todos los tests de este proyecto
        ///</summary>
        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void SetUpThisClass(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestCtx)
        {
            WebTestBase.SetUpClass(TestCtx);
            //Configuracion basica del SeleniumManager para estos proyectos
            SelemaConfig selemaConfig = new SelemaConfig().SetProjectRoot(Config.ProjectRoot);
            sm = LifecycleMstest4.GetManager(sm, selemaConfig)
                .Add(new WatermarkService().SetDelayOnFailure(3));

            string mode;
            if (sm.GetCiService().IsLocal())
            {
                // En local utiliza los parametros definidos en app.properties
                WebHost = Config.prop.GetProperty("app.front.host");
                sm.SetBrowser(Config.prop.GetProperty("test.browser"))
                    .SetMaximize(Config.prop.GetProperty("test.browser.maximized") == "true");
                mode = "local";
            }
            else
            {
                // Cuando se ejecuta en jenkins siempre s usa chrome maximizado.
                // Puede haber diferentes modos de ejecucion de los browsers remotos
                WebHost = Parameters.GetIpV4Address();
                // Para probar drivers remotos en local
                // WebHost = "host.docker.internal";
                sm.SetBrowser("chrome");
                mode = Config.prop.GetProperty("test.remote.mode") ?? "preload";

                // las siguientes propiedades se han anyadido a app.properties al ejecutar jenkins
                //string testLabel = "chrome";
                //string remoteWebDriverUrl = "http://localhost:4444";
                //string videoControllerUrl = "http://localhost:4449/selema-video-controller";
                //string gridWebDriverUrl = "http://localhost:4444";
                string testLabel = Config.prop.GetProperty("selema.preload.label");
                string remoteWebDriverUrl = Config.prop.GetProperty("selema.preload.driver.url");
                string videoControllerUrl = Config.prop.GetProperty("selema.preload.video.controller");
                string gridWebDriverUrl = Config.prop.GetProperty("selema.grid.driver.url");
                //string gridWebDriverUrl = "http://testuser:test@localhost:4444";
                if (mode == "preload" || mode == "preload-console")
                    sm.SetDriverUrl(remoteWebDriverUrl)
                         .Add(new RemoteBrowserService().SetVideo(
                            new VideoControllerRemote(testLabel, videoControllerUrl, selemaConfig.GetReportDir())
                         ));
                else if (mode == "grid")
                    sm.SetDriverUrl(gridWebDriverUrl)
                         .Add(new DynamicGridBrowserService().SetVideo().SetVnc());
                else
                    throw new ArgumentException("Invalid test.remote.mode: " + mode);

                // Anyade argumentos para cada modo, siempre maximizado
                if (mode == "preload-console")
                    // Este modo es experimental, usa ua imagen con un perfil personalizado que arranca con la consola javasccript abierta.
                    // El problema es que es flaky cuando se obiene un screenshot poco tiempo despues de cargar la pagina,
                    // fallando algo más de la mitad de las veces. Parece que la presencia de la consola al abrir influye
                    // en el renderizado de la pagina. Esto tambien ocurre en modo grid si se configura para abrir la consola automaticamente.
                    // Aunque se use WaitUntilReadyState() sigue causando problemas, causando que se cierre la consola y la pagina quede bloqueada.
                    sm.SetArguments(["--start-maximized", "--auto-open-devtools-for-tabs", "--user-data-dir=/home/seluser/console-profile", "--profile-directory=Default"]);
                else
                    // preload y grid:
                    // En modo grid, para abrir la consola cuando se visualiza la ejecucion desde VNC, pulsar ESCAPE antes de F12
                    // para que F12 sea enviada al navegador del container y no al navegador local.
                    // Pulsando de neuvo ESCAPE se devuelve el control al navegador local.
                    sm.SetArguments(["--start-maximized"]);
           }

            //Direcciones de los puertos
            sm.GetLogger().Info("-- Browser Mode: " + mode + " -- Host: " + WebHost);
            WebPort = Config.prop.GetProperty("app.front.port");
            WebPath = Config.prop.GetProperty("app.front.path");
            WebRoot = "http://" + WebHost + ":" + WebPort;
            WebMain = WebRoot + "/" + WebPath;

            if (Config.prop.GetProperty("app.front.jscover") == "true")
                sm.Add(JsCoverService.GetInstance(WebRoot));
        }

        public WebTestBase()
        {
        }

        [TestInitialize] public override void SetUp()
        {
            base.SetUp();
            context = new DbTestContext();
            util = new DbTestUtil(context);
            va = new VisualAssert().SetNormalizeEol(true) // igual que en unitarias
                .SetReportSubdir(Path.Combine(Config.ProjectRoot, "reports"))
                .SetUseLocalAbsolutePath(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_HOME")));
            context.CleanDatabase();
        }

        [TestCleanup] public override void TearDown()
        {
            context.Dispose();
            base.TearDown();
        }

        // Carga de una pagina con repeticiones en caso de timeout
        // Usado inicialmente para solucionar el problema de timemeout que se producia con mucha frecuencia en CI al cargar js desde cloudflare (p.e. signalr.js):
        //   OpenQA.Selenium.WebDriverException: The HTTP request to the remote WebDriver server for URL http://selenoid-xmii:4444/wd/hub/session/a5ce67d58eda37371b578074b99dc11c/url timed out after 60 seconds. ---> 
        //   System.Threading.Tasks.TaskCanceledException: The request was canceled due to the configured HttpClient.Timeout of 60 seconds elapsing. ---> 
        //   System.TimeoutException: The operation was canceled. ---> System.Threading.Tasks.TaskCanceledException: The operation was canceled. ---> 
        //   System.IO.IOException: Unable to read data from the transport connection: Operation canceled. ---> 
        //   System.Net.Sockets.SocketException: Operation canceled
        //
        // Cuando un html carga un js desde clowdflare, se producia este timeout, migigado recargando varias veces la carga de esta (entre 1 y 4 reintentos la mayoria de las veces).
        // No se exactamente si es debido a la infraestructura, la forma en que dotnet envia las cabeceras de las peticiones de estas paginas, o si cloudflare realiza algun tipo de control de flujo,
        // pero he visto varios reports recientes con problemas similares a este.
        // La solucion consiste en utilizar cualquier otro CDN como jsdelver o unpkg.
        // Tras ello, no es necesario utilizar este metodo, pero queda en el codigo por si se produce alguna situacion simialr
        protected void GoUrl(string url)
        {
            int numRetries = 5;
            int loadTimeout = 10;
            int afterTimeout = 10;
            for (int i = 1; i <= numRetries; i++)
            {
                try
                {
                    sm.Driver.Manage().Timeouts().PageLoad = System.TimeSpan.FromSeconds(loadTimeout);
                    sm.Driver.Url = url;
                    break;
                }
                catch (OpenQA.Selenium.WebDriverException e)
                {
                    if (i == numRetries)
                        throw;
                    sm.GetLogger().Error("Retry " + i + " error: " + e.Message);
                    sm.QuitDriver(sm.Driver);
                    System.Threading.Thread.Sleep(afterTimeout * 1000);
                    sm.ReplaceDriver(sm.CreateDriver());
                }

            }
        }

        ////////////////////////////////////////////////////////////
        // Utilidades para realizar algunas acciones comunes de selenium 
        ////////////////////////////////////////////////////////////

        ///<summary>
        ///Accion de click de Selenium personalizada alternativa a driver.Click() usando Actions
        ///</summary>
        ///<remarks>
        ///https://stackoverflow.com/questions/44912203/selenium-web-driver-java-element-is-not-clickable-at-point-x-y-other-elem
        ///</remarks>
        public void ClickActions(IWebElement elem)
        {
            Actions actions = new Actions(sm.Driver);
            actions.MoveToElement(elem).Click().Perform();
        }
        ///<summary>
        ///Accion de click de Selenium personalizada alternativa a driver.Click() usando JavascriptExecutor
        ///</summary>
        public void ClickJavascript(IWebElement elem)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)sm.Driver;
            executor.ExecuteScript("arguments[0].click();", elem);
        }

        ///<summary>
        ///Accion de sendKeys de Selenium personalizada alternativa a driver.SendKeys() utilizando Actions
        ///</summary>
        ///<remarks>
        ///http://stackoverflow.com/questions/31547459/selenium-sendkeys-different-behaviour-for-chrome-firefox-and-safari
        ///</remarks>
        public void SendKeysActions(IWebElement elem, string value)
        {
            Actions actions = new Actions(sm.Driver);
            actions.MoveToElement(elem).SendKeys(value).Perform();
        }
        ///<summary>
        ///Accion de sendKeys de Selenium personalizada alternativa a driver.SendKeys() utilizando JavascriptExecutor
        ///</summary>
        public void SendKeysJavascript(IWebElement elem, string value)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)sm.Driver;
            executor.ExecuteScript("arguments[0].value = arguments[1];", elem, value);
        }

        ///<summary>
        ///Busca un elemento utilizando un Wait de selenium a partir del locator indicado
        ///</summary>
        public IWebElement FindBy(By locator)
        {
            var wait = new WebDriverWait(sm.Driver, GetDefaultTimeout());
            return wait.Until(drv => drv.FindElement(locator));
        }
        ///<summary>
        ///Busca un elemento utilizando un Wait de selenium con el id indicado
        ///</summary>
        public IWebElement FindById(string id)
        {
            return FindBy(By.Id(id));
        }

        ///<summary>
        ///Realiza un Wait hasta que el valor de Text del elemento contiene expected
        ///</summary>
        public void WaitUntilTextPresent(IWebElement elem, string expected)
        {
            var wait = new WebDriverWait(sm.Driver, GetDefaultTimeout());
            wait.Message = "Expected text not found in element.";
            //OpenQA.Selenium.Support.UI contiene la definicion de WebDriverWait y ExpectedConditions,
            //pero este ultimo esta deprecated, se debe usar SeleniumExtras
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TextToBePresentInElement(elem, expected));
        }

        ///<summary>
        ///Realiza un Wait hasta que el valor de Text de un dropdown contiene expected y selecciona este valor
        ///</summary>
        public void WaitAndSelectByText(IWebElement elem, string text)
        {
            var wait = new WebDriverWait(sm.Driver, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .TextToBePresentInElement(elem, text));

            var select = new SelectElement(elem);
            select.SelectByText(text);
        }

        ///<summary>
        ///Espera hasta que el valor de la ejecucion de la sql indicada sea igual a expectedCsv
        ///(reintenta continuamente hasta que se cumple esta condicion o han pasado 5 segundos)
        ///</summary>
        public void WaitUntilDatabaseValueAndAssert(string expectedCsv, string sql)
        {
            string actualCsv = null;
            for (int i = 0; i < 25; i++)
            {
                actualCsv = util.ExecuteQueryToCsv(sql);
                if (actualCsv == expectedCsv)
                    break;
                Thread.Sleep(200);
            }
            sm.VisualAssertEquals(expectedCsv, actualCsv, "");
        }

        ///<summary
        ///Espera hasta que el estado de carga de la pagina sea "complete"
        ///</summary>
        public void WaitUntilReadyState()
        {
            new WebDriverWait(sm.Driver, GetDefaultTimeout())
                .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
       }

        ///<summary>
        ///Metodo general para manejar las ventanas emergentes del navegador (alert, confirm, prompt)
        ///indicando la accion del usuario (accept, dismiss).
        ///Devuelve el texto que contiene el alert para que pueda ser usado en asserts.
        ///</summary>
        ///<remarks>
        ///No usar este metodo con PhantomJS pues no soporta estas ventanas, aunque hay un workaround en:
        ///https://stackoverflow.com/questions/29820448/how-to-handle-accept-js-alerts-in-phantomjs-using-webdriver
        ///</remarks>
        public string ManageAlert(bool accept, bool dismiss, string text)
        {
            WebDriverWait wait = new WebDriverWait(sm.Driver, GetDefaultTimeout());
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            //Tras asegurar que esta la ventana pasa a ella y realiza las acciones
            IAlert alert = sm.Driver.SwitchTo().Alert();
            string alertText = alert.Text;
            if (text != null)
                alert.SendKeys(text);
            if (accept)
                alert.Accept();
            if (dismiss)
                alert.Dismiss();
            return alertText;
        }

        //Obtiene un timestamp con el timeout por defecto para usar en los Wait de selenium
        protected TimeSpan GetDefaultTimeout()
        {
            //puede ser negativo pues en java see utiliza este valor para mejorar la eficiencia
            return new TimeSpan(0, 0, 10);
        }


    }
}
