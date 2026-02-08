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
                    "cp2.csv" => $"{header}\n",
                    "cp3.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10\nRuta Prueba;1;43.6450;-5.7600;50;10\nRuta Prueba;0;43.7450;-5.8600;50;10",
                    "cp4.1.csv" => $"{header}\n ;0;43.5450;-5.6600;50;10",
                    "cp4.2.csv" => $"{header}\nRuta Prueba; ;43.5450;-5.6600;50;10",
                    "cp4.3.csv" => $"{header}\nRuta Prueba;0; ;-5.6600;50;10",
                    "cp4.4.csv" => $"{header}\nRuta Prueba;0;43.5450; ;50;10",
                    "cp4.5.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600; ;10",
                    "cp4.6.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50; ",
                    "cp5.csv" => $"{header}\nRuta Prueba;Error;43.5450;-5.6600;50;10",
                    "cp6.1.csv" => $"{header}\nRuta Prueba;0;43sss;-5.6600;50;10",
                    "cp6.2.csv" => $"{header}\nRuta Prueba;0;43.5450;Error;50;10",
                    "cp6.3.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;Error;10",
                    "cp6.4.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;$2.2#|",
                    "cp7.csv" => $"{header}\nRuta Prueba;2;43.5450;-5.6600;50;10",
                    "cp8.1.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;-10",
                    "cp8.2.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;-50;10",
                    "cp9.1.csv" => $"{header}\nRuta Prueba;0;90.1000;-5.6600;50;10",
                    "cp9.2.csv" => $"{header}\nRuta Prueba;0;43.5450;-180.1000;50;10",
                    "cp10.1.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50",
                    "cp10.2.csv" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10;11",
                    "cp11.txt" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10",
                    "cp12" => $"{header}\nRuta Prueba;0;43.5450;-5.6600;50;10",

                    _ => throw new Exception("CaseID no definido")
                };
            }
        }
    }
}

