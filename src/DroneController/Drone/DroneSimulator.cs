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
                Console.WriteLine($"[DroneSimulator] *** DRONE ARRIVED AT DESTINATION ***");
                Console.WriteLine($"[DroneSimulator] IsPeriodic={_flightSimulator.IsPeriodic}");
                Console.WriteLine($"[DroneSimulator] Current State={_status.State}");
                Console.WriteLine($"[DroneSimulator] Position: Lat={_status.Latitude}, Lon={_status.Longitude}");
                if (!_flightSimulator.IsPeriodic)
                {
                    _status.State = DroneState.Landed;
                    _status.Speed = 0;
                    _status.Altitude = 0;
                    continueSimulation = false;
                    Console.WriteLine($"[DroneSimulator] *** DRONE SET TO LANDED STATE (non-periodic route) ***");
                }
                else
                {
                    Console.WriteLine($"[DroneSimulator] Periodic route - continuing flight");
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
            Console.WriteLine($"[DroneSimulator] *** StartFlightPlan called with {waypoints.Length} waypoints, isPeriodic={isPeriodic} ***");
            
            // Stop any existing flight before starting new flight plan
            if (_task != null && !_task.IsCompleted)
            {
                Console.WriteLine($"[DroneSimulator] *** STOPPING EXISTING FLIGHT before starting new flight plan ***");
                Console.WriteLine($"[DroneSimulator] Task status: {_task.Status}");
                try
                {
                    _tokenSource?.Cancel();
                    _task.Wait(TimeSpan.FromSeconds(2)); // Wait with timeout
                    Console.WriteLine($"[DroneSimulator] Existing flight stopped successfully");
                }
                catch (AggregateException ex)
                {
                    // Expected after cancellation
                    Console.WriteLine($"[DroneSimulator] AggregateException during cancel (expected): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DroneSimulator] Unexpected exception during cancel: {ex.Message}");
                }
                finally
                {
                    _tokenSource?.Dispose();
                    Console.WriteLine($"[DroneSimulator] Token source disposed");
                }
            }
            else
            {
                Console.WriteLine($"[DroneSimulator] No existing flight to stop (task is null or completed)");
            }
            
            _status.State = DroneState.Flying;
            _tokenSource = new CancellationTokenSource();
            CancellationToken token = _tokenSource.Token;

            _task = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"[DroneSimulator] Flight task started for {waypoints.Length} waypoints");
                StartSimulation(waypoints, isPeriodic);

                int stepCount = 0;
                while (StepSimulation())
                {
                    stepCount++;
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine($"[DroneSimulator] *** CANCELLATION REQUESTED after {stepCount} steps ***");
                        token.ThrowIfCancellationRequested();
                    }

                    Task.Delay(UpdateIntervalMs).Wait();
                }
                Console.WriteLine($"[DroneSimulator] *** FLIGHT TASK COMPLETED after {stepCount} steps ***");
                Console.WriteLine($"[DroneSimulator] Final state: {_status.State}, Position: Lat={_status.Latitude}, Lon={_status.Longitude}");
            }, _tokenSource.Token);

            _task.ContinueWith((_task) => 
            {
                if (_task.IsCanceled)
                    Console.WriteLine($"[DroneSimulator] Task simulation CANCELLED");
                else if (_task.IsFaulted)
                    Console.WriteLine($"[DroneSimulator] Task simulation FAULTED: {_task.Exception?.Message}");
                else
                    Console.WriteLine($"[DroneSimulator] Task simulation FINISHED normally");
            });
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
                Console.WriteLine($"[DroneSimulator] *** STOPPING EXISTING FLIGHT before manual GoTo ***");
                Console.WriteLine($"[DroneSimulator] Task status: {_task.Status}");
                try
                {
                    _tokenSource?.Cancel();
                    _task.Wait(TimeSpan.FromSeconds(2)); // Wait with timeout
                    Console.WriteLine($"[DroneSimulator] Existing flight stopped successfully");
                }
                catch (AggregateException ex)
                {
                    // Expected after cancellation
                    Console.WriteLine($"[DroneSimulator] AggregateException during cancel (expected): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DroneSimulator] Unexpected exception during cancel: {ex.Message}");
                }
                finally
                {
                    _tokenSource?.Dispose();
                    Console.WriteLine($"[DroneSimulator] Token source disposed");
                }
            }
            else
            {
                Console.WriteLine($"[DroneSimulator] No existing flight to stop (task is null or completed)");
            }

            // Get current position
            DroneStatus currentStatus = GetStatus();
            
            Console.WriteLine($"[DroneSimulator] Current position: Lat={currentStatus.Latitude}, Lon={currentStatus.Longitude}, Battery={currentStatus.Battery}");

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

            Console.WriteLine($"[DroneSimulator] *** MANUAL GOTO WAYPOINTS ***");
            Console.WriteLine($"[DroneSimulator] Waypoint 0 (current): Lat={waypoints[0].Latitude}, Lon={waypoints[0].Longitude}, Alt={waypoints[0].Altitude}, Speed={waypoints[0].Speed}");
            Console.WriteLine($"[DroneSimulator] Waypoint 1 (target): Lat={waypoints[1].Latitude}, Lon={waypoints[1].Longitude}, Alt={waypoints[1].Altitude}, Speed={waypoints[1].Speed}");
            Console.WriteLine($"[DroneSimulator] Starting manual flight with 2 waypoints (isPeriodic=FALSE)");
            // Start a new flight plan from current position to target coordinate (non-periodic by default)
            StartFlightPlan(waypoints, isPeriodic: false);
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