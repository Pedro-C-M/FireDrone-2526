using System;
using System.Threading;

namespace DroneController
{
	/* 
	 * El controlador del dron recibe dos parámetros a través de la línea de comandos cuando se ejecuta:
	 *	- Identificador del dron (necesario para crear la cola que lo comunica con el backend)
	 *	- Driver que se usa para controlar el dron
	 */

	class Program
	{
		// Para gestionar la terminación
		private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

		// Terminación del controlador: aprovechar esta función para liberar recursos
		private static void OnExit(object sender, ConsoleCancelEventArgs args)
		{
			Log.Debug("Shutting down DroneController");
			_closing.Set();
		}

		static void Main(string[] args)
		{
			if (args.Length != 2)
				throw new ArgumentException("Invalid number of arguments");

			// Ejemplo: 124af46
			string DroneID = args[0];

			// Ejemplo: DroneSimulator
			string DroneDriver = args[1];

			DroneController controller = new DroneController(DroneID, DroneDriver);

			// El controlador contiene un bucle de procesamiento de mensajes
			controller.Run();

			// Esperar a Control-C para terminar la aplicación
			Log.Debug("Executing DroneController. Press Control-C to exit");
			Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
			_closing.WaitOne();

			controller.Stop();
		}
	}
}
