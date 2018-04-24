using Microsoft.AspNetCore.Mvc;

namespace Diov.Web
{
    public class ErrorController : Controller
    {
        [HttpGet("/error/{error}")]
        public IActionResult Detail(string error)
        {
            return View(error);
        }
    }
}