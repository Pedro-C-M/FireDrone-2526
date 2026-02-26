using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Giis.Selema.Framework.Mstest4;
using System;
using System.Threading;

namespace TestWeb
{
    /// <summary>
    /// Test funcionales basados en la máquina de estados del dron (Modo Manual y Retorno Automático).
    /// Estrategia: Pares de caminos (pair-wise paths)
    /// 
    /// Estados:
    /// - Idle (Dron parado): Estado = Cancelled (2) o sin flight plan
    /// - ManualToPoint: Estado = Manual (3)
    /// - AutoRoute: Estado = OnCourse (0)
    /// - Landed: Estado = Completed (1) o dron aterrizado
    /// 
    /// Eventos:
    /// E1: Restart/Resume (botón Resume/Restart)
    /// E2: Manual + introducir coordenadas (botón Manual + coords)
    /// E3: Dron llega a coordenadas (evento automático)
    /// E4: Usuario pulsa Resume
    /// E5: Usuario pulsa Stop
    /// E6: Dron completa ruta (evento automático)
    /// E7: Usuario pulsa Manual durante ruta
    /// E8: Usuario pulsa Stop después de E7
    /// 
    /// Pares de caminos por estado:
    /// - Estado Idle: 5-1, 5-7, 8-7, 8-1
    /// - Estado AutoRoute: 4-2, 4-6, 4-5
    /// - Estado ManualToPoint: 7-4, 7-3, 7-8, 2-4, 2-3, 2-8
    /// </summary>
    [TestClass()]
    public class TestStateMachine : WebTestBase
    {
        // IDs de elementos en la UI
        private const int TEST_DRONE_ID = 1;
        private const int TEST_ROUTE_ID = 1;
        private const int TEST_FLIGHT_PLAN_ID = 1;

        // Coordenadas de prueba (área de Gijón)
        private const double TEST_LAT_1 = 43.5322;
        private const double TEST_LON_1 = -5.6611;
        private const double TEST_LAT_2 = 43.5400;
        private const double TEST_LON_2 = -5.6700;
        private const double TEST_LAT_3 = 43.5422;
        private const double TEST_LON_3 = -5.6711;
        private const double TEST_LAT_4 = 43.5500;
        private const double TEST_LON_4 = -5.6800;
        private const double TEST_LAT_5 = 43.5622;
        private const double TEST_LON_5 = -5.6911;
        private const double TEST_SPEED = 100.0;

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();
            PrepareTestDatabase();
        }

