using System;
using System.Threading.Tasks;

namespace TestLoad
{
    class Program
    {
        // Este programa es un menú simple para poblar o limpiar la base de datos antes o después de las pruebas de carga.
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
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
                        Console.WriteLine("Cargamos la base de datos");
                        Pause();
                        break;
                    case '2':
                        Console.WriteLine("Limpiamos la base de datos");
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

        static void Pause()
        {
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}