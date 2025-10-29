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

		public double GetCurrentLatitude() { return _currentWaypoint.Latitude;  }
		public double GetCurrentLongitude() { return _currentWaypoint.Longitude; }
		public double GetCurrentAltitude() { return _currentWaypoint.Altitude; }
		public double GetCurrentSpeed() { return _currentWaypoint.Speed; }
		public double GetCurrentStep() { return _currentStep; }
		public double GetCurrentNumSteps() { return _numSteps; }
		public double GetCurrentWaypointIndex() { return _indexCurrentWaypoint; }

		public FlightSimulator(Waypoint[] waypoints, int updateInterval)
		{
			_updateInterval = updateInterval;
			_waypoints = waypoints;
			_indexCurrentWaypoint = -1;

			NextCoordinate();
		}

		public int GetNumSteps()
		{
			return _numSteps;
		}

		private bool NextCoordinate()
		{
			_indexCurrentWaypoint++;

			if (_indexCurrentWaypoint == _waypoints.Length - 1)
			{
				_currentWaypoint = _waypoints[_indexCurrentWaypoint];
				return true;
			}

			_currentWaypoint = new Waypoint {
				Latitude = _waypoints[_indexCurrentWaypoint].Latitude,
				Longitude = _waypoints[_indexCurrentWaypoint].Longitude,
				Altitude = _waypoints[_indexCurrentWaypoint].Altitude,
				Speed = _waypoints[_indexCurrentWaypoint].Speed
			};

			// Set rate of change
			// Distance between points with direction for lat and lon (km)
			var deltaLat = (_waypoints[_indexCurrentWaypoint + 1].Latitude - _currentWaypoint.Latitude) * KM_IN_DEGREE;
			var deltaLon = (_waypoints[_indexCurrentWaypoint + 1].Longitude - _currentWaypoint.Longitude) * KM_IN_DEGREE;

			// Ss the crow flies distance (km)
			var deltaDist = Math.Sqrt((deltaLat * deltaLat) + (deltaLon * deltaLon));

			// Total time between points at desired speed (sec)
			double speed = _waypoints[_indexCurrentWaypoint].Speed / SECONDS_IN_HOUR;
			var deltaSeconds = deltaDist / speed;

			deltaSeconds = deltaSeconds / (_updateInterval / 1000.0);

			// Rate of change for each update 
			_currentRateLatitude = deltaLat / deltaSeconds / KM_IN_DEGREE;
			_currentRateLongitude = deltaLon / deltaSeconds / KM_IN_DEGREE;

			// Total steps
			_numSteps = (int)Math.Floor(deltaSeconds);
			_currentStep = 0;

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
				arrived = NextCoordinate();
			}

			return arrived;
		}
	}
}
