using DroneController.Drone;
using System;
using System.Linq;

namespace DroneController
{
    /* Referencia de simulación
	 * https://github.com/russellgoldenberg/geolocation-simulator
	 * 
	 * Simula el movimiento interpolando entre las coordenadas en función de la velocidad
	 * 
	 * Cada vez que se invoca al método StepSimulation se avanza 1 segundo (por defecto) en dirección a la siguiente coordenada
	 * hasta llegar al destino
	 * 
	 * Aplica una aproximación simple para calcular el avance
	 * 
	 * No tiene en cuenta la altitud
	 */

    class FlightSimulator
    {
        const double KM_IN_DEGREE = 110.562;
        const int SECONDS_IN_HOUR = 3600;

        int _updateInterval;
        Waypoint[] _waypoints;
        int _indexCurrentWaypoint;
        Waypoint _currentWaypoint;
        double _currentRateLatitude;
        double _currentRateLongitude;

        int _numSteps;
        int _currentStep;
        bool _isPeriodic;

        public double GetCurrentLatitude() { return _currentWaypoint.Latitude; }
        public double GetCurrentLongitude() { return _currentWaypoint.Longitude; }
        public double GetCurrentAltitude() { return _currentWaypoint.Altitude; }
        public double GetCurrentSpeed() { return _currentWaypoint.Speed; }
        public double GetCurrentStep() { return _currentStep; }
        public double GetCurrentNumSteps() { return _numSteps; }
        public double GetCurrentWaypointIndex() { return _indexCurrentWaypoint; }

        public bool IsPeriodic => _isPeriodic;

        public FlightSimulator(Waypoint[] waypoints, int updateInterval, bool isPeriodic)
        {
            _updateInterval = updateInterval;
            _waypoints = waypoints;
            _indexCurrentWaypoint = -1;
            _isPeriodic = isPeriodic;

            NextCoordinate();
        }

        public int GetNumSteps()
        {
            return _numSteps;
        }

        private bool NextCoordinate()
        {
            _indexCurrentWaypoint++;

            // Check if we've reached the end of the waypoint array
            if (_indexCurrentWaypoint >= _waypoints.Length)
            {
                if (_isPeriodic)
                {
                    _indexCurrentWaypoint = 0; // REINICIO - wrap back to beginning
                    Console.WriteLine($"[FlightSimulator] Reached end of periodic route, wrapping to start");
                }
                else
                {
                    // For simple routes, we've finished
                    _indexCurrentWaypoint = _waypoints.Length - 1;
                    _currentWaypoint = _waypoints[_indexCurrentWaypoint];
                    Console.WriteLine($"[FlightSimulator] ✓ Reached end of simple route - ARRIVED (waypoint {_indexCurrentWaypoint}/{_waypoints.Length})");
                    return true; // ruta simple terminada
                }
            }

            _currentWaypoint = new Waypoint
            {
                Latitude = _waypoints[_indexCurrentWaypoint].Latitude,
                Longitude = _waypoints[_indexCurrentWaypoint].Longitude,
                Altitude = _waypoints[_indexCurrentWaypoint].Altitude,
                Speed = _waypoints[_indexCurrentWaypoint].Speed
            };

            // Determine the next waypoint index (with wrap-around for periodic routes)
            int nextIndex;
            if (_indexCurrentWaypoint == _waypoints.Length - 1)
            {
                // We're at the last waypoint
                if (_isPeriodic)
                {
                    nextIndex = 0; // Wrap back to first waypoint for periodic routes
                    Console.WriteLine($"[FlightSimulator] At last waypoint of periodic route, will wrap to start");
                }
                else
                {
                    // For non-periodic routes, we've arrived at the final destination
                    // StepSimulation already placed us at this position before calling NextCoordinate
                    Console.WriteLine($"[FlightSimulator] ✓ Arrived at final waypoint of simple route (waypoint {_indexCurrentWaypoint}/{_waypoints.Length})");
                    return true;
                }
            }
            else
            {
                nextIndex = _indexCurrentWaypoint + 1;
                Console.WriteLine($"[FlightSimulator] Moving to waypoint {_indexCurrentWaypoint}/{_waypoints.Length - 1}, next={nextIndex}");
            }

            // Set rate of change
            // Distance between points with direction for lat and lon (km)
            var deltaLat = (_waypoints[nextIndex].Latitude - _currentWaypoint.Latitude) * KM_IN_DEGREE;
                var deltaLon = (_waypoints[nextIndex].Longitude - _currentWaypoint.Longitude) * KM_IN_DEGREE;

                // As the crow flies distance (km)
                var deltaDist = Math.Sqrt((deltaLat * deltaLat) + (deltaLon * deltaLon));

                // Total time between points at desired speed (sec)
                double speed = _waypoints[_indexCurrentWaypoint].Speed / SECONDS_IN_HOUR;
                var deltaSeconds = deltaDist / speed;

                deltaSeconds = deltaSeconds / (_updateInterval / 1000.0);

		int minSteps = 5;
		deltaSeconds = Math.Max(deltaSeconds, minSteps);

                // Rate of change for each update 
                _currentRateLatitude = deltaLat / deltaSeconds / KM_IN_DEGREE;
                _currentRateLongitude = deltaLon / deltaSeconds / KM_IN_DEGREE;

                // Total steps
                _numSteps = (int)Math.Floor(deltaSeconds);
                _currentStep = 0;

                Console.WriteLine($"[FlightSimulator] Distance to next waypoint: {deltaDist:F3}km, Speed: {_waypoints[_indexCurrentWaypoint].Speed}km/h, Steps: {_numSteps}");

            return false;
        }

        public bool StepSimulation()
        {
            bool arrived = false;

            _currentStep++;
            if (_currentStep < _numSteps)
            {
                _currentWaypoint.Latitude += _currentRateLatitude;
                _currentWaypoint.Longitude += _currentRateLongitude;
            }
            else
            {
                // Ensure we're exactly at the target waypoint before considering arrival
                int nextIndex = _indexCurrentWaypoint + 1;
                if (nextIndex < _waypoints.Length)
                {
                    _currentWaypoint.Latitude = _waypoints[nextIndex].Latitude;
                    _currentWaypoint.Longitude = _waypoints[nextIndex].Longitude;
                    _currentWaypoint.Altitude = _waypoints[nextIndex].Altitude;
                }
                
                arrived = NextCoordinate();
                
                // For non-periodic routes at the last waypoint, ensure we're exactly at that position
                if (arrived && !_isPeriodic && _indexCurrentWaypoint < _waypoints.Length)
                {
                    _currentWaypoint.Latitude = _waypoints[_indexCurrentWaypoint].Latitude;
                    _currentWaypoint.Longitude = _waypoints[_indexCurrentWaypoint].Longitude;
                    _currentWaypoint.Altitude = _waypoints[_indexCurrentWaypoint].Altitude;
                    Console.WriteLine($"[FlightSimulator] Final position correction: Lat={_currentWaypoint.Latitude}, Lon={_currentWaypoint.Longitude}");
                }
            }

            return arrived;
        }
    }
}