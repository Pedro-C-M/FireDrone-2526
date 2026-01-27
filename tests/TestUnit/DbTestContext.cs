using Microsoft.EntityFrameworkCore;
using Microsoft.Testing.Platform.Extensions.Messages;
using CentralBackend;
using System;
using System.IO;

namespace TestUnit
{
    ///<summary>
    ///Contexto de BD a utilizar en cada proyecto de pruebas.
    ///Hereda del contexto usado en el backend del proyecto, pero se instancia utilizando DbContextOptionsBuilder
    ///porque al ejecutar los test no se esta en el entorno ASP.NET que lo inicializa (habitualmente) con AddDbContext.
    ///Aqui se implementa el metodo que limpia toda la base de datos y que sera invocado en el setup de los tests.
    ///</summary>
    public class DbTestContext : FireDrone 
    {
        //Origen de datos relativo a donde se ejecutan los test
        private static readonly string DataSource = "Data Source=" + Config.DatabaseFileName;

        ///<summary>
        ///Obtiene el contexto de BD especifico de este proyecto para usar en los tests
        ///Personalizar con una de las dos formas siguientes dependiendo de si el contexto padre tiene o no parametro options
        ///</summary>

        //Si hay parametro options
        //public DbTestContext() : base(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(DataSource).Options) { }
        //Si NO hay parametro options
        public DbTestContext() : base() { }
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(DataSource);

        /// <summary>
        /// Elimina todas las filas de las tablas de aplicacion.
        /// La lista de tablas ordenadas de detalle a maestro en app.properties habra sido establecida en Config.DatabaseTables
        /// </summary>
        public void CleanDatabase()
        {
            if (!File.Exists(Config.DatabaseFileName))
                throw new Exception("El fichero de base de datos no existe en la carpeta de la solucion o no tiene el nombre requerido");
            // descomentar esto mientras se esta configurando la personalizacion para cada proyecto
            // this.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Test (value text)");

            DbTestUtil util = new(this);
            if (!string.IsNullOrEmpty(Config.DatabaseTables))
                util.CleanTables(Config.DatabaseTables.Trim().Split(","), false);
        }
    }
}
