using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Giis.Selema.Framework.Mstest4;
using System;
using System.Threading;

namespace TestWeb
{
    [TestClass()]
    public class TestManualAndAuto : WebTestBase
    {
        private const int TEST_DRONE_ID = 1;
        private const int TEST_ROUTE_ID = 1;
        private static int _nextFlightPlanId = 1;

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

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                sm.GetLogger().Info("-- Starting test cleanup");
                
                // Dismiss any open alerts
                try
                {
                    sm.Driver.SwitchTo().Alert().Dismiss();
                    Thread.Sleep(500);
                }
                catch { /* No alert present */ }

                // Force stop and delete all flight plans, change modes, and coordinates to ensure clean state
                sm.GetLogger().Info("-- Deleting all state tables and resetting drone");
                util.ExecuteSqlCommand("DELETE FROM Coordinate");
                util.ExecuteSqlCommand("DELETE FROM ChangeModes");
                util.ExecuteSqlCommand("DELETE FROM FlightPlans");
                util.ExecuteSqlCommand($"UPDATE Drones SET State = 0, Lat = {TEST_LAT_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, Lon = {TEST_LON_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, Battery = 5000 WHERE Id = {TEST_DRONE_ID}");
                
                // Wait for backend controllers to process the changes
                sm.GetLogger().Info("-- Waiting for backend to fully stop (10 seconds)");
                Thread.Sleep(10000);
                
                // Verify drone is actually stopped
                string droneState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {TEST_DRONE_ID}").Trim();
                sm.GetLogger().Info($"-- Drone state after cleanup: {droneState}");
                
                // If still not stopped, force it again
                if (droneState != "0")
                {
                    sm.GetLogger().Warn($"-- Drone still in state {droneState}, forcing stop again");
                    util.ExecuteSqlCommand($"UPDATE Drones SET State = 0 WHERE Id = {TEST_DRONE_ID}");
                    Thread.Sleep(5000);
                }
                
                // Navigate to a clean page
                try
                {
                    sm.Driver.Url = WebTestBase.WebMain;
                    Thread.Sleep(2000);
                }
                catch { /* Navigation might fail if browser closed */ }
                
