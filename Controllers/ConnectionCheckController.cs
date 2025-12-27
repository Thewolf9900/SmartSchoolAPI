using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;

namespace SmartSchoolAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionCheckController : ControllerBase
    {
        private readonly SmartSchoolDbContext _context;
        private readonly ILogger<ConnectionCheckController> _logger;

        public ConnectionCheckController(SmartSchoolDbContext context, ILogger<ConnectionCheckController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                _logger.LogInformation("Checking database connection...");
                
                // ببساطة نحاول التحقق مما إذا كان الاتصال متاحًا
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new 
                    { 
                        Status = "Connected", 
                        Message = "Successfully connected to the database.",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new 
                    { 
                        Status = "Failed", 
                        Message = "Could not connect to the database (CanConnectAsync returned false).",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection check failed.");
                return StatusCode(500, new 
                { 
                    Status = "Error", 
                    Message = $"Connection failed with exception: {ex.Message}", 
                    Details = ex.InnerException?.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
