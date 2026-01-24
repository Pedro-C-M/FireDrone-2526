using CentralBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace CentralBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly RouteService _service;

        public RouteController(RouteService service)
        {
            _service = service;
        }

        // GET: api/routes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Route>>> GetAll()
        {
            try
            {
                // El servicio debe incluir los Puntos (Include(r => r.Coords))
                var routes = await _service.GetAllAsync();
                return Ok(routes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RouteController] Error getting routes: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/routes/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportRoutes(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                Console.WriteLine($"[RouteController] Importing routes from file: {file.FileName}");

                using (var stream = file.OpenReadStream())
                {
                    // Delegamos la lógica compleja de parseo al servicio
                    int count = await _service.ImportFromCsvAsync(stream, file.FileName);

                    Console.WriteLine($"[RouteController] Successfully imported {count} routes.");
                    return Ok(new { message = $"Successfully imported {count} routes." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RouteController] Error importing routes: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound($"Route with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                // Devolvemos 409 Conflict o 400 Bad Request con el mensaje del error
                return StatusCode(409, new { error = ex.Message });
            }
        }
    }
}