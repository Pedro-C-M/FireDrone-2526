using System;
using System.Collections.Generic;
using System.IO;

namespace TestUnit
{
    ///<summary>
    ///Funcionalidad basica de la clase Properties de java.
    ///</summary>
    public class Properties
    {
        private Dictionary<string, string> properties;

        public Properties Load(string propFileName)
        {
            try
            {
                this.properties = LoadAllProperties(propFileName);
                return this;
            }
            catch (FileNotFoundException e)
            {
                throw new Exception("Can't load properties file " + propFileName, e);
            }
        }

        private static Dictionary<string, string> LoadAllProperties(string propFile)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(propFile);
            foreach (string line in lines)
            {
                if (line.Trim() == "" || line.Trim().StartsWith("#")) //ignora comentarios
                    continue;
                //Parte en nombre de propiedad y valor. Asume que no hay ningun caracter = en el valor
                System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex("=");
                string[] comp = rg.Split(line.Trim(), 2);
                if (comp.Length != 2)
                    throw new Exception("Invalid property specification: " + line);
                //busco si existe la propiedad (case sensitive)
                props.Add(comp[0].Trim(), comp[1].Trim());
            }
            return props;
        }

        ///<summary>
        ///Lee el valor de una propiedad de un fichero .properties (estilo Java).
        ///</summary>
        public string GetProperty(string propName)
        {
            return properties.ContainsKey(propName) ? properties[propName] : null;
        }

        ///<summary>
        ///Lee una propiedad, si o existe establece un valor por defecto
        ///</summary>
        public string GetProperty(string propName, string defaultValue)
        {
            string prop = GetProperty(propName);
            return prop ?? defaultValue;
        }
    }
}