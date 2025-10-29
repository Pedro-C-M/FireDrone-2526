using System;

namespace DroneController
{
	class Log
	{
		static public void Debug(string message)
		{
			Console.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} {message}");
		}
	}
}
