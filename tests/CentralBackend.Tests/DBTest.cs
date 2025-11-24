using CentralBackend;

namespace CentralBackend.Tests
{
    [TestClass]
    public class DBTest
    {
        [TestMethod]
        public void InicializarBaseDeDatos_ComoSeedData()
        {
            InstanciateBD.FormaBaseDeBD();
        }
    }
}
