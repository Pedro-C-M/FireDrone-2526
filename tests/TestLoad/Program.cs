using System;
using System.Threading.Tasks;

namespace TestLoad
{
    class Program
    {
        const int DEFAULT_DRONES = 50;
        private static readonly HttpClient client = new HttpClient();
        private const string BackendUrl = "http://localhost:5306";

        // Este programa es un menú simple para poblar o limpiar la base de datos antes o después de las pruebas de carga.
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==========================================");
                Console.WriteLine("=========== PRUEBAS DE CARGA =============");
                Console.WriteLine("==========================================");
                Console.WriteLine("1. Poblar BD para pruebas de carga");
                Console.WriteLine("2. Limpiar BD tras acabar pruebas de carga");
                Console.WriteLine("3. Salir");
                Console.WriteLine("==========================================");
                Console.WriteLine("Selecciona una opción: ");

                var key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case '1':
                        int nDrones = AskForNumber("¿Cuántos drones quieres crear?: ");
                        Console.WriteLine(" - Cargando la base de datos para "+ nDrones +" drones...");
                        await CleanDatabase(nDrones);
                        Console.WriteLine("Base de datos cargada");
                        Pause();
                        break;
                    case '2':
                        Console.WriteLine("Limpiando la base de datos...");
                        await CleanDatabase();
                        Console.WriteLine("Base de datos limpiada");
                        Pause();
                        break;
                    case '3':
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        Pause();
                        break;
                }
            }
        }
        static int AskForNumber(string message)
        {
            Console.Write(message);
            string input = Console.ReadLine();
            if (int.TryParse(input, out int result) && result > 0)
            {
                return result;
            }
            Console.WriteLine("Número inválido, usando valor por defecto: "+ DEFAULT_DRONES);
            return DEFAULT_DRONES;
        }

        static void Pause()
        {
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        private static async Task CleanDatabase(int? nDrones = null)
        {
            Console.WriteLine($"Iniciando limpieza de base de datos{(nDrones.HasValue ? $" con {nDrones} drones" : "")}...");
            try
            {
                string url = $"{BackendUrl}/api/database/clean";
                if (nDrones.HasValue)
                {
                    url += $"?drones={nDrones.Value}";
                }

                HttpResponseMessage response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Éxito: {result}");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de conexión: {e.Message}");
                Console.WriteLine($"Asegúrese de que el backend esté ejecutándose en {BackendUrl}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error inesperado: {e.Message}");
            }
        }
    }
}