        /// <summary>
        /// Prepara la base de datos con datos de prueba necesarios
        /// </summary>
        private void PrepareTestDatabase()
        {
            // Limpia y prepara datos de prueba
            util.ExecuteSqlCommand("DELETE FROM FlightPlans");
            util.ExecuteSqlCommand("DELETE FROM RoutePoints");
            util.ExecuteSqlCommand("DELETE FROM Drones");
            util.ExecuteSqlCommand("DELETE FROM Routes");
            util.ExecuteSqlCommand("DELETE FROM BaseStations");
            util.ExecuteSqlCommand("DELETE FROM ControlStations");

            //Inserta datos base
            util.ExecuteSqlCommand($"INSERT INTO ControlStations (Id, Lat, Lon) VALUES (1, {TEST_LAT_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_1.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            util.ExecuteSqlCommand($"INSERT INTO BaseStations (Id, ControlStationId) VALUES (1, 1)");

            // Inserta dron de prueba con estado inicial Stopped (0) and sufficient battery
            util.ExecuteSqlCommand($"INSERT INTO Drones (Id, Lat, Lon, Battery, State, BaseStationId, ControlStationId, Speed) " +
                                   $"VALUES ({TEST_DRONE_ID}, {TEST_LAT_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 5000, 0, 1, 1, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");

            // Inserta ruta de prueba
            util.ExecuteSqlCommand($"INSERT INTO Routes (Id, Type) VALUES ({TEST_ROUTE_ID}, 0)");
            
            // Inserta puntos de ruta
            util.ExecuteSqlCommand($"INSERT INTO RoutePoints (RouteId, Lat, Long, Height, Velocity) " +
                                   $"VALUES ({TEST_ROUTE_ID}, {TEST_LAT_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 10.0, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            util.ExecuteSqlCommand($"INSERT INTO RoutePoints (RouteId, Lat, Long, Height, Velocity) " +
                                   $"VALUES ({TEST_ROUTE_ID}, {TEST_LAT_2.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_2.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 10.0, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            util.ExecuteSqlCommand($"INSERT INTO RoutePoints (RouteId, Lat, Long, Height, Velocity) " +
                                   $"VALUES ({TEST_ROUTE_ID}, {TEST_LAT_3.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_3.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 10.0, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            util.ExecuteSqlCommand($"INSERT INTO RoutePoints (RouteId, Lat, Long, Height, Velocity) " +
                                   $"VALUES ({TEST_ROUTE_ID}, {TEST_LAT_4.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_4.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 10.0, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            util.ExecuteSqlCommand($"INSERT INTO RoutePoints (RouteId, Lat, Long, Height, Velocity) " +
                                   $"VALUES ({TEST_ROUTE_ID}, {TEST_LAT_5.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_5.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 10.0, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
        }

        /// <summary>
        /// Navega a la página principal del dashboard
        /// </summary>
        private void NavigateToDashboard()
        {
            sm.GetLogger().Info("-- Navigating to dashboard");
            sm.Driver.Url = WebTestBase.WebMain;
            sm.Watermark();
            WaitUntilReadyState();
            Thread.Sleep(45000); // Espera más tiempo para que se carguen y rendericen los datos
        }

        /// <summary>
        /// Crea un flight plan de prueba con estado específico
        /// </summary>
        private int CreateTestFlightPlan(int state = 2)
        {
            int planId = TEST_FLIGHT_PLAN_ID;
            string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            util.ExecuteSqlCommand(
                $"INSERT INTO FlightPlans (Id, DronId, RutaId, EstControlId, StartingTime, State) " +
                $"VALUES ({planId}, {TEST_DRONE_ID}, {TEST_ROUTE_ID}, 1, '{startTime}', {state})");
            
            return planId;
        }

        /// <summary>
        /// Obtiene el botón Resume de un flight plan
        /// </summary>
        private IWebElement GetResumeButton(int planId)
        {
            // Busca la fila del flight plan y luego el botón Resume
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-resume"));
        }

        /// <summary>
        /// Obtiene el botón Restart de un flight plan
        /// </summary>
        private IWebElement GetRestartButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-restart"));
        }

        /// <summary>
        /// Obtiene el botón Stop de un flight plan
        /// </summary>
        private IWebElement GetStopButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-stop"));
        }

