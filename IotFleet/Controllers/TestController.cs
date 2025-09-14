using Microsoft.AspNetCore.Mvc;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Test controller is working", timestamp = DateTime.UtcNow });
        }
    }
}

