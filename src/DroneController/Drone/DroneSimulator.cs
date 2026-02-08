using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DroneController.Drone
{
    /*
	 * La clase DroneSimulator simula el movimiento de un dron implementando la interfaz IDroneDriver.
	 * 
	 * Para simular el vuelo se usa la clase FlightSimulator, que implementa el cálculo del desplazamiento.
	 * 
	 * Para simular el aspecto temporal se crea una tarea de invoca de forma periódica a StepSimulation,
	 * que actualiza la posición cada UpdateIntervalMs. Cada vez que se actualiza la posición se reduce la bateria en una unidad
	 * 
	 * Los tests muestran un ejemplo de uso.
	 */

    public class DroneSimulator : IDroneDriver
    {
        const int DEFAULT_UPDATE_INTERVAL_MS = 1000;
        const int DEFAULT_INITIAL_BATTERY = 1000;
      const int LOW_BATTERY_THRESHOLD = 100; // 10% of 1000
      const int MAX_BATTERY = 1000;
        const int MIN_BATTERY = 0;

        public int UpdateIntervalMs { get; set; }
        public int InitialBattery { get; set; }

        private CancellationTokenSource _tokenSource;
        private Task _task;

        private FlightSimulator _flightSimulator;

        private object _statusLock = new object();
        private DroneStatus _status;
        private bool _lowBatteryAlarmTriggered = false; // Track if low battery alarm was already sent

        public DroneSimulator()
        {
            UpdateIntervalMs = DEFAULT_UPDATE_INTERVAL_MS;
            InitialBattery = DEFAULT_INITIAL_BATTERY;

            _status = new DroneStatus
            {
                Latitude = 0,
                Longitude = 0,
                Altitude = 0,
                Speed = 0,
                Battery = 0,
                State = DroneState.Stopped,
                IsAlarm = false,
                AlarmType = AlarmType.None
            };
        }

        IDroneCallback _updateCallback = null;

        public void SetUpdateCallback(IDroneCallback callback)
        {
            _updateCallback = callback;
        }

        // Número de pasos necesarios para completar la simulación entre las dos posiciones actuales
        public int GetNumSteps()
        {
            return _flightSimulator.GetNumSteps();
        }

        // Retorna true si la simulación debe continuar
        public bool StepSimulation()
        {
            // Check if drone is in a terminal state (Landed or Stopped) - don't process updates
            lock (_statusLock)
            {
                if (_status.State == DroneState.Landed || _status.State == DroneState.Stopped)
                {
                    return false;
                }
            }

            // Actualiza la posición
            bool arrived = _flightSimulator.StepSimulation();

            // Actualiza el estado
            bool continueSimulation = UpdateStatus(arrived);

            // La simulación termina cuando se acaba la bateria y se llega al destino
            return continueSimulation;
        }

    // Actualiza el estado
    private bool UpdateStatus(bool arrived)
    {
        bool continueSimulation = true;
        AlarmType alarmToTrigger = AlarmType.None;

        // Actualización del estado de forma sincronizada (variable compartida)
        lock (_statusLock)
        {
            // Don't process updates if drone is Landed or Stopped
            if (_status.State == DroneState.Landed || _status.State == DroneState.Stopped)
            {
                return false;
            }

            _status.Latitude = _flightSimulator.GetCurrentLatitude();
            _status.Longitude = _flightSimulator.GetCurrentLongitude();
            _status.Altitude = _flightSimulator.GetCurrentAltitude();
            _status.Speed = _flightSimulator.GetCurrentSpeed();

            // Reset alarm state for this update
            _status.IsAlarm = false;
            _status.AlarmType = AlarmType.None;

            // Se reduce la batería en una unidad, but don't go below 0
            if (_status.Battery > MIN_BATTERY)
            {
                _status.Battery--;
            }

            if (arrived)
            {
                if (!_flightSimulator.IsPeriodic)
                {
                    _status.State = DroneState.Landed;
                    continueSimulation = false;
                }
            }

            // Check for battery depleted (critical alarm)
            if (_status.Battery <= MIN_BATTERY)
            {
                _status.Battery = MIN_BATTERY; // Ensure battery is exactly 0
                _status.Altitude = 0;
                _status.Speed = 0;
                _status.State = DroneState.Landed; // Set state to Landed when battery depleted
                _status.IsAlarm = true;
                _status.AlarmType = AlarmType.BatteryDepleted;
                alarmToTrigger = AlarmType.BatteryDepleted;

                continueSimulation = false;
            }
            // Check for low battery warning (only trigger once)
            else if (_status.Battery <= LOW_BATTERY_THRESHOLD && !_lowBatteryAlarmTriggered)
            {
                _status.IsAlarm = true;
                _status.AlarmType = AlarmType.LowBattery;
                alarmToTrigger = AlarmType.LowBattery;
                _lowBatteryAlarmTriggered = true;
            }

            // Always call Update callback
            if (_updateCallback != null)
            {
                _updateCallback.Update(_status);

                // Trigger alarm callback if an alarm condition was detected
                if (alarmToTrigger != AlarmType.None)
                {
                    _updateCallback.OnAlarm(_status, alarmToTrigger);
                }
            }
        }

            return continueSimulation;
    }

        // Inicializa la simulación
        public void StartSimulation(Waypoint[] waypoints, bool isPeriodic = false)
        {
            _flightSimulator = new FlightSimulator(waypoints, UpdateIntervalMs, isPeriodic);
	        Console.WriteLine($"[DroneSimulator] Waypoint speed received: {waypoints[0].Speed}");
       
            // Validate and clamp InitialBattery to valid range [0, 1000]
            int validatedBattery = InitialBattery;
            if (validatedBattery < MIN_BATTERY)
            {
                validatedBattery = MIN_BATTERY;
            }
            else if (validatedBattery > MAX_BATTERY)
            {
                validatedBattery = MAX_BATTERY;
            }
        
            _status.Battery = validatedBattery;
            _status.State = DroneState.Flying;
            _status.IsAlarm = false;
            _status.AlarmType = AlarmType.None;
	        _lowBatteryAlarmTriggered = false; // Reset low battery alarm flag
        }

        // Ejecuta una tarea para simular el plan de vuelo entre la lista de coordenadas
        public void StartFlightPlan(Waypoint[] waypoints, bool isPeriodic = false)
        {
            _status.State = DroneState.Flying;
            _tokenSource = new CancellationTokenSource();
            CancellationToken token = _tokenSource.Token;

            _task = Task.Factory.StartNew(() =>
            {
                StartSimulation(waypoints, isPeriodic);

                while (StepSimulation())
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();

                    Task.Delay(UpdateIntervalMs).Wait();
                }
            }, _tokenSource.Token);

            _task.ContinueWith((_task) => Log.Debug("Task simulation finished"));
        }

        // Detiene la tarea de simulación
        public void StopFlightPlan()
        {//Si peta aqui es que se intenta parar sin start antes
            _tokenSource.Cancel();
            try
            {
                _task.Wait();
            }
            catch (AggregateException /*e*/)
            {
                // Excepción esperada tras la cancelación
            }
            finally
            {
                _tokenSource.Dispose();
            }
            lock (_statusLock)
            {
                _status.State = DroneState.Stopped;
                _status.Speed = 0;

                // Notify the callback that the drone has stopped
                if (_updateCallback != null)
                {
                    _updateCallback.Update(_status);
                }
            }
        }

        // Ir a una coordenada específica (modo manual)
        public void GoTo(double latitude, double longitude, double speed)
        {
            Console.WriteLine($"[DroneSimulator] GoTo called: Lat={latitude}, Lon={longitude}, Speed={speed}");

		if (speed <= 0)
    		{
        		Console.WriteLine("[DroneSimulator] ⚠ Speed invalid, using default 20");
        		speed = 20; 
    		}

            // Stop any existing flight before starting manual movement
            if (_task != null && !_task.IsCompleted)
            {
                Console.WriteLine($"[DroneSimulator] Stopping existing flight before manual GoTo");
                try
                {
                    _tokenSource?.Cancel();
                    _task.Wait(TimeSpan.FromSeconds(2)); // Wait with timeout
                }
                catch (AggregateException)
                {
                    // Expected after cancellation
                }
                finally
                {
                    _tokenSource?.Dispose();
                }
            }

            // Get current position
            DroneStatus currentStatus = GetStatus();

            // Create waypoints array: current position + target coordinate
            Waypoint[] waypoints = new Waypoint[]
            {
                new Waypoint
                {
                    Latitude = currentStatus.Latitude,
                    Longitude = currentStatus.Longitude,
                    Altitude = currentStatus.Altitude > 0 ? currentStatus.Altitude : 50, // Use current altitude or default
		    Speed = speed
                },
                new Waypoint
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Altitude = 50, // Default altitude
		    Speed= speed
                }
            };

            // Start a new flight plan from current position to target coordinate
            StartFlightPlan(waypoints);
        }

        // Obtiene el estado actual del dron
        public DroneStatus GetStatus()
        {
            DroneStatus status;
            lock (_statusLock)
            {
                status = _status;
            }
            return status;
        }
    }
}