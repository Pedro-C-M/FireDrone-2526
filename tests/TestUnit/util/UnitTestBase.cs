using Giis.Visualassert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace TestUnit
{
    /// <summary>
    /// Clase base para todos los test unitarios.
    /// Provee de metodos estandar de inicializacion y finalizacion de las pruebas que gestionan el contexto e
    /// instancian objetos de utilidad:
    /// -context: el contexto de BD para las pruebas
    /// -util: algunas utilidades para manejar la base de datos
    /// -va: componente VisualAssert para comparacion en html de strings largos
    ///  (ver https://github.com/javiertuya/visual-assert)
    /// </summary>
    public class UnitTestBase
    {
        protected DbTestContext context; //el contexto de la BD que debe usarse en todas las subclases
        protected DbTestUtil util; //otras utilidades de acceso a la base de datos para uso en las subclases
        protected VisualAssert va; //para comparaciones de strings largos que muestren las diferencias html

        [TestInitialize] public virtual void SetUp()
        {
            context = new DbTestContext(); //cada proyecto tendra uno diferente
            util = new DbTestUtil(context);
            //configura VisualAssert para que escriba en la carpeta reports de la solucion
            //y muestre link local para VisualStudio cuando no se ejcuta desde Jenkins
            va = new VisualAssert().SetNormalizeEol(true)
                .SetReportSubdir(Path.Combine(Config.ProjectRoot, "reports"))
                .SetUseLocalAbsolutePath(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_HOME")));
            context.CleanDatabase(); //asegura entorno limpio para cada test
        }
        [TestCleanup] public virtual void TearDown()
        {
            context.Dispose();
        }

    }
}
