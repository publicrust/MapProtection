using Library;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapProtectionController : ControllerBase
    {
        private readonly ILogger<MapProtectionController> _logger;

        public MapProtectionController(ILogger<MapProtectionController> ILogger)
        {
            _logger = ILogger;
        }

        [HttpPost("protect")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ProtectMap([FromForm] IFormFile mapFile, [FromForm] MapProtectOptions options)
        {
            if (mapFile == null || mapFile.Length == 0)
            {
                return BadRequest("Map file is required.");
            }

            // Проверка расширения файла
            var extension = Path.GetExtension(mapFile.FileName).ToLower();
            if (extension != ".map")
            {
                return BadRequest("Only .map files are allowed.");
            }

            try
            {
                var name = Path.GetFileNameWithoutExtension(mapFile.FileName);

                using (var mapStream = new MemoryStream())
                {
                    await mapFile.CopyToAsync(mapStream);
                    mapStream.Position = 0;

                    var map = new Map();
                    var result = map.Protect(name + ".map", mapStream, options);

                    var pluginBytes = Encoding.UTF8.GetBytes(result.Plugin);

                    return File(pluginBytes, "application/octet-stream", $"MapProtection.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while protecting the map.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
