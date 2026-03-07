using System;

namespace DroneController
{
	static class Log
	{
		static public void Debug(string message)
		{
			Console.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} {message}");
		}
	}
}
