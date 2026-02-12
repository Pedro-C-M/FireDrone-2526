using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace TestLoad
{
    internal static class DataLoader
    {
        // ⚠️ AJUSTA EL PUERTO SI ES NECESARIO (5306 es el Central Backend)
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:5306") };
        private const int CANTIDAD = 50; // Número de elementos a crear

        public static async Task Run(int nDrones)
        {
        }
    }
}
