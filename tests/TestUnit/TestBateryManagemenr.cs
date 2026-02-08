using DroneController;
using DroneController.Drone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace TestUnit
{
    [TestClass]
    public class TestBateryManagement
    {
        private DroneSimulator _droneSimulator = null!;
        private Mock<IDroneCallback> _callbackMock = null!;

        // Long route waypoints to ensure simulation doesn't arrive early
        private static readonly Waypoint[] LongRouteWaypoints = new[]
        {
            new Waypoint { Latitude = 43.36, Longitude = -5.84, Altitude = 50, Speed = 20 },
            new Waypoint { Latitude = 44.36, Longitude = -6.84, Altitude = 55, Speed = 20 }
        };

        // Short route waypoints to reach destination quickly
        private static readonly Waypoint[] ShortRouteWaypoints = new[]
        {
            new Waypoint { Latitude = 43.36, Longitude = -5.84, Altitude = 50, Speed = 1000 },
            new Waypoint { Latitude = 43.36001, Longitude = -5.84001, Altitude = 50, Speed = 1000 }
        };

        [TestInitialize]
        public void Setup()
        {
            _droneSimulator = new DroneSimulator();
            _callbackMock = new Mock<IDroneCallback>();
            _droneSimulator.SetUpdateCallback(_callbackMock.Object);
        }


        // CP1
        [TestMethod]
        public void Battery1_Flying_NotArrived_BatteryZero_SimulationStops_AlarmGenerated()
        {
            _droneSimulator.InitialBattery = 1;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(0, status.Battery, "Battery should be 0 after decrement");
            Assert.IsFalse(continueSimulation, "Simulation should stop when battery reaches 0");
            Assert.AreEqual(0, status.Altitude, "Altitude should be 0 when battery depleted");
            Assert.AreEqual(0, status.Speed, "Speed should be 0 when battery depleted");
            Assert.IsTrue(status.IsAlarm, "IsAlarm should be true");
            Assert.AreEqual(AlarmType.BatteryDepleted, status.AlarmType, "AlarmType should be BatteryDepleted");

            _callbackMock.Verify(c => c.OnAlarm(
                It.Is<DroneStatus>(s => s.Battery == 0 && s.IsAlarm),
                AlarmType.BatteryDepleted),
                Times.Once(),
            "OnAlarm should be invoked with BatteryDepleted when battery reaches 0");
        }

        // CP2
        [TestMethod]
        public void Battery2_Flying_NotArrived_Battery1_SimulationContinues()
        {
            _droneSimulator.InitialBattery = 2;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);


            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(1, status.Battery, "Battery should be 1 after decrement");
            Assert.IsTrue(continueSimulation, "Simulation should continue with battery > 0");
            Assert.AreEqual(DroneState.Flying, status.State, "State should remain Flying");
            Assert.IsTrue(status.IsAlarm, "IsAlarm should be true");
            Assert.AreEqual(AlarmType.LowBattery, status.AlarmType, "AlarmType should be LowBattery");

            _callbackMock.Verify(c => c.OnAlarm(
                It.Is<DroneStatus>(s => s.Battery == 1 && s.IsAlarm),
                AlarmType.LowBattery),
                Times.Once(),
            "OnAlarm should be invoked with LowBattery when battery is below 10% threshold");
        }

        // CP3
        [TestMethod]
        public void Battery10_Flying_NotArrived_Battery9_SimulationContinues()
        {

            _droneSimulator.InitialBattery = 10;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(9, status.Battery, "Battery should be 9 after decrement");
            Assert.IsTrue(continueSimulation, "Simulation should continue");
            Assert.IsTrue(status.IsAlarm, "IsAlarm should be true (below 10% threshold)");
            Assert.AreEqual(AlarmType.LowBattery, status.AlarmType, "AlarmType should be LowBattery");

            _callbackMock.Verify(c => c.OnAlarm(
                It.Is<DroneStatus>(s => s.Battery == 9 && s.IsAlarm),
                AlarmType.LowBattery),
                Times.Once(),
            "OnAlarm should be invoked with LowBattery when battery is below 10% threshold");
        }

        // CP4
        [TestMethod]
        public void Battery11_Flying_NotArrived_Battery10_SimulationContinues()
        {
            _droneSimulator.InitialBattery = 11;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(10, status.Battery, "Battery should be 10 after decrement");
            Assert.IsTrue(continueSimulation, "Simulation should continue");
            Assert.IsTrue(status.IsAlarm, "IsAlarm should be true (at/below 10% threshold)");
            Assert.AreEqual(AlarmType.LowBattery, status.AlarmType, "AlarmType should be LowBattery");

            _callbackMock.Verify(c => c.OnAlarm(
                It.Is<DroneStatus>(s => s.Battery == 10 && s.IsAlarm),
                AlarmType.LowBattery),
                Times.Once(),
                "OnAlarm should be invoked with LowBattery when battery is at/below 10% threshold");
        }

        // CP5
        [TestMethod]
        public void Battery500_Flying_Arrived_Battery499_StateLanded_SimulationStops()
        {

            _droneSimulator.InitialBattery = 500;
            _droneSimulator.StartSimulation(ShortRouteWaypoints, isPeriodic: false);

            bool continueSimulation = true;
            int steps = 0;
            int maxSteps = 500;

            while (continueSimulation && steps < maxSteps)
            {
                continueSimulation = _droneSimulator.StepSimulation();
                steps++;
            }

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(DroneState.Landed, status.State, "State should be Landed after arrival");
            Assert.IsFalse(continueSimulation, "Simulation should stop after arrival on simple route");
            Assert.IsTrue(status.Battery < 500, "Battery should have decreased");
        }

        // CP6
        [TestMethod]
        public void Battery1_Flying_Arrived_Battery0_Landed_AlarmGenerated()
        {

            _droneSimulator.InitialBattery = 1;
            _droneSimulator.StartSimulation(ShortRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(0, status.Battery, "Battery should be 0");
            Assert.IsFalse(continueSimulation, "Simulation should stop");
            Assert.AreEqual(DroneState.Landed, status.State, "State should be Landed when battery is depleted");
            Assert.IsTrue(status.IsAlarm, "IsAlarm should be true");
            Assert.AreEqual(AlarmType.BatteryDepleted, status.AlarmType, "AlarmType should be BatteryDepleted");

            _callbackMock.Verify(c => c.OnAlarm(
                It.Is<DroneStatus>(s => s.Battery == 0),
                AlarmType.BatteryDepleted),
                Times.Once(),
            "OnAlarm should be invoked with BatteryDepleted");
        }

        // CP7
        [TestMethod]
        public void Battery500_Flying_Arrived_Periodic_Battery499_Flying_SimulationContinues()
        {
            _droneSimulator.InitialBattery = 500;
            _droneSimulator.StartSimulation(ShortRouteWaypoints, isPeriodic: true);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(499, status.Battery, "Battery should be 499 after one step");
            Assert.AreEqual(DroneState.Flying, status.State, "State should remain Flying for periodic route");
            Assert.IsTrue(continueSimulation, "Simulation should continue for periodic route");
        }

        // CP08
        [TestMethod]
        public void Battery0_Flying_NotArrived_BatteryNegative_InvalidOrError()
        {

            _droneSimulator.InitialBattery = 0;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(0, status.Battery, "Battery should stay at 0 (cannot go negative)");
            Assert.IsFalse(continueSimulation, "Simulation should stop when battery is 0");
            Assert.AreEqual(DroneState.Landed, status.State, "State should be Landed when battery is depleted");
        }

        // CP09
        [TestMethod]
        public void Battery500_Stopped_NotArrived_ShouldNotProcessUpdate()
        {
            _droneSimulator.InitialBattery = 500;

            var status = _droneSimulator.GetStatus();

            Assert.AreEqual(DroneState.Stopped, status.State, "State should be Stopped");
            Assert.AreEqual(0, status.Battery, "Battery should be 0 (not initialized by StartSimulation)");
        }

        // CP10
        [TestMethod]
        public void Battery500_Landed_NotArrived_ShouldNotProcessUpdate()
        {

            _droneSimulator.InitialBattery = 500;
            _droneSimulator.StartSimulation(ShortRouteWaypoints, isPeriodic: false);

            bool continueSimulation = true;
            while (continueSimulation)
            {
                continueSimulation = _droneSimulator.StepSimulation();
            }

            var statusAfterLanding = _droneSimulator.GetStatus();
            int batteryAfterLanding = (int)statusAfterLanding.Battery;

            Assert.AreEqual(DroneState.Landed, statusAfterLanding.State, "State should be Landed");

            bool shouldNotContinue = _droneSimulator.StepSimulation();
            var statusAfterExtraStep = _droneSimulator.GetStatus();

            Assert.IsFalse(shouldNotContinue, "Simulation should not continue when Landed");
            Assert.AreEqual(DroneState.Landed, statusAfterExtraStep.State, "State should remain Landed");
            Assert.AreEqual(batteryAfterLanding, (int)statusAfterExtraStep.Battery, "Battery should not change when Landed");
        }

        // CP11
        [TestMethod]
        public void CBatteryNegative1_Flying_NotArrived_ErrorOrInvalidHandling()
        {

            _droneSimulator.InitialBattery = -1;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(0, status.Battery, "Battery should be clamped to 0 (negative values not allowed)");
            Assert.IsFalse(continueSimulation, "Simulation should stop when battery is 0");
            Assert.AreEqual(DroneState.Landed, status.State, "State should be Landed when battery is depleted");
        }

        // CP12
        [TestMethod]
        public void Battery1001_Flying_NotArrived_ErrorOrNormalization()
        {

            _droneSimulator.InitialBattery = 1001;
            _droneSimulator.StartSimulation(LongRouteWaypoints, isPeriodic: false);

            bool continueSimulation = _droneSimulator.StepSimulation();

            var status = _droneSimulator.GetStatus();
            Assert.AreEqual(999, status.Battery, "Battery should be 999 after decrement from clamped 1000");
            Assert.IsTrue(continueSimulation, "Simulation should continue");
        }
    }
    
}
