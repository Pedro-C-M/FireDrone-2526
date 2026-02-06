using System;

namespace TestUnit.util
{
    namespace TestUnit
    {
        ///<summary>
        ///Utilidades de uso general para los test
        ///</summary>
        public static class GeneralTestUtil
        {
            ///<summary>
            ///Clase para generar los CSV en memoria según el diccionario, así me ahorro necesitar una carpeta de CSV y aquí queda reusable
            ///</summary>
            public static string GetCsvContent(string fileName)
            {
                var header = "Nombre;Tipo;Lat;Lon;Altura;Velocidad";

                return fileName switch
                {
                    "cp1.1.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10",
                    "cp1.2.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10\nRuta Prueba;0;43.6450;-5.7600;50;10\nRuta Prueba;0;43.7450;-5.8600;50;10",
                    "cp1.3.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10\nRuta Prueba;0;43.7450;-5.8600;50;10\nRuta Prueba;0;43.6450;-5.7600;50;10",

                    _ => throw new Exception("CaseID no definido")
                };
            }
        }
    }
}

