using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CodeRedCreations.Controllers
{
    public class ErrorController : Controller
    {
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        [Route("Error/401")]
        public IActionResult Unauthorized()
        {
            return View();
        }

        [Route("Error/403")]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Route("Error/404")]
        public IActionResult NotFound()
        {
            return View();
        }

    }
}
