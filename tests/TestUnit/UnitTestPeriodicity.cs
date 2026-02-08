using DroneController.Drone;
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
    public class UnitTestPeriodicity : UnitTestBase
    {
        private DroneSimulator _drone;
        private Waypoint[] _rutaTest;

        [TestInitialize] public override void SetUp()
        {
            base.SetUp();

            _drone = new DroneSimulator();
            _drone.UpdateIntervalMs = 10; //Para que las pruebas vayan rápido
            _drone.InitialBattery = 5000; // Batería de sobra

            _rutaTest = new Waypoint[] //Ruta corta
            {
                new Waypoint { Latitude = 0, Longitude = 0, Speed = 100 },       // Origen
                new Waypoint { Latitude = 0.0001, Longitude = 0.0001, Speed = 100 } // Destino cercano
            };
        }
        [TestCleanup] public override void TearDown()
        {
            base.TearDown();
            _drone.StopFlightPlan();
        }


        [TestMethod]
        [DataRow(DroneState.Landed)]
        [DataRow(DroneState.Flying)]
        public async Task Periodicity_SimpleRoute_LandOnArrival(DroneState estadoDron)
        {
            _drone.GetStatus().State = estadoDron;
            bool isPeriodic = false;
            _drone.StartFlightPlan(_rutaTest, isPeriodic);
            Assert.AreEqual(DroneState.Flying, _drone.GetStatus().State);

            //En vez de un delay o bucle con un timeout, voy a esperar a que el dron aterrice o se acabe x intentos
            int maxRetries = 20;
            while (_drone.GetStatus().State == DroneState.Flying && maxRetries > 0)
            {
                await Task.Delay(50); // Esperamos 100ms reales
                maxRetries--;
            }

            var status = _drone.GetStatus();
            Assert.AreEqual(DroneState.Landed, status.State, "El dron debería haber aterrizado al terminar la ruta simple.");
            // Posición (aproximada al destino)
            Assert.AreEqual(0.0001, status.Latitude, 0.00001);
        }

        [TestMethod]
        public async Task Periodicity_PeriodicRoute_ContinueFliying()
        {
            _drone.InitialBattery = 10000;
            _drone.GetStatus().State = DroneState.Stopped;

            bool isPeriodic = true;
            _drone.StartFlightPlan(_rutaTest, isPeriodic);
            Assert.AreEqual(DroneState.Flying, _drone.GetStatus().State);

            // Calculamos cuánto batería tiene al principio
            double batteryStart = _drone.InitialBattery;

            // Esperamos un tiempo suficiente para haber completado la ruta al menos una vez
            // (Usamos el mismo tiempo que en el test anterior habría causado el aterrizaje con certeza, un poco más por si acaso 100ms+)
            await Task.Delay(1100);

            // Assert
            var status = _drone.GetStatus();

            //El estado DEBE seguir siendo Flying
            Assert.AreEqual(DroneState.Flying, status.State, "En ruta periódica, el dron no debe aterrizar.");

            //La batería debe haber bajado
            Assert.IsLessThan(batteryStart, status.Battery, "La batería debería consumirse.");
        }
    }
}
