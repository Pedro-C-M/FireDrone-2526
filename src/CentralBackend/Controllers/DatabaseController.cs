using Microsoft.AspNetCore.Mvc;
namespace CentralBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        [HttpPost("clean")]
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

