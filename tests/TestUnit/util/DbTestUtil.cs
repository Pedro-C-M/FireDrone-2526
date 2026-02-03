using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Data.Common;

namespace TestUnit
{
    ///<summary>
    ///Utilidades de uso general para manipulacion de la BD para pruebas.
    ///</summary>
    public class DbTestUtil(DbContext context)
    {
        ///<summary>
        ///Borra todas las filas de un conjunto de tablas especificadas en orden de detalle a maestro.
        ///Si updateAutoincrement es true reinicia la secuencia de campos autoincrementales de sqlite
        ///</summary>
        public void CleanTables(string[] tableNames, bool updateAutoincrement)
        {
            //Comprueba si puede existir alguna tabla autoincremental (si no existe, la tabla sqlite_secuence no se crea)
            string sqliteSequence = this.ExecuteQueryToCsv("SELECT name FROM sqlite_master WHERE type='table' and name='sqlite_sequence'", "");
            updateAutoincrement = sqliteSequence == "sqlite_sequence";

            foreach (string tableName in tableNames)
            {
                //ignora warning de potencial inyeccion sql porque aunque la segunda sentencia se puede parametrizar,
                //la primera no pues se trata de un identificador (habría que crear un procedimiento almacenado).
                //Esto es aceptable aqui pues es solamente para los tests
#pragma warning disable EF1000
                context.Database.ExecuteSqlRaw("DELETE FROM " + tableName);
                if (updateAutoincrement)
                    context.Database.ExecuteSqlRaw("UPDATE sqlite_sequence SET seq=0 WHERE name='" + tableName + "'");
#pragma warning restore EF1000
            }
        }

        ///<summary>
        ///Ejecuta un comando SQL de actualizacion
        ///</summary>
        public void ExecuteSqlCommand(string sql)
        {
             context.Database.ExecuteSqlRaw(sql);
        }

        ///<summary>
        ///Obtiene en formato csv (usando el separador especificado) todos los datos resultado de ejecutar una sentencia sql.
        ///Los valores nulos se muestran como string en blanco (implementado en FieldToCsv).
        ///Remplaza todas las comas en campos por un punto para evitar problemas con valores decimales
        ///que puedan ser dependientes de la localizacion (implementado en FieldToCsv)
        ///</summary>
        public string ExecuteQueryToCsv(string sql, string separator)
        {
            StringBuilder sb = new();
            using (DbConnection conn = context.Database.GetDbConnection())
            { 
                conn.Open();
                using (DbCommand stmt = conn.CreateCommand())
                {
                    stmt.CommandText = sql;
                    DbDataReader Dr = stmt.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                    while (Dr.Read())
                    {
                        sb.Append((sb.Length == 0 ? "" : "\n"));
                        for (int i = 0; i < Dr.VisibleFieldCount; i++)
                            sb.Append((i == 0 ? "" : separator) + FieldToCsv(Dr, i));
                    }
                }
            }
            return sb.ToString();
        }
        ///<summary>
        ///Obtiene en formato csv (usando coma como separador) todos los datos resultado de ejecutar una sentencia sql.
        ///Los valores nulos se muestran como string en blanco (implementado en FieldToCsv).
        ///Remplaza todas las comas en campos por un punto para evitar problemas con valores decimales
        ///que puedan ser dependientes de la localizacion (implementado en FieldToCsv)
        ///</summary>
        public string ExecuteQueryToCsv(string sql)
        {
            return ExecuteQueryToCsv(sql, ",");
        }
        private string FieldToCsv(DbDataReader dr, int ordinal)
        {
            if (dr.IsDBNull(ordinal))
                return "";
            else
                return dr.GetString(ordinal).Replace(",", ".");
        }
    }
}