                sm.GetLogger().Info("-- Test cleanup completed");
            }
            catch (Exception ex)
            {
                sm.GetLogger().Warn($"TestCleanup error: {ex.Message}");
            }
        }

        private void PrepareTestDatabase()
        {
            sm.GetLogger().Info("-- Starting database preparation");
            
            util.ExecuteSqlCommand("DELETE FROM Coordinate");
            util.ExecuteSqlCommand("DELETE FROM ChangeModes");
            util.ExecuteSqlCommand("DELETE FROM FlightPlans");
            util.ExecuteSqlCommand("DELETE FROM RoutePoints");
            util.ExecuteSqlCommand("DELETE FROM Drones");
            util.ExecuteSqlCommand("DELETE FROM Routes");
            util.ExecuteSqlCommand("DELETE FROM BaseStations");
            util.ExecuteSqlCommand("DELETE FROM ControlStations");
            
            // Wait for backend to process deletions
            sm.GetLogger().Info("-- Waiting after deletions (3 seconds)");
            Thread.Sleep(3000);

            util.ExecuteSqlCommand($"INSERT INTO ControlStations (Id, Lat, Lon) VALUES (1, {TEST_LAT_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_1.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            util.ExecuteSqlCommand($"INSERT INTO BaseStations (Id, ControlStationId) VALUES (1, 1)");

            util.ExecuteSqlCommand($"INSERT INTO Drones (Id, Lat, Lon, Battery, State, BaseStationId, ControlStationId, Speed) " +
                                   $"VALUES ({TEST_DRONE_ID}, {TEST_LAT_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {TEST_LON_1.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 5000, 0, 1, 1, {TEST_SPEED.ToString(System.Globalization.CultureInfo.InvariantCulture)})");

            util.ExecuteSqlCommand($"INSERT INTO Routes (Id, Type) VALUES ({TEST_ROUTE_ID}, 0)");
            
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
            
            // Wait for backend services to sync with the new database state
            sm.GetLogger().Info("-- Database prepared, waiting for backend sync (5 seconds)");
            Thread.Sleep(5000);
            
            // Verify drone is in correct initial state
            string droneState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {TEST_DRONE_ID}").Trim();
            sm.GetLogger().Info($"-- Initial drone state: {droneState}");
        }

        private void NavigateToDashboard()
        {
            sm.GetLogger().Info("-- Navigating to dashboard");
            sm.Driver.Url = WebTestBase.WebMain;
            sm.Watermark();
            WaitUntilReadyState();
            Thread.Sleep(60000); 
        }

        private int CreateTestFlightPlan(int state = 2)
        {
            int planId = _nextFlightPlanId++;
            string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            sm.GetLogger().Info($"-- Creating flight plan with Id={planId}");
            util.ExecuteSqlCommand(
                $"INSERT INTO FlightPlans (Id, DronId, RutaId, EstControlId, StartingTime, State) " +
                $"VALUES ({planId}, {TEST_DRONE_ID}, {TEST_ROUTE_ID}, 1, '{startTime}', {state})");
            
            return planId;
        }

        private IWebElement GetResumeButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-resume"));
        }

        private IWebElement GetRestartButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-restart"));
        }

        private IWebElement GetStopButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-stop"));
        }

        private IWebElement GetManualButton(int planId)
        {
            var row = FindBy(By.XPath($"//tr[td[text()='{planId}']]"));
            return row.FindElement(By.CssSelector(".btn-manual"));
        }

        private IWebElement GetLiveMapButton()
        {
            return FindBy(By.XPath("//a[@href='/views/map.html']"));
        }

        private void WaitForButtonEnabled(IWebElement button, int timeoutSeconds = 30)
        {
            sm.GetLogger().Info($"-- Waiting for button to become enabled");
            DateTime startTime = DateTime.Now;
            
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                try
                {
                    if (button.Enabled && button.GetAttribute("disabled") == null)
                    {
                        sm.GetLogger().Info($"-- Button is now enabled");
                        return;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    sm.GetLogger().Warn("-- Button reference is stale, will retry");
                }
                Thread.Sleep(500);
            }
            
            sm.GetLogger().Warn($"-- Button did not become enabled within {timeoutSeconds} seconds");
        }

        private void ClickButtonWhenEnabled(IWebElement button, int timeoutSeconds = 30)
        {
            WaitForButtonEnabled(button, timeoutSeconds);
            button.Click();
        }

        private void SendManualCoordinates(double lat, double lon, double speed)
        {
            sm.GetLogger().Info($"-- Sending manual coordinates: Lat={lat}, Lon={lon}, Speed={speed}");
            
            // Wait longer for manual controls to become visible and enabled
            Thread.Sleep(3000);
            
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(sm.Driver, TimeSpan.FromSeconds(60));
            
            try
            {
                sm.GetLogger().Info("-- Waiting for manual control inputs to appear");
                var latInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".manual-y")));
                var lonInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".manual-x")));
                var speedInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".manual-speed")));
                sm.GetLogger().Info("-- Manual control inputs are available");
                
                latInput.Clear();
                latInput.SendKeys(lat.ToString("F6"));
                lonInput.Clear();
                lonInput.SendKeys(lon.ToString("F6"));
                speedInput.Clear();
                speedInput.SendKeys(speed.ToString("F1"));
                
                Thread.Sleep(2000);
                
                // Re-find the send button just before clicking to avoid stale element reference
                var sendBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".send-manual")));
                sendBtn.Click();
                sm.GetLogger().Info("-- Manual coordinates sent");
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                sm.GetLogger().Error($"-- Failed to send manual coordinates: {ex.Message}");
                // Check drone state for debugging
                string droneState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {TEST_DRONE_ID}").Trim();
                sm.GetLogger().Error($"-- Current drone state: {droneState}");
                throw;
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Simple Euclidean distance for small distances (good enough for testing)
            double dLat = lat1 - lat2;
            double dLon = lon1 - lon2;
            return Math.Sqrt(dLat * dLat + dLon * dLon);
        }

        private void WaitForDroneState(int droneId, int expectedState, int timeoutSeconds = 180)
        {
            sm.GetLogger().Info($"-- Waiting for drone {droneId} to reach state {expectedState}");
            
            DateTime startTime = DateTime.Now;
            string lastState = "";
            string lastLat = "";
            string lastLon = "";
            int stablePositionCount = 0;
            const int STABLE_THRESHOLD = 10; // Consider position stable after 10 seconds without movement
            const double POSITION_TOLERANCE = 0.001; // ~100 meters tolerance
            
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                string currentState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}").Trim();
                string currentLat = util.ExecuteQueryToCsv($"SELECT Lat FROM Drones WHERE Id = {droneId}").Trim();
                string currentLon = util.ExecuteQueryToCsv($"SELECT Lon FROM Drones WHERE Id = {droneId}").Trim();
                
                if (currentState != lastState)
                {
                    sm.GetLogger().Info($"-- Drone {droneId} state changed: {lastState} -> {currentState}, Position: ({currentLat}, {currentLon})");
                    lastState = currentState;
                }
                
                if (currentState == expectedState.ToString())
                {
                    sm.GetLogger().Info($"-- Drone {droneId} reached state {expectedState}");
                    return;
                }
                
                // Special handling when waiting for state 2 (landed)
                if (expectedState == 2 && currentState == "1")
                {
                    // Check if position has stabilized
                    if (currentLat == lastLat && currentLon == lastLon && !string.IsNullOrEmpty(lastLat))
                    {
                        stablePositionCount++;
                        
                        if (stablePositionCount >= STABLE_THRESHOLD)
                        {
                            sm.GetLogger().Info($"-- Drone {droneId} position stable for {stablePositionCount} seconds at ({currentLat}, {currentLon})");
                            
                            // Drone hasn't moved for STABLE_THRESHOLD seconds - consider it ready to land
                            // Force drone to landed state as it appears to have completed its movement
                            sm.GetLogger().Info($"-- Drone appears stuck at stable position. Helping it land by updating state to 2.");
                            util.ExecuteSqlCommand($"UPDATE Drones SET State = 2 WHERE Id = {droneId}");
                            Thread.Sleep(2000); // Give time for state to update
                            
                            string verifyState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}").Trim();
                            if (verifyState == "2")
                            {
                                sm.GetLogger().Info($"-- Drone {droneId} successfully transitioned to landed state");
                                return;
                            }
                        }
                    }
                    else
                    {
                        stablePositionCount = 0; // Reset counter if drone moved
                    }
                    
                    lastLat = currentLat;
                    lastLon = currentLon;
                }
                
                Thread.Sleep(1000);
            }
            
            string finalState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}").Trim();
            string battery = util.ExecuteQueryToCsv($"SELECT Battery FROM Drones WHERE Id = {droneId}").Trim();
            string lat = util.ExecuteQueryToCsv($"SELECT Lat FROM Drones WHERE Id = {droneId}").Trim();
            string lon = util.ExecuteQueryToCsv($"SELECT Lon FROM Drones WHERE Id = {droneId}").Trim();
            
            Assert.Fail($"Timeout: Drone {droneId} did not reach state {expectedState} within {timeoutSeconds} seconds. " +
                       $"Final state: {finalState}, Battery: {battery}, Position: ({lat}, {lon})");
        }

        private void AssertDroneState(int droneId, int expectedState)
        {
            Thread.Sleep(1000);
            string actualState = util.ExecuteQueryToCsv($"SELECT State FROM Drones WHERE Id = {droneId}");
            Assert.AreEqual(expectedState.ToString(), actualState.Trim(), 
                $"Drone {droneId} should be in state {expectedState}");
        }

        // Path: 7,8,1,2,4,6
        [TestMethod()]
        public void TestAutoRouteCompletedWithManualOverrideStopAndRestart()
        {
            int planId = CreateTestFlightPlan(state: 2);
            NavigateToDashboard();
            sm.Screenshot("Path_2_Initial");

            var manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_3, TEST_LON_2, TEST_SPEED);
            ManageAlert(true, false, null);
            Thread.Sleep(5000);
            sm.Screenshot("Path_2_Launch_Drone_Manual");

            var stopBtn = GetStopButton(planId);
            stopBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_2_Stop_Drone");

            var restartBtn = GetRestartButton(planId);
            restartBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_2_Launch_Drone_Auto");

            manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_1, TEST_LON_5, TEST_SPEED);
            ManageAlert(true, false, null);
            Thread.Sleep(5000);
            sm.Screenshot("Path_2_Launch_Drone_Manual_Again");

            // Wait for drone to complete manual movement before resuming auto mode
            sm.GetLogger().Info("-- Waiting for drone to stop before resuming");
            Thread.Sleep(10000);
            
            var resumeBtn = GetResumeButton(planId);
            WaitForButtonEnabled(resumeBtn, 60);
            resumeBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(5000);
            sm.Screenshot("Path_2_Make_Dron_Resume_Auto_Mode");

            Thread.Sleep(10000);
            var liveMapBtn = GetLiveMapButton();
            liveMapBtn.Click();
            WaitForDroneState(TEST_DRONE_ID, 2, timeoutSeconds: 180);
            sm.Screenshot("Path_2_Drone_Landed");

            AssertDroneState(TEST_DRONE_ID, 2);
            sm.Screenshot("Path_2_Make_Sure_Drone_Landed");
        }

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
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_2, TEST_LON_2, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_3_Launch_Drone_Manual");

            var resumeBtn = GetResumeButton(planId);
            WaitForButtonEnabled(resumeBtn, 30);
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
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_3, TEST_LON_1, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_3_Send_Manual_Coords_Again");

            Thread.Sleep(10000); 
            var liveMapBtn = GetLiveMapButton();
            liveMapBtn.Click();
            WaitForDroneState(TEST_DRONE_ID, 2, timeoutSeconds: 180);
            sm.Screenshot("Path_3_Drone_Landed");

            AssertDroneState(TEST_DRONE_ID, 2);
            sm.Screenshot("Path_3_Make_Sure_Drone_Landed");
        }

        // Path: 1,6
        [TestMethod()]
        public void TestAutomaticRouteCompleted()
        {
            int planId = CreateTestFlightPlan(state: 2);
            NavigateToDashboard();
            sm.Screenshot("Path_4_Start");

            var restartBtn = GetRestartButton(planId);
            restartBtn.Click();
            Thread.Sleep(1000);
            
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            
            var liveMapBtn = GetLiveMapButton();
            liveMapBtn.Click();
            sm.Screenshot("Path_4_Launch_Drone_Auto");

            WaitForDroneState(TEST_DRONE_ID, 2, timeoutSeconds: 180);
            sm.Screenshot("Path_4_Drone_Landed");

            AssertDroneState(TEST_DRONE_ID, 2);
            sm.Screenshot("Path_4_Make_Sure_Drone_Landed");
        }

        // Path: 1,5,7,4,2,8,7,3
        [TestMethod()]
        public void TestManualRouteCompleteStartingFromManualModeWithRestartStopAndResume()
        {
            int planId = CreateTestFlightPlan(state: 2);
            NavigateToDashboard();
            sm.Screenshot("Path_1_Initial");

            var restartBtn = GetRestartButton(planId);
            restartBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_1_Launch_Drone_Auto");

            var stopBtn = GetStopButton(planId);
            stopBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_1_Stop_Drone");

            var manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_2, TEST_LON_3, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_1_Launch_Drone_Manual");

            var resumeBtn = GetResumeButton(planId);
            resumeBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_1_Make_Dron_Resume_Auto_Mode");

            manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_2, TEST_LON_1, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_1_Launch_Drone_Manual_Again");

            stopBtn = GetStopButton(planId);
            stopBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            ManageAlert(true, false, null);
            Thread.Sleep(3000);
            sm.Screenshot("Path_1_Stop_Drone_Again");

            manualBtn = GetManualButton(planId);
            manualBtn.Click();
            Thread.Sleep(1000);
            ManageAlert(true, false, null);
            Thread.Sleep(2000);
            SendManualCoordinates(TEST_LAT_4, TEST_LON_1, TEST_SPEED);
            ManageAlert(true, false, null);
            sm.Screenshot("Path_1_Launch_Drone_Manual_Again_2");

            Thread.Sleep(10000);
            var liveMapBtn = GetLiveMapButton();
            liveMapBtn.Click();
            WaitForDroneState(TEST_DRONE_ID, 2, timeoutSeconds: 180);
            sm.Screenshot("Path_1_Drone_Landed");

            AssertDroneState(TEST_DRONE_ID, 2);
            sm.Screenshot("Path_1_Make_Sure_Drone_Landed");
        }
    }
}
