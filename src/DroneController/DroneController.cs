using Newtonsoft.Json;
using System;
using System.Threading;

namespace DroneController
{
	public class DroneController
	{
		string _droneID;
		string _droneDriver;

		IDroneDriver _drone;

		public DroneController(string droneID, string droneDriver)
		{
			_droneID = droneID;
			_droneDriver = droneDriver;

			Log.Debug($"Drone controller {_droneID}-{_droneDriver} starting");

			// Crear cola de mensajes
			CreateMessageQueue(_droneID);

			// Instanciar driver de forma dinámica
			_drone = CreateDroneDriver(_droneDriver);
		}

		// Esperar a recibir mensajes del backend a través de la cola
		// Se procesaran en HandleDroneCommand
		public void Run()
		{
			/*
			 * FALTA POR COMPLETAR
			 * *
			 */ 
		}

		public void Stop()
		{
			/*
			 * FALTA POR COMPLETAR
			 * *
			 */
		}

		// Se instancia el driver de forma dinámica. 
		// Debe haber una clase que implemente la interfaz IDroneDriver y cuyo nombre coincida con el driver
		// en el namespace del controlador
		private IDroneDriver CreateDroneDriver(string DroneDriver)
		{
			Type type = Type.GetType(GetType().Namespace + "." + DroneDriver);
			if (type == null)
			{
				throw new ArgumentException($"Error unable to find drone driver {DroneDriver}");
			}
			IDroneDriver drone = (IDroneDriver)Activator.CreateInstance(type);

			// Sería necesario publicar la información
			drone.SetUpdateCallback(new ConsoleDroneUpdate());

			return drone;
		}

		// Crear una cola para recibir comandos del backend control
		private void CreateMessageQueue(string DroneID)
		{
			/*
			 * FALTA POR COMPLETAR
			 * *
			 */
		}

		// Enviar el estado a través de la cola para recibir al backend control
		private void SendStatus(string message)
		{
			/*
			 * FALTA POR COMPLETAR
			 * *
			 */
		}

		// Gestión de los mensajes de comandos recibidos por el controlador
		// Si se añaden más mensajes se debería gestionar con una tabla
		private void HandleDroneCommand(string commandtext)
		{
			// Decodificar el mensaje
			DroneCommand command = JsonConvert.DeserializeObject<DroneCommand>(commandtext);

			Log.Debug($"Executing drone command {command.Command}");

			if (command.Command == DroneCommand.START_FLIGHT_PLAN_CMD)
			{
				// Decodificar los argumentos
				Waypoint[] waypoints = JsonConvert.DeserializeObject<Waypoint[]>(command.Arguments);
				_drone.StartFlightPlan(waypoints);
			}
			else if (command.Command == DroneCommand.STOP_FLIGHT_PLAN_CMD)
			{
				_drone.StopFlightPlan();
			}
			else if (command.Command == DroneCommand.STATUS_CMD)
			{
				DroneStatus status = _drone.GetStatus();

				// Codificar el estado como JSON
				var statusStr = JsonConvert.SerializeObject(status);

				SendStatus(statusStr);
			}
		}
	}
}
