using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Xpress_backend_V2.Controllers
{
    [Route("/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Xpress backend API is live 🚀");
    }

}