        /// <summary>
        /// Obtiene el botón Manual de un flight plan
        /// </summary>
        private IWebElement GetManualButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-manual"));
        }

        /// <summary>
        /// Obtiene el botón Live Map del menú de navegación
        /// </summary>
        private IWebElement GetLiveMapButton()
        {
            return FindBy(By.XPath("//a[@href='/views/map.html']"));
        }

        /// <summary>
        /// Envía coordenadas manuales para un flight plan
        /// </summary>
        private void SendManualCoordinates(double lat, double lon, double speed)
        {
            sm.GetLogger().Info($"-- Sending manual coordinates: Lat={lat}, Lon={lon}, Speed={speed}");
            
            // Espera a que aparezca el formulario manual usando FindBy
            var latInput = FindBy(By.CssSelector(".manual-y"));
            var lonInput = FindBy(By.CssSelector(".manual-x"));
            var speedInput = FindBy(By.CssSelector(".manual-speed"));
            var sendBtn = FindBy(By.CssSelector(".send-manual"));

            latInput.Clear();
            latInput.SendKeys(lat.ToString("F6"));
            lonInput.Clear();
            lonInput.SendKeys(lon.ToString("F6"));
            speedInput.Clear();
            speedInput.SendKeys(speed.ToString("F1"));
            
            Thread.Sleep(500);
            sendBtn.Click();
        }

        /// <summary>
        /// Espera hasta que el dron alcance un estado específico
        /// </summary>
        private void WaitForDroneState(int droneId, int expectedState, int timeoutSeconds = 180)
        {
            sm.GetLogger().Info($"-- Waiting for drone {droneId} to reach state {expectedState}");
            
            DateTime startTime = DateTime.Now;
            string lastState = "";
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                string currentState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}").Trim();
                
                // Log state changes to help diagnose issues
                if (currentState != lastState)
                {
                    sm.GetLogger().Info($"-- Drone {droneId} state changed: {lastState} -> {currentState}");
                    lastState = currentState;
                }
                
                if (currentState == expectedState.ToString())
                {
                    sm.GetLogger().Info($"-- Drone {droneId} reached state {expectedState}");
                    return;
                }
                Thread.Sleep(1000); // Check every second instead of every 2 seconds
            }
            
            // Provide detailed failure information
            string finalState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}").Trim();
            string battery = util.ExecuteQueryToCsv($"SELECT Battery FROM Drones WHERE Id = {droneId}").Trim();
            string lat = util.ExecuteQueryToCsv($"SELECT Lat FROM Drones WHERE Id = {droneId}").Trim();
            string lon = util.ExecuteQueryToCsv($"SELECT Lon FROM Drones WHERE Id = {droneId}").Trim();
            
            Assert.Fail($"Timeout: Drone {droneId} did not reach state {expectedState} within {timeoutSeconds} seconds. " +
                       $"Final state: {finalState}, Battery: {battery}, Position: ({lat}, {lon})");
        }

        /// <summary>
        /// Espera hasta que el flight plan alcance un estado específico
        /// </summary>
        private void WaitForFlightPlanState(int planId, int expectedState, int timeoutSeconds = 180)
        {
            sm.GetLogger().Info($"-- Waiting for flight plan {planId} to reach state {expectedState}");
            
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                string currentState = util.ExecuteQueryToCsv($"SELECT State FROM FlightPlans WHERE Id = {planId}").Trim();
                if (currentState == expectedState.ToString())
                {
                    sm.GetLogger().Info($"-- Flight plan {planId} reached state {expectedState}");
                    return;
                }
                Thread.Sleep(2000); // Verifica cada 2 segundos
            }
            
            Assert.Fail($"Timeout: Flight plan {planId} did not reach state {expectedState} within {timeoutSeconds} seconds");
        }

        /// <summary>
        /// Verifica el estado de un flight plan
        /// </summary>
        private void AssertFlightPlanState(int planId, int expectedState)
        {
            Thread.Sleep(1000);
            string actualState = util.ExecuteQueryToCsv($"SELECT State FROM FlightPlans WHERE Id = {planId}");
            Assert.AreEqual(expectedState.ToString(), actualState.Trim(), 
                $"Flight plan {planId} should be in state {expectedState}");
        }

        /// <summary>
        /// Verifica el estado de un dron
        /// </summary>
        private void AssertDroneState(int droneId, int expectedState)
        {
            Thread.Sleep(1000);
            string actualState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}");
            Assert.AreEqual(expectedState.ToString(), actualState.Trim(), 
                $"Drone {droneId} should be in state {expectedState}");
        }

        // [TestMethod()]
        // public void TestPath_1_5_7_4_2_8_7_3()
        // {
        //     int planId = CreateTestFlightPlan(state: 2);
        //     NavigateToDashboard();
        //     sm.Screenshot("Path_1_Initial");

        //     var restartBtn = GetRestartButton(planId);
        //     restartBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_1_Step1");

        //     var stopBtn = GetStopButton(planId);
        //     stopBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_1_Step5");

        //     restartBtn = GetRestartButton(planId);
        //     restartBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);

        //     var manualBtn = GetManualButton(planId);
        //     manualBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(1000);
        //     SendManualCoordinates(TEST_LAT_2, TEST_LON_2, TEST_SPEED);
        //     sm.Screenshot("Path_1_Step7");

        //     var resumeBtn = GetResumeButton(planId);
        //     resumeBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_1_Step4");

        //     manualBtn = GetManualButton(planId);
        //     manualBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(1000);
        //     SendManualCoordinates(TEST_LAT_1, TEST_LON_1, TEST_SPEED);
        //     sm.Screenshot("Path_1_Step2");

        //     stopBtn = GetStopButton(planId);
        //     stopBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_1_Step8");

        //     restartBtn = GetRestartButton(planId);
        //     restartBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);

        //     manualBtn = GetManualButton(planId);
        //     manualBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(1000);
        //     SendManualCoordinates(TEST_LAT_2, TEST_LON_2, TEST_SPEED);
        //     sm.Screenshot("Path_1_Step7_2");

        //     sm.Screenshot("Path_1_Final");
        // }

        // [TestMethod()]
        // public void TestPath_7_8_1_2_4_6()
        // {
        //     int planId = CreateTestFlightPlan(state: 2);
        //     NavigateToDashboard();
        //     sm.Screenshot("Path_2_Initial");

        //     var restartBtn = GetRestartButton(planId);
        //     restartBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);

        //     var manualBtn = GetManualButton(planId);
        //     manualBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(1000);
        //     SendManualCoordinates(TEST_LAT_2, TEST_LON_2, TEST_SPEED);
        //     sm.Screenshot("Path_2_Step7");

        //     var stopBtn = GetStopButton(planId);
        //     stopBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_2_Step8");

        //     restartBtn = GetRestartButton(planId);
        //     restartBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_2_Step1");

        //     manualBtn = GetManualButton(planId);
        //     manualBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(1000);
        //     SendManualCoordinates(TEST_LAT_1, TEST_LON_1, TEST_SPEED);
        //     sm.Screenshot("Path_2_Step2");

        //     var resumeBtn = GetResumeButton(planId);
        //     resumeBtn.Click();
        //     Thread.Sleep(1000);
        //     ManageAlert(true, false, null);
        //     Thread.Sleep(3000);
        //     sm.Screenshot("Path_2_Step4");

        //     sm.Screenshot("Path_2_Final");
        // }

        // Path: 1,2,4,5,1,2,3
        [TestMethod()]
        public void TestManualRouteCompletedWithRestartResumeAndStop()
        {
            int planId = CreateTestFlightPlan(state: 2);
            NavigateToDashboard();
            sm.Screenshot("Path_3_Initial");

            var restartBtn = GetRestartButton(planId);
            restartBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_3_Launch_Drone_Auto");

            var manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(1000);
            SendManualCoordinates(TEST_LAT_2, TEST_LON_2, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_3_Launch_Drone_Manual");

            var resumeBtn = GetResumeButton(planId);
            resumeBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_3_Make_Dron_Resume_Auto_Mode");

            var stopBtn = GetStopButton(planId);
            stopBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_3_Stop_Drone");

            restartBtn = GetRestartButton(planId);
            restartBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_3_Launch_Drone_Auto_Again");

            manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(1000);
            SendManualCoordinates(TEST_LAT_3, TEST_LON_1, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_3_Send_Manual_Coords_Again");

            var liveMapBtn = GetLiveMapButton();
            liveMapBtn.Click();
            WaitForDroneState(TEST_DRONE_ID, 2, timeoutSeconds: 120);
            sm.Screenshot("Path_3_Drone_Landed");

            AssertDroneState(TEST_DRONE_ID, 2);
            sm.Screenshot("Path_3_Make_Sure_Drone_Landed");
        }

        // Path: 1,6
        // [TestMethod()]
        // public void TestAutomaticRouteCompleted()
        // {
        //     int planId = CreateTestFlightPlan(state: 2);
        //     NavigateToDashboard();
        //     sm.Screenshot("Path_4_Start");

        //     var restartBtn = GetRestartButton(planId);
        //     restartBtn.Click();
        //     Thread.Sleep(1000);
            
        //     ManageAlert(true, false, null);
        //     ManageAlert(true, false, null);
            
        //     var liveMapBtn = GetLiveMapButton();
        //     liveMapBtn.Click();
        //     sm.Screenshot("Path_4_Launch_Drone_Auto");

        //     WaitForDroneState(TEST_DRONE_ID, 2, timeoutSeconds: 300);
        //     sm.Screenshot("Path_4_Drone_Landed");

        //     AssertDroneState(TEST_DRONE_ID, 2);
        //     sm.Screenshot("Path_4_Make_Sure_Drone_Landed");
        // }
    }
}
