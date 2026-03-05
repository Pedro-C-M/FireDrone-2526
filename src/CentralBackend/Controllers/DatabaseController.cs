using Microsoft.AspNetCore.Mvc;
namespace CentralBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        /**
         * Método usado para limpiar y resetear la base de datos a su estado inicial. 
         * Esto es útil para pruebas o para reiniciar el sistema sin tener que 
         * eliminar manualmente los datos.
         */
        [HttpPost("clean")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult CleanDatabase([FromQuery] int? drones)
        {
            try
            {
                // Limpiar y resetear la base de datos usando la utilidad existente
                if (drones.HasValue)
                {
                    InstanciateBD.FormaBaseDeBD(drones.Value);
                    return Ok($"Database cleaned and reseeded successfully with {drones.Value} drones.");
                }
                else
                {
                    InstanciateBD.FormaBaseDeBD();
                    return Ok("Database cleaned and reseeded successfully.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while cleaning the database: {ex.Message}");
            }
        }
    }
}

