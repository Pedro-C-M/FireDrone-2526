using System.IO;

namespace TestUnit
{
    ///<summary>
    ///Constantes y utilidades de uso general para obtener datos basicos de la configuracion.
    ///</summary>
    public static class Config
    {
        //direccion relativa de la carpeta de la solucion
        public static readonly string ProjectRoot = Path.Combine("..", "..", "..", "..", "..");

        //Propiedades de donde se lee la configuracion
        public static readonly string ApplicationProperties = "app.properties"; //archivo de configuracion que define host y puerto
        public static readonly string PropFileName = Path.Combine(ProjectRoot, ApplicationProperties);
        public static readonly Properties prop = new Properties().Load(PropFileName);

        //Otros valores derivados
        public static readonly string SolutionFileName = Path.Combine(ProjectRoot, prop.GetProperty("app.solution"));
        public static readonly string DatabaseFileName = SolutionFileName.Replace(".sln", ".db");
        public static readonly string DatabaseTables = prop.GetProperty("app.data.tables");

    }
}
