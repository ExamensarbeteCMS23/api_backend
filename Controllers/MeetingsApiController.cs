using Microsoft.AspNetCore.Mvc;

namespace api_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsApiController : ControllerBase
    {
        [HttpGet]  
        public IActionResult Index()
        {
            return Ok("API fungerar!");
        }
    }
}
